using System.Collections.ObjectModel;

namespace MiniHelpDesk.Core;

/* Interfejs: ICommand. Cel: znacznik operacji zmieniającej stan. */
public interface ICommand;
/* Klasa: CreateTicket. Cel: dane żądania dodania; priorytet nie ma wartości domyślnej. */
public sealed record CreateTicket(string? Name, string? Description, TicketPriority? Priority) : ICommand;
/* Klasa: EditTicket. Cel: dane edycji istniejącego Id. */
public sealed record EditTicket(Guid Id, string? Name, string? Description, TicketPriority? Priority) : ICommand;
/* Klasa: DeleteTicket. Cel: żądanie usunięcia wskazanego Id. */
public sealed record DeleteTicket(Guid Id) : ICommand;
/* Klasa: ChangeTicketStatus. Cel: jawna zmiana statusu. */
public sealed record ChangeTicketStatus(Guid Id, TicketStatus Status) : ICommand;

/* Klasa: CommandResult. Cel: wynik zapisu oraz błędy pól lub operacji. */
public sealed record CommandResult(bool Success, Guid? Id = null, ValidationResult? Validation = null, string Error = "");

/* Interfejs: ICommandHandler. Cel: typowany kontrakt wykonania komendy. */
public interface ICommandHandler<in T> where T : ICommand
{
    /* Metoda: Handle. Parametr: komenda. Wynik: powodzenie lub błędy bez zapisu. */
    CommandResult Handle(T command);
}

/* Klasa: CommandBus. Cel: kierowanie zapisów do zarejestrowanych handlerów.
   Odpowiedzialność: transport komend, bez odczytu i bez logiki formularza. */
public sealed class CommandBus
{
    private readonly Dictionary<Type, Func<ICommand, CommandResult>> _handlers = new();

    /* Metoda: Register. Parametr: handler danego typu. Wynik: rejestracja dokładnie raz.
       Wyjątki: ArgumentException przy ponownej rejestracji. */
    public void Register<T>(ICommandHandler<T> handler) where T : ICommand =>
        _handlers.Add(typeof(T), command => handler.Handle((T)command));

    /* Metoda: Send. Parametr: komenda. Wynik: rezultat właściwego handlera.
       Wyjątki: InvalidOperationException, jeżeli nie zarejestrowano handlera. */
    public CommandResult Send<T>(T command) where T : ICommand =>
        _handlers.TryGetValue(command.GetType(), out var handler)
            ? handler(command) : throw new InvalidOperationException("Brak handlera komendy.");
}

/* Klasa: CreateTicketHandler. Cel: walidowany zapis nowego zgłoszenia. */
public sealed class CreateTicketHandler(ITicketRepository repository) : ICommandHandler<CreateTicket>
{
    /* Metoda: Handle. Parametr: pola tworzenia. Wynik: nowe Id lub błędy; status zawsze Nowe. */
    public CommandResult Handle(CreateTicket command)
    {
        var v = TicketValidator.Validate(command.Name, command.Description, command.Priority);
        if (!v.IsValid) return new(false, Validation: v);
        var ticket = new Ticket(v.Name, v.Description, command.Priority!.Value);
        repository.Add(ticket);
        return new(true, ticket.Id);
    }
}

/* Klasa: EditTicketHandler. Cel: atomowa walidacja przed zmianą istniejącego zgłoszenia. */
public sealed class EditTicketHandler(ITicketRepository repository) : ICommandHandler<EditTicket>
{
    /* Metoda: Handle. Parametr: Id i nowe pola. Wynik: zmiana danych lub błąd; Id/status bez zmian. */
    public CommandResult Handle(EditTicket command)
    {
        var v = TicketValidator.Validate(command.Name, command.Description, command.Priority);
        if (!v.IsValid) return new(false, Validation: v);
        var ticket = repository.Find(command.Id);
        if (ticket is null) return new(false, Error: "Zgłoszenie już nie istnieje.");
        ticket.Edit(v.Name, v.Description, command.Priority!.Value);
        return new(true, ticket.Id);
    }
}

/* Klasa: DeleteTicketHandler. Cel: usunięcie zgłoszenia wskazanego przez Id. */
public sealed class DeleteTicketHandler(ITicketRepository repository) : ICommandHandler<DeleteTicket>
{
    /* Metoda: Handle. Parametr: Id. Wynik: powodzenie albo informacja o braku zgłoszenia. */
    public CommandResult Handle(DeleteTicket command) => repository.Remove(command.Id)
        ? new(true, command.Id) : new(false, Error: "Zgłoszenie już nie istnieje.");
}

/* Klasa: ChangeStatusHandler. Cel: zmiana statusu z kontrolą wartości enum. */
public sealed class ChangeStatusHandler(ITicketRepository repository) : ICommandHandler<ChangeTicketStatus>
{
    /* Metoda: Handle. Parametr: Id i status. Wynik: zmiana lub błąd bez modyfikacji. */
    public CommandResult Handle(ChangeTicketStatus command)
    {
        if (!Enum.IsDefined(command.Status)) return new(false, Error: "Niepoprawny status.");
        var ticket = repository.Find(command.Id);
        if (ticket is null) return new(false, Error: "Zgłoszenie już nie istnieje.");
        ticket.ChangeStatus(command.Status);
        return new(true, ticket.Id);
    }
}

/* Klasa: GetTicketsQuery. Cel: żądanie odczytu listy bez zmiany danych. */
public sealed record GetTicketsQuery;

/* Klasa: GetTicketsHandler. Cel: niezależna ścieżka Queries w CQRS. */
public sealed class GetTicketsHandler(ITicketRepository repository)
{
    /* Metoda: Handle. Parametr: zapytanie. Wynik: wspólny, obserwowalny widok tylko do odczytu. */
    public ReadOnlyObservableCollection<Ticket> Handle(GetTicketsQuery query) => repository.Items;
}

/* Klasa: HelpDeskServices. Cel: składanie repozytorium, busa i handlerów w jednym miejscu. */
public sealed class HelpDeskServices
{
    public CommandBus Commands { get; } = new();
    public GetTicketsHandler Queries { get; }

    /* Konstruktor: parametr opcjonalne repozytorium. Wynik: komplet zarejestrowanych usług.
       Brak parametru tworzy nową, niezależną instancję danych (także dla testów). */
    public HelpDeskServices(ITicketRepository? repository = null)
    {
        repository ??= new InMemoryTicketRepository();
        Commands.Register(new CreateTicketHandler(repository));
        Commands.Register(new EditTicketHandler(repository));
        Commands.Register(new DeleteTicketHandler(repository));
        Commands.Register(new ChangeStatusHandler(repository));
        Queries = new(repository);
    }
}
