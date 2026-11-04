using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHelpDesk.Core;
using MiniHelpDesk.ViewModels;

namespace MiniHelpDesk.Tests;

/* Klasa: LogicTests. Cel: niezależne testy jednostkowe reguł, CQRS, Observer i formularza.
   Każdy test tworzy własne repozytorium; asercje pochodzą z prawdziwego MSTest.TestFramework. */
[TestClass]
public sealed class LogicTests
{
    /* Metoda: Add. Parametr: zestaw usług. Wynik: Id poprawnie dodanego zgłoszenia. */
    private static Guid Add(HelpDeskServices s) => s.Commands.Send(new CreateTicket("Drukarka", "Nie drukuje od rana", TicketPriority.High)).Id!.Value;

    /* Metoda: RequiredFields. Wynik: błędy przy każdym pustym polu, także dla null. */
    [TestMethod] public void RequiredFields()
    {
        var v = TicketValidator.Validate(null, null, null);
        Assert.IsFalse(v.IsValid); Assert.IsFalse(string.IsNullOrEmpty(v.NameError));
        Assert.IsFalse(string.IsNullOrEmpty(v.DescriptionError)); Assert.IsFalse(string.IsNullOrEmpty(v.PriorityError));
    }
    /* Metoda: TrimBeforeLengthCheck. Wynik: brzegi usuwane przed sprawdzaniem długości. */
    [TestMethod] public void TrimBeforeLengthCheck()
    {
        Assert.IsFalse(TicketValidator.Validate(" ab ", " 123456789 ", TicketPriority.Low).IsValid);
        var v = TicketValidator.Validate(" abc ", " 1234567890 ", TicketPriority.Low);
        Assert.IsTrue(v.IsValid); Assert.AreEqual("abc", v.Name); Assert.AreEqual("1234567890", v.Description);
    }
    /* Metoda: WhitespaceRejected. Wynik: same białe znaki nie tworzą poprawnych pól. */
    [TestMethod] public void WhitespaceRejected() => Assert.IsFalse(TicketValidator.Validate(" \t\n ", "            ", TicketPriority.Low).IsValid);
    /* Metoda: UnknownPriorityRejected. Wynik: enum spoza zakresu nie przechodzi walidacji. */
    [TestMethod] public void UnknownPriorityRejected() => Assert.IsFalse(TicketValidator.Validate("abc", "1234567890", (TicketPriority)99).IsValid);
    /* Metoda: AddSetsNewStatus. Wynik: nowe Id, wyczyszczone pola i status Nowe. */
    [TestMethod] public void AddSetsNewStatus()
    {
        var s = new HelpDeskServices();
        var r = s.Commands.Send(new CreateTicket(" abc ", " 1234567890 ", TicketPriority.Medium));
        var t = s.Queries.Handle(new()).Single();
        Assert.IsTrue(r.Success); Assert.AreNotEqual(Guid.Empty, t.Id); Assert.AreEqual(t.Id, r.Id);
        Assert.AreEqual("abc", t.Name); Assert.AreEqual("1234567890", t.Description); Assert.AreEqual(TicketStatus.New, t.Status);
    }
    /* Metoda: InvalidAddDoesNotWrite. Wynik: handlery bronią repozytorium nawet z pominięciem UI. */
    [TestMethod] public void InvalidAddDoesNotWrite()
    {
        var s = new HelpDeskServices();
        var r = s.Commands.Send(new CreateTicket("x", "za krótki", null));
        Assert.IsFalse(r.Success); Assert.IsNotNull(r.Validation); Assert.AreEqual(0, s.Queries.Handle(new()).Count);
    }
    /* Metoda: IdsAreUnique. Wynik: różne Id dla 100 takich samych zgłoszeń. */
    [TestMethod] public void IdsAreUnique()
    {
        var s = new HelpDeskServices(); for (var i = 0; i < 100; i++) Add(s);
        Assert.AreEqual(100, s.Queries.Handle(new()).Select(t => t.Id).Distinct().Count());
    }
    /* Metoda: QueryDoesNotModifyData. Wynik: odczyt zwraca stabilny widok bez zapisu. */
    [TestMethod] public void QueryDoesNotModifyData()
    {
        var s = new HelpDeskServices(); Add(s);
        var one = s.Queries.Handle(new()); var two = s.Queries.Handle(new());
        Assert.AreSame(one, two); Assert.AreEqual(1, two.Count);
    }
    /* Metoda: CollectionNotifies. Wynik: Add/Remove generują powiadomienia kolekcji. */
    [TestMethod] public void CollectionNotifies()
    {
        var s = new HelpDeskServices(); var events = new List<NotifyCollectionChangedAction>();
        ((INotifyCollectionChanged)s.Queries.Handle(new())).CollectionChanged += (_, e) => events.Add(e.Action);
        var id = Add(s); s.Commands.Send(new DeleteTicket(id));
        CollectionAssert.AreEqual(new[] { NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Remove }, events);
    }
    /* Metoda: EditPreservesIdentityAndStatus. Wynik: edycja w miejscu nie resetuje statusu. */
    [TestMethod] public void EditPreservesIdentityAndStatus()
    {
        var s = new HelpDeskServices(); var id = Add(s); var t = s.Queries.Handle(new()).Single();
        s.Commands.Send(new ChangeTicketStatus(id, TicketStatus.InProgress));
        Assert.IsTrue(s.Commands.Send(new EditTicket(id, " Nowa nazwa ", " Dłuższy opis problemu ", TicketPriority.Low)).Success);
        Assert.AreSame(t, s.Queries.Handle(new()).Single()); Assert.AreEqual(id, t.Id);
        Assert.AreEqual("Nowa nazwa", t.Name); Assert.AreEqual(TicketStatus.InProgress, t.Status);
    }
    /* Metoda: InvalidEditIsAtomic. Wynik: żadne pole nie zmienia się przy błędnej edycji. */
    [TestMethod] public void InvalidEditIsAtomic()
    {
        var s = new HelpDeskServices(); var id = Add(s); var t = s.Queries.Handle(new()).Single();
        var name = t.Name; var description = t.Description; var priority = t.Priority;
        Assert.IsFalse(s.Commands.Send(new EditTicket(id, "x", "poprawny opis nowy", TicketPriority.Low)).Success);
        Assert.AreEqual(name, t.Name); Assert.AreEqual(description, t.Description); Assert.AreEqual(priority, t.Priority);
    }
    /* Metoda: StatusAndPropertiesNotify. Wynik: Binding dostaje zmianę danych i etykiet. */
    [TestMethod] public void StatusAndPropertiesNotify()
    {
        var s = new HelpDeskServices(); var id = Add(s); var t = s.Queries.Handle(new()).Single(); var names = new List<string>();
        t.PropertyChanged += (_, e) => names.Add(e.PropertyName!);
        s.Commands.Send(new EditTicket(id, "Inna nazwa", "Inny opis problemu", TicketPriority.Low));
        s.Commands.Send(new ChangeTicketStatus(id, TicketStatus.Closed));
        foreach (var name in new[] { "Name", "Description", "Priority", "PriorityLabel", "Status", "StatusLabel" }) CollectionAssert.Contains(names, name);
        Assert.AreEqual("Zamknięte", t.StatusLabel);
    }
    /* Metoda: SameStatusDoesNotNotify. Wynik: brak zbędnych zdarzeń dla tej samej wartości. */
    [TestMethod] public void SameStatusDoesNotNotify()
    {
        var s = new HelpDeskServices(); var id = Add(s); var count = 0;
        s.Queries.Handle(new()).Single().PropertyChanged += (_, _) => count++;
        s.Commands.Send(new ChangeTicketStatus(id, TicketStatus.New)); Assert.AreEqual(0, count);
    }
    /* Metoda: UnknownStatusDoesNotWrite. Wynik: niepoprawny status odrzucony przez handler. */
    [TestMethod] public void UnknownStatusDoesNotWrite()
    {
        var s = new HelpDeskServices(); var id = Add(s);
        Assert.IsFalse(s.Commands.Send(new ChangeTicketStatus(id, (TicketStatus)999)).Success);
        Assert.AreEqual(TicketStatus.New, s.Queries.Handle(new()).Single().Status);
    }
    /* Metoda: MissingIdsReturnErrors. Wynik: brak elementu nie jest sukcesem ani wyjątkiem. */
    [TestMethod] public void MissingIdsReturnErrors()
    {
        var s = new HelpDeskServices(); var id = Guid.NewGuid();
        Assert.IsFalse(s.Commands.Send(new DeleteTicket(id)).Success);
        Assert.IsFalse(s.Commands.Send(new ChangeTicketStatus(id, TicketStatus.Closed)).Success);
        Assert.IsFalse(s.Commands.Send(new EditTicket(id, "abc", "1234567890", TicketPriority.Low)).Success);
    }
    /* Metoda: DeleteRemovesOnlySelected. Wynik: usunięte tylko wskazane Id. */
    [TestMethod] public void DeleteRemovesOnlySelected()
    {
        var s = new HelpDeskServices(); var first = Add(s); var second = Add(s);
        Assert.IsTrue(s.Commands.Send(new DeleteTicket(first)).Success);
        Assert.AreEqual(second, s.Queries.Handle(new()).Single().Id);
    }
    /* Metoda: RepositoryRejectsDuplicateId. Wynik: repozytorium broni unikatowości. */
    [TestMethod] public void RepositoryRejectsDuplicateId()
    {
        var repo = new InMemoryTicketRepository(); var t = new Ticket("abc", "1234567890", TicketPriority.Low); repo.Add(t);
        Assert.ThrowsException<InvalidOperationException>(() => repo.Add(t));
    }
    /* Metoda: UnregisteredCommandFailsClearly. Wynik: brak handlera daje czytelny wyjątek. */
    [TestMethod] public void UnregisteredCommandFailsClearly() => Assert.ThrowsException<InvalidOperationException>(() => new CommandBus().Send(new DeleteTicket(Guid.NewGuid())));
    /* Metoda: InvalidFormDisablesSave. Wynik: błędny formularz blokuje wykonanie komendy. */
    [TestMethod] public void InvalidFormDisablesSave()
    {
        var s = new HelpDeskServices(); var vm = new HelpDeskViewModel(s);
        Assert.IsFalse(vm.SaveCommand.CanExecute(null)); vm.SaveCommand.Execute(null);
        Assert.AreEqual(0, vm.Tickets.Count);
        vm.Name = "abc"; vm.Description = "1234567890"; Assert.IsFalse(vm.SaveCommand.CanExecute(null));
        vm.PriorityIndex = 0; Assert.IsTrue(vm.SaveCommand.CanExecute(null));
        vm.SaveCommand.Execute(null); Assert.AreEqual(1, vm.Tickets.Count); Assert.IsFalse(vm.FormOpen);
    }
    /* Metoda: DraftDoesNotLeakAndCancelDiscards. Wynik: zmiana/anulowanie formularza nie modyfikuje modelu. */
    [TestMethod] public void DraftDoesNotLeakAndCancelDiscards()
    {
        var s = new HelpDeskServices(); Add(s); var vm = new HelpDeskViewModel(s); var t = vm.Tickets.Single();
        vm.SelectedTicket = t; vm.Name = "Robocza nazwa";
        Assert.AreEqual("Drukarka", t.Name); Assert.IsTrue(vm.FormOpen);
        vm.CancelCommand.Execute(null); Assert.AreEqual("Drukarka", t.Name); Assert.IsNull(vm.SelectedTicket);
    }
    /* Metoda: ViewModelEditAndStatusAndDelete. Wynik: pełna ścieżka zmian przez komendy UI. */
    [TestMethod] public void ViewModelEditAndStatusAndDelete()
    {
        var s = new HelpDeskServices(); var id = Add(s); var vm = new HelpDeskViewModel(s); var t = vm.Tickets.Single();
        vm.SelectedTicket = t; vm.Name = "Nowa nazwa"; vm.SaveCommand.Execute(null);
        Assert.AreEqual(id, t.Id); Assert.AreEqual("Nowa nazwa", t.Name);
        vm.SelectedTicket = t; vm.StatusIndex = 2; vm.StatusCommand.Execute(null); Assert.AreEqual(TicketStatus.Closed, t.Status);
        vm.DeleteCommand.Execute(null); Assert.AreEqual(0, vm.Tickets.Count);
    }
    /* Metoda: SeparateInstancesAreIsolated. Wynik: testy ani inne kontenery nie dotykają danych użytkownika. */
    [TestMethod] public void SeparateInstancesAreIsolated()
    { var one = new HelpDeskServices(); Add(one); Assert.AreEqual(0, new HelpDeskServices().Queries.Handle(new()).Count); }
}
