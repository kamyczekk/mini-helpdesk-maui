namespace MiniHelpDesk.WinUI;

/* Klasa: App. Cel: natywny punkt wejścia WinUI do wspólnej aplikacji MAUI. */
public partial class App : MauiWinUIApplication
{
    /* Konstruktor: inicjalizacja zasobów WinUI. */
    public App() => InitializeComponent();
    /* Metoda: CreateMauiApp. Wynik: ta sama logika i XAML co w Androidzie. */
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
