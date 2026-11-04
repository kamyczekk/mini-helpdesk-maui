using Android.App;
using Android.Runtime;

namespace MiniHelpDesk;

/* Klasa: MainApplication. Cel: uruchomienie kontenera MAUI w procesie Android. */
[Application]
public class MainApplication : MauiApplication
{
    /* Konstruktor: parametry natywnego uchwytu Android. Wynik: inicjalizacja aplikacji. */
    public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }
    /* Metoda: CreateMauiApp. Wynik: wspólna konfiguracja, identyczna jak na Windows. */
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
