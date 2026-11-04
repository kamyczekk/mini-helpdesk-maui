namespace MiniHelpDesk.Tests;

/* Klasa: ConsoleRunner. Cel: konsolowy tryb tych samych testów w tym samym csproj.
   Kompilowana wyłącznie z HeadlessTests=true; aplikacja MAUI ma własne punkty wejścia. */
internal static class ConsoleRunner
{
    /* Metoda: Main. Wynik: 0 po sukcesie, 1 po błędzie; drukuje pełny raport MSTest. */
    public static int Main()
    {
        var report = TestRunner.Run(); Console.WriteLine(report.Text);
        return report.Failed == 0 && report.Passed > 0 ? 0 : 1;
    }
}
