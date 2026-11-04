using Microsoft.Extensions.DependencyInjection;

namespace MiniHelpDesk;

/* Klasa: App. Cel: punkt wejścia wspólnej aplikacji MAUI na obu platformach. */
public partial class App : Application
{
    private readonly IServiceProvider _services;
    /* Konstruktor: parametr kontener DI. Wynik: zasoby XAML gotowe przed utworzeniem strony. */
    public App(IServiceProvider services) { InitializeComponent(); _services = services; }
    /* Metoda: CreateWindow. Parametr: stan aktywacji. Wynik: jedno okno aplikacji. */
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_services.GetRequiredService<MainPage>()) { Title = "Mini HelpDesk" };
}
