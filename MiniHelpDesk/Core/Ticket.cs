namespace MiniHelpDesk.Core;

/* Typ: TicketPriority. Cel: zamknięty zestaw dopuszczalnych priorytetów. */
public enum TicketPriority { Low, Medium, High }

/* Typ: TicketStatus. Cel: zamknięty zestaw dopuszczalnych statusów. */
public enum TicketStatus { New, InProgress, Closed }

/* Klasa: Ticket. Cel: obserwowalny model zgłoszenia.
   Odpowiedzialność: stałe Id i zmiany właściwości dokonywane wyłącznie przez logikę zapisu. */
public sealed class Ticket : ObservableObject
{
    private string _name;
    private string _description;
    private TicketPriority _priority;
    private TicketStatus _status;
    public Guid Id { get; }
    public string Name { get => _name; private set => Set(ref _name, value); }
    public string Description { get => _description; private set => Set(ref _description, value); }
    public TicketPriority Priority
    {
        get => _priority;
        private set { if (Set(ref _priority, value)) Notify(nameof(PriorityLabel)); }
    }
    public TicketStatus Status
    {
        get => _status;
        private set { if (Set(ref _status, value)) Notify(nameof(StatusLabel)); }
    }
    public string PriorityLabel => Labels.Priority(Priority);
    public string StatusLabel => Labels.Status(Status);
    public string ShortId => Id.ToString("N")[..8].ToUpperInvariant();

    /* Konstruktor: Ticket. Parametry: poprawne pola. Wynik: nowe Guid i status Nowe.
       Dostęp: internal; publiczny zapis przechodzi przez Command Bus i walidację. */
    internal Ticket(string name, string description, TicketPriority priority)
    {
        Id = Guid.NewGuid();
        _name = name; _description = description; _priority = priority;
        _status = TicketStatus.New;
    }

    /* Metoda: Edit. Parametry: zatwierdzone pola. Wynik: zmiana w miejscu bez zmiany Id/statusu. */
    internal void Edit(string name, string description, TicketPriority priority)
    { Name = name; Description = description; Priority = priority; }

    /* Metoda: ChangeStatus. Parametr: poprawny status. Wynik: powiadomienie widoku. */
    internal void ChangeStatus(TicketStatus status) => Status = status;
}

/* Klasa: Labels. Cel: polskie etykiety wspólne dla obu platform. */
public static class Labels
{
    /* Metoda: Priority. Parametr: priorytet. Wynik: polska nazwa. */
    public static string Priority(TicketPriority value) => value switch
    { TicketPriority.Low => "Niski", TicketPriority.Medium => "Średni", TicketPriority.High => "Wysoki", _ => "Nieznany" };

    /* Metoda: Status. Parametr: status. Wynik: polska nazwa. */
    public static string Status(TicketStatus value) => value switch
    { TicketStatus.New => "Nowe", TicketStatus.InProgress => "W trakcie", TicketStatus.Closed => "Zamknięte", _ => "Nieznany" };
}
