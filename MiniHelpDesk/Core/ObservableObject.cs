using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MiniHelpDesk.Core;

/* Klasa: ObservableObject. Cel: wspólna implementacja Observer dla modeli i formularza.
   Odpowiedzialność: powiadamianie Binding tylko po rzeczywistej zmianie wartości. */
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /* Metoda: Set. Parametry: pole, nowa wartość, nazwa właściwości.
       Wynik: true po zmianie; emituje PropertyChanged, false dla identycznej wartości. */
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        Notify(name);
        return true;
    }

    /* Metoda: Notify. Parametr: nazwa właściwości. Wynik: zdarzenie Observer. */
    protected void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
