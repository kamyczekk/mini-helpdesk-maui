using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiniHelpDesk.Tests;

/* Klasa: TestReport. Cel: wspólny wynik runnera dla okna MAUI i konsoli. */
public sealed record TestReport(int Passed, int Failed, string Text);

/* Klasa: TestRunner. Cel: uruchamianie metod oznaczonych TestMethod bez dodatkowego csproj.
   Nie jest to standardowy dotnet test; asercje i atrybuty są oryginalnym MSTest. */
public static class TestRunner
{
    /* Metoda: Run. Wynik: raport ze wszystkich testów; nowy obiekt klasy dla każdej metody. */
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(LogicTests))]
    public static TestReport Run()
    {
        var text = new StringBuilder(); var passed = 0; var failed = 0;
        foreach (var method in typeof(LogicTests).GetMethods().Where(m => m.GetCustomAttribute<TestMethodAttribute>() is not null).OrderBy(m => m.Name))
        {
            try { method.Invoke(new LogicTests(), null); passed++; text.AppendLine($"OK  {method.Name}"); }
            catch (Exception e)
            {
                failed++; var error = e is TargetInvocationException { InnerException: not null } invocation ? invocation.InnerException : e;
                text.AppendLine($"BŁĄD  {method.Name}: {error!.Message}");
            }
        }
        var summary = $"MSTest — zaliczone: {passed}, błędy: {failed}\n\n";
        return new(passed, failed, summary + text);
    }
}
