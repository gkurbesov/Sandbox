// See https://aka.ms/new-console-template for more information

#pragma warning disable VSTHRD200
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

Console.OutputEncoding = System.Text.Encoding.UTF8;


string bootstrapServers = "";
string topicName = ""; // Имя искомого топика

var config = new AdminClientConfig { BootstrapServers = bootstrapServers };
using var adminClient = new AdminClientBuilder(config).Build();


var companion = new KafkaAlertDutyCompanion(bootstrapServers);

var handler = new KafkaMessageHandler(TimeSpan.FromHours(5));
await companion.ConsumeTopicAsync(topicName,
    async message => handler.HandleMessage(message),
    messageLimit: 10);


var messages = handler.GetMessages();


Console.WriteLine($"Messages count: {messages.Count()}");

//var message = messages.FirstOrDefault();
foreach (var message in messages)
{
    ShowKafkaMessage(message);
}

void ShowKafkaMessage(KafkaMessage message)
{
    if (message != null)
    {
        Console.WriteLine($"HeaderKeys: {string.Join(", ", message.HeaderKeys)}");
        Console.WriteLine($"MissingTypes: {string.Join(", ", message.MissingTypes)}");
        Console.WriteLine($"Bounces: {message.Value.Bounces}");
        Console.WriteLine($"ProposedDelay: {message.Value.ProposedDelay}");
        Console.WriteLine($"OriginTopic: {message.Value.OriginTopic}");

        foreach (var (type, count) in message.TypeAndCount)
        {
            Console.WriteLine($"\tType: {type}, Count: {count}");
        }
    }
}


public class KafkaAlertDutyCompanion
{
    private readonly string _kafkaBootstrapServers;

    public KafkaAlertDutyCompanion(string kafkaBootstrapServers)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
    }

    public bool TopicIsExist(string topic)
    {
        var config = new AdminClientConfig { BootstrapServers = _kafkaBootstrapServers };
        using var adminClient = new AdminClientBuilder(config).Build();

        // Получаем список всех топиков
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var topics = metadata.Topics.Select(t => t.Topic).ToList();

        return topics.SingleOrDefault(t => t == topic) != null;
    }


    public async Task ConsumeTopicAsync(
        string topicName,
        Func<KafkaMessage, Task> handler,
        int? messageLimit = null,
        TimeSpan? timeout = null)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Latest,
            GroupId = "KafkaAlertDutyCompanion",
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(topicName);

        try
        {
            using var limiter = new ConsumerLimiter(messageLimit, timeout);
            while (!limiter.IsLimitReached())
            {
                var consumeResult = consumer.Consume();
                limiter.SetConsumed();

                var delayedMessage = JsonSerializer.Deserialize<DelayedMessage>(consumeResult.Message.Value);

                var headerKeys = new HashSet<string>(consumeResult.Message.Headers.Select(x => x.Key));

                var typeAndCount = delayedMessage.GetTransactionalOutboxMessage().MessageItems
                    .Select(item => item.Type)
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .ToArray();

                var missingTypes = typeAndCount
                    .Select(item => item.Key)
                    .Where(type => !headerKeys.Contains(type))
                    .Distinct()
                    .ToArray();

                var kafkaMessage = new KafkaMessage()
                {
                    HeaderKeys = headerKeys,
                    MissingTypes = missingTypes,
                    TypeAndCount = typeAndCount,
                    Value = delayedMessage
                };

                await handler(kafkaMessage).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}

public class KafkaMessageHandler
{
    private readonly TimeSpan _maxProposedDelay;
    private readonly List<KafkaMessage> _messages = new();
    private readonly HashSet<int> bounces = new();
    private bool _isAnalyzed = false;

    public KafkaMessageHandler(TimeSpan maxProposedDelay)
    {
        _maxProposedDelay = maxProposedDelay;
    }

    public void HandleMessage(KafkaMessage message)
    {
        var bounce = message.Value.Bounces;

        if (message.Value.ProposedDelay < _maxProposedDelay)
        {
            if (bounce != null && !bounces.Contains(bounce.Value))
            {
                bounces.Add(bounce.Value);
                return;
            }
        }

        // Добавляем сообщение в список для последующего анализа
        _messages.Add(message);
    }

    public IEnumerable<KafkaMessage> GetMessages()
    {
        return _messages.OrderBy(x => x.Value.Bounces);
    }
}

public record ConsumerLimiter : IDisposable
{
    private readonly int? _messageLimit;
    private int _messageCount;
    private CancellationTokenSource _cancellationTokenSource;

    public ConsumerLimiter(int? messageLimit = null, TimeSpan? timeout = null)
    {
        if (!messageLimit.HasValue && !timeout.HasValue)
        {
            throw new ArgumentException("Either messageLimit or timeout must be set");
        }

        _messageLimit = messageLimit;

        if (timeout != null)
            _cancellationTokenSource = new CancellationTokenSource(timeout.Value);
    }

    public bool IsLimitReached()
    {
        var messageLimit = _messageCount >= _messageLimit;
        var timeoutLimit = _cancellationTokenSource?.IsCancellationRequested ?? false;
        return messageLimit || timeoutLimit;
    }

    public void SetConsumed()
    {
        _messageCount++;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}

public record KafkaMessage
{
    public HashSet<string> HeaderKeys { get; init; }
    public IEnumerable<string> MissingTypes { get; init; }

    public IEnumerable<(string, int)> TypeAndCount { get; init; }

    public DelayedMessage Value { get; init; }
}

public record DelayedMessage
{
    [JsonPropertyName("bounces")] public int? Bounces { get; set; }
    [JsonPropertyName("proposedDelay")] public TimeSpan ProposedDelay { get; set; }
    [JsonPropertyName("originTopic")] public string? OriginTopic { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; }

    private TransactionalOutboxMessage? _transactionalOutboxMessage;

    public TransactionalOutboxMessage? GetTransactionalOutboxMessage() =>
        _transactionalOutboxMessage ??= JsonSerializer.Deserialize<TransactionalOutboxMessage>(Message);
}

[DataContract]
public sealed record TransactionalOutboxMessage
{
    [JsonPropertyName("messageItems")] public TransactionalOutboxMessageItem[] MessageItems { get; init; }

    [JsonPropertyName("creationDateTimeUtc")]
    public DateTime CreationDateTimeUtc { get; init; }

    [JsonPropertyName("producedDateTimeUtc")]
    public DateTime ProducedDateTimeUtc { get; init; }

    [JsonPropertyName("committedDateTimeUtc")]
    public DateTime? CommitDateTimeUtc { get; init; }
}

[DataContract]
public sealed class TransactionalOutboxMessageItem
{
    [JsonPropertyName("message")] public object Message { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }
}
#pragma warning restore VSTHRD200