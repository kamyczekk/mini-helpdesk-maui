using System.Collections.ObjectModel;

namespace MiniHelpDesk.Core;

/* Interfejs: ITicketRepository. Cel: kontrakt magazynu pamięciowego.
   Odczyt udostępnia obserwowalny widok bez publicznych metod modyfikacji kolekcji. */
public interface ITicketRepository
{
    ReadOnlyObservableCollection<Ticket> Items { get; }
    /* Metoda: Find. Parametr: Id. Wynik: zgłoszenie albo null. */
    Ticket? Find(Guid id);
    /* Metoda: Add. Parametr: nowe zgłoszenie. Wynik: dodanie i zdarzenie kolekcji. */
    void Add(Ticket ticket);
    /* Metoda: Remove. Parametr: Id. Wynik: true po usunięciu, false gdy brak. */
    bool Remove(Guid id);
}

/* Klasa: InMemoryTicketRepository. Cel: przechowywanie danych w RAM.
   Odpowiedzialność: ObservableCollection; instancja produkcyjna żyje przez czas działania aplikacji. */
public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly ObservableCollection<Ticket> _items = new();
    public ReadOnlyObservableCollection<Ticket> Items { get; }

    /* Konstruktor: tworzy pojedynczy obserwowalny widok kolekcji. */
    public InMemoryTicketRepository() => Items = new(_items);

    /* Metoda: Find. Parametr: Id. Wynik: istniejący obiekt lub null. */
    public Ticket? Find(Guid id) => _items.FirstOrDefault(t => t.Id == id);

    /* Metoda: Add. Parametr: zgłoszenie. Wynik: dodanie do kolekcji.
       Wyjątki: InvalidOperationException dla powtórzonego Id. */
    public void Add(Ticket ticket)
    {
        if (Find(ticket.Id) is not null) throw new InvalidOperationException("Id musi być unikatowe.");
        _items.Add(ticket);
    }

    /* Metoda: Remove. Parametr: Id. Wynik: czy usunięto element. */
    public bool Remove(Guid id)
    {
        var ticket = Find(id);
        return ticket is not null && _items.Remove(ticket);
    }
}
