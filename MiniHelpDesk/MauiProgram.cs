using Microsoft.Extensions.DependencyInjection;
using MiniHelpDesk.Core;
using MiniHelpDesk.ViewModels;

namespace MiniHelpDesk;

/* Klasa: MauiProgram. Cel: wspólny composition root oraz rejestracja usług DI. */
public static class MauiProgram
{
    /* Metoda: CreateMauiApp. Wynik: skonfigurowana aplikacja z jednym repozytorium RAM. */
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<App>();
        builder.Services.AddSingleton<HelpDeskServices>();
        builder.Services.AddSingleton<HelpDeskViewModel>();
        builder.Services.AddSingleton<MainPage>();
        return builder.Build();
    }
}
