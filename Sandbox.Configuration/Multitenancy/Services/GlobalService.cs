namespace Sandbox.Configuration.Multitenancy.Services;

public interface IGlobalService
{
    string GetGlobalValue();
}

public class GlobalService : IGlobalService
{
    private readonly string _stateValue = Random.Shared.Next().ToString();

    public GlobalService()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"GlobalService created");
        Console.ResetColor();
    }

    public string GetGlobalValue() => $"Global value is {_stateValue}";
}