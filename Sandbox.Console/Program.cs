// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using System.Reflection;


Console.WriteLine(new Executer().Execute());


public class Executer()
{
    public string Execute()
    {
        using var command = new Command(() => "Hello");
        return ExecuteCommand(command);
    }
    
    private static string ExecuteCommand(Command command)
    {
        return command.Execute();
    }
}

public class Command(Func<string> func) : IDisposable
{
    public string Execute() => func();

    public void Dispose() => Console.WriteLine("Dispose command");
}


public class TestDispose : IDisposable
{
    public void Dispose()
    {
        // TODO release managed resources here
    }
}

public enum Suits
{
    [Description("Derevo")] Wood,
    [Description("Voda")] Water
}

public static class SuitsExtensions
{
    public static string GetDescription(this Suits value)
    {
        // Получаем тип перечисления
        Type type = value.GetType();

        // Получаем информацию о поле, соответствующем значению перечисления
        FieldInfo fieldInfo = type.GetField(value.ToString());

        // Получаем атрибут 'Description' для этого значения, если он есть
        DescriptionAttribute attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>(false);

        // Возвращаем описание или само значение перечисления, если описание отсутствует
        return attribute == null ? value.ToString() : attribute.Description;
    }
}