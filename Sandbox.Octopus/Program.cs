// See https://aka.ms/new-console-template for more information

#pragma warning disable VSTHRD200
#pragma warning disable CA2007
#pragma warning disable CS0618 // Type or member is obsolete
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Octopus.Client;
using Octopus.Client.Model;

var server = "";
var apiKey = "";

var endpoint = new OctopusServerEndpoint(server, apiKey);

// Stable, Beta
var statuses = await GetStatuses("Stable", 80);

var stats = GetDeployStepStatusStats(statuses);

//ShowInConsole(statuses);
PrintDeployStepStatusStats(stats);

WriteCsvStatuses(statuses.OrderByDescending(x => x.Duration));
WriteCsvStepsStat(stats);

void WriteCsvStatuses(IEnumerable<DeployStatus> statuses)
{
    using var writer = new StreamWriter(@"c:\repo3\statuses_stat.csv");
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    csv.Context.RegisterClassMap<DeployStatusMap>();
    csv.WriteRecords(statuses);
}

void WriteCsvStepsStat(IEnumerable<DeployStepStatusStat> stats)
{
    using var writer = new StreamWriter(@"c:\repo3\steps_stat.csv");
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    csv.Context.RegisterClassMap<DeployStepStatusStatCsvMap>();
    csv.WriteRecords(stats);
}

void PrintDeployStepStatusStats(IEnumerable<DeployStepStatusStat> stats)
{
    foreach (var status in stats)
    {
        Console.WriteLine($"Name: {status.Name}");
        Console.WriteLine($"\t\t90th percentile: {status.Percentile}");
        Console.WriteLine($"\t\tAvg duration: {status.AverageDuration}");
    }
}

void PrintDeployStatuses(IEnumerable<DeployStatus> statuses)
{
    foreach (var status in statuses)
    {
        Console.WriteLine($"Name: {status.Name}");
        Console.WriteLine($"Environment: {status.Environment}");
        Console.WriteLine($"Status: {status.Status}");
        Console.WriteLine($"TaskId: {status.TaskId}");
        Console.WriteLine($"Duration: {status.Duration}");
        Console.WriteLine($"ProjectsCount: {status.ProjectsCount}");
        Console.WriteLine("Steps:");
        foreach (var step in status.Steps)
        {
            Console.WriteLine($"\tName: {step.Name}");
            Console.WriteLine($"\tStatus: {step.Status}");
            Console.WriteLine($"\tDuration: {step.Duration}");
        }
    }
}

IEnumerable<DeployStepStatusStat> GetDeployStepStatusStats(IEnumerable<DeployStatus> deployStatus)
{
    var stepStats = deployStatus.SelectMany(d => d.Steps)
        .GroupBy(s => NormalizeStepName(s.Name))
        .Select(g =>
        {
            var durations = g.Where(s => s.Duration.HasValue).Select(s => s.Duration.Value).ToArray();

            return new DeployStepStatusStat(g.Key,
                RoundToSeconds(TimeSpan.FromSeconds(durations.Average(d => d.TotalSeconds)))!.Value,
                TimeSpan.FromSeconds(Calculate95Percentile(durations.Select(d => d.TotalSeconds).ToArray())),
                RoundToSeconds(durations.Max())!.Value
            );
        })
        .OrderByDescending(x => x.Percentile);

    return stepStats.ToArray();
}

async Task<IEnumerable<DeployStatus>> GetStatuses(string env, int count, bool skipForSingleProject = false)
{
    var statuses = new List<DeployStatus>(count);
    using var client = await OctopusAsyncClient.Create(endpoint);

    var project = await client.Repository.Projects.FindByName("DirectCRM");
    var environment = await client.Repository.Environments.FindByName(env);
    var deployments = await client.Repository.Deployments
        .FindBy([project.Id], [environment.Id], skip: 0, take: count);

    var allowedTaskState = new[] { TaskState.Success, TaskState.Failed, TaskState.Canceled, TaskState.TimedOut };

    foreach (var deployment in deployments.Items)
    {
        var task = await client.Repository.Tasks.Get(deployment.TaskId);

        if (!allowedTaskState.Contains(task.State))
            continue;

        var details = await client.Repository.Tasks.GetDetails(task, includeVerboseOutput: true, tail: null);

        int? projectsCount = null;
        var steps = new List<DeployStepStatus>(details.ActivityLogs.Count);
        foreach (var activityLog in details.ActivityLogs)
        {
            foreach (var activityLogChild in activityLog.Children)
            {
                if (activityLogChild.Name.Contains("Step 4"))
                {
                    projectsCount = GetProjectsCount(activityLogChild);
                    if (projectsCount == 0 || (projectsCount == 1 && skipForSingleProject))
                        continue;
                }

                TimeSpan? stepDuration = null;
                if (activityLogChild is { Ended: not null, Started: not null })
                {
                    stepDuration = RoundToSeconds((activityLogChild.Ended - activityLogChild.Started));
                }

                var step = new DeployStepStatus(activityLogChild.Name, activityLogChild.Status.ToString(),
                    RoundToSeconds(stepDuration));
                steps.Add(step);
            }
        }

        TimeSpan? deployDuration = null;
        if (task is { CompletedTime: not null, StartTime: not null })
        {
            deployDuration = task.CompletedTime - task.StartTime;
        }

        var deployStatus = new DeployStatus(
            deployment.Name,
            environment.Name,
            task.State.ToString(),
            task.Id,
            RoundToSeconds(deployDuration),
            projectsCount,
            steps);

        statuses.Add(deployStatus);
    }

    return statuses;
}

int GetProjectsCount(ActivityElement activityElement)
{
    if (!activityElement.Children.Any())
        return 0;

    var log = activityElement
        .Children
        .Single()
        .LogElements
        .FirstOrDefault(x => x.MessageText.StartsWith("Deploying projects:"))
        ?.MessageText
        .Replace("Deploying projects: ", string.Empty)
        .Trim() ?? String.Empty;

    return log.Split(',').Length;
}

static TimeSpan? RoundToSeconds(TimeSpan? timeSpan)
{
    return timeSpan is not null
        ? TimeSpan.FromSeconds(Math.Round(timeSpan.Value.TotalSeconds))
        : null;
}

static double Calculate95Percentile(double[] data)
{
    Array.Sort(data);
    var percentileIndex = (int)Math.Ceiling(data.Length * 0.95) - 1;
    return data[percentileIndex];
}

static string NormalizeStepName(string stepName)
{
    var parts = stepName.Split(':');
    return parts.Length > 1 ? parts[1].Trim() : stepName;
}

record DeployStatus(
    string Name,
    string Environment,
    string Status,
    string TaskId,
    TimeSpan? Duration,
    int? ProjectsCount,
    IEnumerable<DeployStepStatus> Steps);

record DeployStepStatus(string Name, string Status, TimeSpan? Duration);

record DeployStepStatusStat(string Name, TimeSpan AverageDuration, TimeSpan Percentile, TimeSpan MaxDuration);

class DeployStatusMap : ClassMap<DeployStatus>
{
    public DeployStatusMap()
    {
        Map(m => m.Name).Name("name");
        Map(m => m.Environment).Name("environment");
        Map(m => m.Status).Name("status");
        Map(m => m.TaskId).Name("task_id");
        Map(m => m.Duration).Name("duration");
        Map(m => m.ProjectsCount).Name("projects_count");
    }
}

class DeployStepStatusStatCsvMap : ClassMap<DeployStepStatusStat>
{
    public DeployStepStatusStatCsvMap()
    {
        Map(m => m.Name).Name("name");
        Map(m => m.Percentile).Name("precentile");
        Map(m => m.MaxDuration).Name("max");
        Map(m => m.AverageDuration).Name("avg");
    }
}