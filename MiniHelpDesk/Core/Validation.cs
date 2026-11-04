namespace MiniHelpDesk.Core;

/* Klasa: ValidationResult. Cel: błędy przypisane do konkretnych pól i dane po Trim. */
public sealed record ValidationResult(string Name, string Description, string NameError, string DescriptionError, string PriorityError)
{
    public bool IsValid => NameError.Length == 0 && DescriptionError.Length == 0 && PriorityError.Length == 0;
}

/* Klasa: TicketValidator. Cel: jedyne źródło reguł dla formularza i handlerów CQRS. */
public static class TicketValidator
{
    /* Metoda: Validate. Parametry: nazwa/opis (także null), opcjonalny priorytet.
       Wynik: oczyszczone pola i komunikaty; żadne dane nie są zapisywane. */
    public static ValidationResult Validate(string? name, string? description, TicketPriority? priority)
    {
        var n = (name ?? "").Trim();
        var d = (description ?? "").Trim();
        return new(n, d,
            n.Length < 3 ? "Nazwa musi mieć co najmniej 3 znaki." : "",
            d.Length < 10 ? "Opis musi mieć co najmniej 10 znaków." : "",
            !priority.HasValue || !Enum.IsDefined(priority.Value) ? "Wybierz priorytet zgłoszenia." : "");
    }
}
