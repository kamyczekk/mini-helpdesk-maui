# Mini HelpDesk

Jedno rozwiązanie `MiniHelpDesk.sln` i **jeden** projekt `MiniHelpDesk/MiniHelpDesk.csproj`.
Wspólne modele, walidacja, CQRS, ViewModel, testy i `MainPage.xaml` dla Windows i Androida.
Nie używa bazy danych, API, kont ani trwałego zapisu. Po zamknięciu procesu zgłoszenia znikają.

## Wymagania

- .NET SDK 10.0.100 (global.json dopuszcza poprawki tej wersji), workload MAUI.
- Windows 10 1809 lub nowszy; do budowania Windows: Windows i narzędzia WinUI/.NET MAUI w Visual Studio.
- Android 6.0 (API 23) lub nowszy; do budowania JDK 21, Android SDK, emulator lub urządzenie z debugowaniem USB.
- `dotnet workload install maui --version 10.0.114` na Windows. Linux nie obsługuje natywnego celu Windows.

## Uruchomienie Windows

Otwórz rozwiązanie w Visual Studio z obsługą .NET 10/MAUI, wybierz Windows Machine i uruchom.
Alternatywnie na Windows:

```powershell
dotnet build MiniHelpDesk/MiniHelpDesk.csproj -t:Run -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 -p:RuntimeIdentifier=win-x64
```

## Uruchomienie Android

W Visual Studio wybierz emulator albo podłączone urządzenie i uruchom ten sam projekt.
W terminalu z dostępnymi Android SDK/JDK:

```powershell
dotnet build MiniHelpDesk/MiniHelpDesk.csproj -t:Run -f net10.0-android -p:TargetFrameworks=net10.0-android
```

W razie potrzeby przekaż `-p:AndroidSdkDirectory=...` i `-p:JavaSdkDirectory=...`.

## Obsługa

1. `+ Nowe`: wpisz nazwę i opis, wybierz priorytet. Błędy są widoczne przy polach; zapis pozostaje niedostępny przy błędach.
2. Zapis dodaje zgłoszenie do CollectionView, ze statusem Nowe i unikatowym Guid.
3. Wybierz zgłoszenie na liście, aby edytować. Formularz pracuje na kopii roboczej; Anuluj nie zmienia danych.
4. Zmień dane i zapisz albo wybierz status i użyj Zastosuj status. Status jest oddzielną operacją i nie zapisuje roboczego opisu.
5. Usuń zgłoszenie po potwierdzeniu. Lista i właściwości odświeżają się przez Observer.
6. Przy szerokości od 900 jednostek formularz i lista są obok siebie. Poniżej 900 przełączają się w tym samym miejscu; formularz jest przewijany, przyciski mają minimum 48 jednostek wysokości.

## Testy MSTest w jednym projekcie

Przycisk `Uruchom testy logiki (MSTest)` uruchamia testy i pokazuje pełny wynik. Testy mają własne repozytoria i nie zmieniają zgłoszeń użytkownika.
To własny runner korzystający z oryginalnych atrybutów i asercji MSTest.TestFramework, **nie standardowy `dotnet test`**.

Te same źródła logiki, ViewModel i testów można uruchomić bez środowiska MAUI:

```bash
dotnet run --project MiniHelpDesk/MiniHelpDesk.csproj -p:HeadlessTests=true
```

Przełącznik ustawia cel net10.0 i pomija wyłącznie źródła UI/platform. Nie tworzy drugiego projektu ani drugiej implementacji logiki. Nie należy traktować tego testu jako dowodu uruchomienia UI na urządzeniu.

## Architektura i komentarze

- Core/Ticket.cs: model INotifyPropertyChanged; Id nie zmienia się podczas edycji.
- Core/Repository.cs: ObservableCollection z publicznym ReadOnlyObservableCollection do Binding.
- Core/Validation.cs: pojedynczy walidator wspólny dla UI i handlerów. Trim przed długością; enum sprawdzany także pod kątem wartości spoza zakresu.
- Core/Cqrs.cs: komendy tworzenia/edycji/usuwania/statusu, CommandBus, osobne handlery, niezależne zapytanie i handler odczytu.
- ViewModels: stan formularza i komendy UI delegujące do CQRS.
- Tests: jednostkowe asercje MSTest i runner izolujący dane.

Komentarze blokowe `/* ... */` opisują klasy i metody bezpośrednio nad elementami. Ponieważ w przekazanym zadaniu nie było zapowiedzianych szczegółowych schematów, przyjęto: klasa → nazwa/cel/odpowiedzialność; metoda → nazwa/parametry/wynik/wyjątki, gdy występują.

Nie podano dalszej części narastających kryteriów ocen. Zaimplementowano wszystkie opisane operacje, lecz nie przypisujemy aplikacji konkretnej oceny bez brakującej rubryki.

## Pokaz nauczycielowi

Na Windows i Androidzie pokaż: odrzucenie pól z samymi spacjami, granice 3/10 znaków, obowiązkowy priorytet, dodawanie, edycję, anulowanie, status, usuwanie, responsywny układ i raport testów. Nie potrzeba zrzutów ekranu. Faktycznej prezentacji urządzeń nie zastępuje kompilacja ani CI.
