using System.ComponentModel;
using MiniHelpDesk.Tests;
using MiniHelpDesk.ViewModels;

namespace MiniHelpDesk;

/* Klasa: MainPage. Cel: wspólny XAML, adaptacja rozmiaru i dialogi platformowe.
   Logika danych pozostaje wyłącznie w ViewModel/handlerach, bez duplikacji na platformy. */
public partial class MainPage : ContentPage
{
    private readonly HelpDeskViewModel _vm;
    private bool? _wideLayout;
    /* Konstruktor: parametr ViewModel. Wynik: inicjalizacja widoku i BindingContext. */
    public MainPage(HelpDeskViewModel vm)
    {
        InitializeComponent(); _vm = vm; BindingContext = vm;
        vm.PropertyChanged += OnViewModelChanged;
    }
    /* Metoda: OnSizeAllocated. Parametry: rozmiar. Wynik: układ obok siebie lub widok mobilny. */
    protected override void OnSizeAllocated(double width, double height)
    { base.OnSizeAllocated(width, height); if (_vm is not null) AdaptLayout(); }
    /* Metoda: OnViewModelChanged. Wynik: przełączenie listy/formularza na wąskim ekranie. */
    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    { if (e.PropertyName == nameof(_vm.FormOpen)) AdaptLayout(); }
    /* Metoda: AdaptLayout. Wynik: przy szerokości <900 lista i formularz zajmują ten sam obszar naprzemiennie. */
    private void AdaptLayout()
    {
        var wide = Width >= 900;
        if (_wideLayout != wide)
        {
            _wideLayout = wide;
            Workspace.ColumnDefinitions = wide
                ? new ColumnDefinitionCollection { new(new GridLength(1, GridUnitType.Star)), new(new GridLength(1.5, GridUnitType.Star)) }
                : new ColumnDefinitionCollection { new(GridLength.Star) };
            Grid.SetColumn(ListPanel, wide ? 1 : 0);
        }
        FormCard.IsVisible = wide || _vm.FormOpen;
        ListPanel.IsVisible = wide || !_vm.FormOpen;
    }
    /* Metoda: OnDeleteClicked. Wynik: po potwierdzeniu usuwa wybrane zgłoszenie przez komendę UI. */
    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var selected = _vm.SelectedTicket;
        if (selected is null) return;
        if (await DisplayAlertAsync("Usunąć zgłoszenie?", selected.Name, "Usuń", "Anuluj") && _vm.SelectedTicket?.Id == selected.Id)
            _vm.DeleteCommand.Execute(null);
    }
    /* Metoda: OnTestsClicked. Wynik: osobne dane testowe, modalny raport; dane użytkownika bez zmian. */
    private async void OnTestsClicked(object? sender, EventArgs e)
    {
        var report = TestRunner.Run();
        var close = new Button { Text = "Wróć do zgłoszeń" };
        close.Clicked += async (_, _) => await Navigation.PopModalAsync();
        var page = new ContentPage
        {
            Title = "Testy logiki",
            Content = new Grid
            {
                Padding = 20, RowSpacing = 12,
                RowDefinitions = [new(GridLength.Star), new(GridLength.Auto)],
                Children = { new ScrollView { Content = new Label { Text = report.Text, FontSize = 14 } } }
            }
        };
        var grid = (Grid)page.Content;
        Grid.SetRow(close, 1); grid.Children.Add(close);
        await Navigation.PushModalAsync(page);
    }
}
