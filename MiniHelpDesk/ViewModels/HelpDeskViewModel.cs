using System.Collections.ObjectModel;
using MiniHelpDesk.Core;

namespace MiniHelpDesk.ViewModels;

/* Klasa: RelayCommand. Cel: ICommand warstwy UI bez zależności od platformy.
   Odpowiedzialność: wykonanie akcji tylko wtedy, gdy spełnia warunek CanExecute. */
public sealed class RelayCommand(Action action, Func<bool>? canExecute = null) : System.Windows.Input.ICommand
{
    public event EventHandler? CanExecuteChanged;
    /* Metoda: CanExecute. Parametr: nieużywany. Wynik: dostępność przycisku. */
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    /* Metoda: Execute. Parametr: nieużywany. Wynik: wykonanie dozwolonej akcji. */
    public void Execute(object? parameter) { if (CanExecute(parameter)) action(); }
    /* Metoda: Refresh. Wynik: ponowne sprawdzenie dostępności przez Binding. */
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/* Klasa: HelpDeskViewModel. Cel: wspólne wiązania dla Windows/Android.
   Odpowiedzialność: kopia robocza formularza, błędy pól, odczyt Queries i zapisy przez Bus. */
public sealed class HelpDeskViewModel : ObservableObject
{
    private readonly HelpDeskServices _services;
    private string _name = "", _description = "", _message = "Dane są przechowywane tylko podczas działania aplikacji.";
    private string _nameError = "", _descriptionError = "", _priorityError = "";
    private int _priorityIndex = -1, _statusIndex;
    private Ticket? _selectedTicket;
    private bool _formOpen;
    public ReadOnlyObservableCollection<Ticket> Tickets { get; }
    public string[] Priorities { get; } = ["Niski", "Średni", "Wysoki"];
    public string[] Statuses { get; } = ["Nowe", "W trakcie", "Zamknięte"];
    public RelayCommand SaveCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand StatusCommand { get; }
    public string Name { get => _name; set { if (Set(ref _name, value)) Validate(); } }
    public string Description { get => _description; set { if (Set(ref _description, value)) Validate(); } }
    public int PriorityIndex { get => _priorityIndex; set { if (Set(ref _priorityIndex, value)) Validate(); } }
    public int StatusIndex { get => _statusIndex; set { if (Set(ref _statusIndex, value)) StatusCommand.Refresh(); } }
    public string NameError { get => _nameError; private set => Set(ref _nameError, value); }
    public string DescriptionError { get => _descriptionError; private set => Set(ref _descriptionError, value); }
    public string PriorityError { get => _priorityError; private set => Set(ref _priorityError, value); }
    public string Message { get => _message; private set => Set(ref _message, value); }
    public bool FormOpen { get => _formOpen; private set => Set(ref _formOpen, value); }
    public bool IsEditing => SelectedTicket is not null;
    public string FormTitle => IsEditing ? "Edytuj zgłoszenie" : "Nowe zgłoszenie";
    public string SaveLabel => IsEditing ? "Zapisz zmiany" : "Dodaj zgłoszenie";
    public string CountLabel => $"ZGŁOSZENIA · {Tickets.Count}";
    public Ticket? SelectedTicket
    {
        get => _selectedTicket;
        set
        {
            if (!Set(ref _selectedTicket, value)) return;
            if (value is not null)
            {
                Name = value.Name; Description = value.Description;
                PriorityIndex = (int)value.Priority; StatusIndex = (int)value.Status;
                FormOpen = true;
            }
            Notify(nameof(IsEditing)); Notify(nameof(FormTitle)); Notify(nameof(SaveLabel));
            DeleteCommand.Refresh(); StatusCommand.Refresh();
        }
    }

    /* Konstruktor: parametr usługi wspólne dla aplikacji. Wynik: skonfigurowane Binding i komendy. */
    public HelpDeskViewModel(HelpDeskServices services)
    {
        _services = services;
        Tickets = services.Queries.Handle(new());
        SaveCommand = new(Save, () => CurrentValidation().IsValid);
        NewCommand = new(() => Reset(true));
        CancelCommand = new(() => Reset(false));
        DeleteCommand = new(Delete, () => IsEditing);
        StatusCommand = new(ChangeStatus, () => IsEditing && StatusIndex is >= 0 and <= 2);
        ((System.Collections.Specialized.INotifyCollectionChanged)Tickets).CollectionChanged += (_, _) => Notify(nameof(CountLabel));
        Validate();
    }

    /* Metoda: CurrentValidation. Wynik: sprawdzenie kopii roboczej bez zmiany repozytorium. */
    private ValidationResult CurrentValidation() => TicketValidator.Validate(Name, Description,
        PriorityIndex is >= 0 and <= 2 ? (TicketPriority)PriorityIndex : null);

    /* Metoda: Validate. Wynik: aktualne błędy przy polach oraz dostępność zapisu. */
    private void Validate()
    {
        var v = CurrentValidation();
        NameError = v.NameError; DescriptionError = v.DescriptionError; PriorityError = v.PriorityError;
        SaveCommand.Refresh();
    }

    /* Metoda: Save. Wynik: utworzenie/edycja przez Bus po walidacji; bez zmiany przy błędach. */
    private void Save()
    {
        Validate();
        if (!CurrentValidation().IsValid) return;
        var priority = (TicketPriority)PriorityIndex;
        var result = SelectedTicket is { } ticket
            ? _services.Commands.Send(new EditTicket(ticket.Id, Name, Description, priority))
            : _services.Commands.Send(new CreateTicket(Name, Description, priority));
        if (!result.Success) { Message = result.Error; return; }
        var edited = IsEditing;
        Reset(false);
        Message = edited ? "Zmiany zostały zapisane." : "Dodano zgłoszenie ze statusem Nowe.";
    }

    /* Metoda: Delete. Wynik: usunięcie wybranego Id przez Bus i powrót do listy. */
    private void Delete()
    {
        if (SelectedTicket is not { } ticket) return;
        var result = _services.Commands.Send(new DeleteTicket(ticket.Id));
        if (!result.Success) { Message = result.Error; return; }
        Reset(false); Message = "Usunięto zgłoszenie.";
    }

    /* Metoda: ChangeStatus. Wynik: osobny zapis statusu; nie zapisuje roboczej nazwy/opisu. */
    private void ChangeStatus()
    {
        if (SelectedTicket is not { } ticket) return;
        var result = _services.Commands.Send(new ChangeTicketStatus(ticket.Id, (TicketStatus)StatusIndex));
        Message = result.Success ? "Status został zaktualizowany." : result.Error;
    }

    /* Metoda: Reset. Parametr: otwarcie formularza. Wynik: porzucenie kopii roboczej bez zapisu. */
    private void Reset(bool open)
    {
        SelectedTicket = null; Name = ""; Description = ""; PriorityIndex = -1; StatusIndex = 0;
        FormOpen = open;
        Validate();
    }
}
