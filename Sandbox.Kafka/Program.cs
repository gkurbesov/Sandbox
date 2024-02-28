#pragma warning disable VSTHRD200
using Confluent.Kafka;
using Confluent.Kafka.Admin;

var bootstrapServers = "eu-aws-kafka-transactional-outbox-production-a-1-v1.corp.itcd.ru:9092,eu-aws-kafka-transactional-outbox-production-b-1-v1.corp.itcd.ru:9092,eu-aws-kafka-transactional-outbox-production-c-1-v1.corp.itcd.ru:9092";
var isTestRun = true;
var batchSize = 100;


var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

using (var adminClient = new AdminClientBuilder(config).Build())
{
    try
    {
        var groups = await adminClient.ListConsumerGroupsAsync();
        var groupsToRemove = groups.Valid
            .Where(TopicMatcher.IsConsumerGroupRemoved)
            .Select(group => group.GroupId)
            .ToArray();

        var log = string.Join(Environment.NewLine, groupsToRemove);
        Console.WriteLine("This consumer group will be deleted");
        Console.WriteLine(log);
        if (isTestRun)
        {
            Console.WriteLine("IT IS TEST RUN NOTHING HAS BEEN APPLIED");
        }
        else
        {
            foreach (var batch in groupsToRemove.Chunk(batchSize))
            {
                try
                {
                    await adminClient.DeleteGroupsAsync(batch);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred while deleting batch: {e.Message}");
                }
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Exception occurred: {e.Message}");
    }
}

public class TopicMatcher
{
    public static string[] RemovedStrategies = new string[]
    {
        "SlaImportsStateMessageOutboxHandler",
        "ProcessingOrdersOutboxProcessingTask",
        "SlaMailingStopMonitoringOutboxHandler",
        "SlaMailingMessageProcessedOutboxHandler",
        "SlaImportsProcessingMessageOutboxHandler"
    };

    public static bool IsConsumerGroupRemoved(ConsumerGroupListing group)
    {
        return RemovedStrategies.Any(s => group.GroupId.EndsWith($"TransactionalOutbox.Committed.{s}")) 
               && group.State == ConsumerGroupState.Empty;
    }
}
#pragma warning restore VSTHRD200
