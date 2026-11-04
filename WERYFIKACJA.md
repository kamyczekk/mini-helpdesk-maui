# Weryfikacja Mini HelpDesk

Projekt ma jeden plik `.csproj`. Wybrany zakres: Windows **i** Android.

| Wymaganie | Wynik | Dowód |
| --- | --- | --- |
| Nazwa ≥3 i opis ≥10 po Trim | Działa w testach logiki | RequiredFields, TrimBeforeLengthCheck, WhitespaceRejected |
| Obowiązkowy priorytet i odrzucenie nieznanych enumów | Działa w testach | UnknownPriorityRejected, InvalidFormDisablesSave |
| Odrzucenie błędnego zapisu także bez UI | Działa w testach | InvalidAddDoesNotWrite, InvalidEditIsAtomic |
| Domyślny status Nowe i Guid | Działa w testach | AddSetsNewStatus, IdsAreUnique, RepositoryRejectsDuplicateId |
| ObservableCollection i INotifyPropertyChanged | Działa w testach | CollectionNotifies, StatusAndPropertiesNotify, SameStatusDoesNotNotify |
| CQRS, Command Bus, handlery, repozytorium w pamięci | Działa w testach | QueryDoesNotModifyData, UnregisteredCommandFailsClearly i testy zapisów |
| Edycja z zachowaniem Id/statusu; anulowanie | Działa w testach | EditPreservesIdentityAndStatus, DraftDoesNotLeakAndCancelDiscards |
| Usuwanie i status | Działa w testach | DeleteRemovesOnlySelected, ViewModelEditAndStatusAndDelete, UnknownStatusDoesNotWrite |
| Izolacja danych testowych | Działa w testach | SeparateInstancesAreIsolated |
| CollectionView z Binding, wspólny XAML | Skompilowano w celu Android | MainPage.xaml, weryfikacja/android-build.log |
| Android ARM64 APK | Kompilacja udana, bez błędów i ostrzeżeń | weryfikacja/android-build.log |
| Android APK w CI na Windows | Kompilacja i zapis artefaktu zakończone powodzeniem | [Przebieg GitHub Actions](https://github.com/kamyczekk/mini-helpdesk-maui/actions/runs/41796016558) |
| Windows | Kompilacja w CI na Windows zakończona powodzeniem | [Przebieg GitHub Actions](https://github.com/kamyczekk/mini-helpdesk-maui/actions/runs/41796016558), krok Windows |
| Zachowanie UI na Windows i Androidzie | Do demonstracji na urządzeniach | Scenariusz w URUCHOMIENIE.md; nie deklarujemy wykonania tego pokazu |

## Rzeczywiście wykonane testy

**22/22 testy MSTest zaliczone**, 0 błędów. Pełny wynik: `weryfikacja/mstest.log`.

```bash
dotnet run --project MiniHelpDesk/MiniHelpDesk.csproj -p:HeadlessTests=true
```

Wykorzystano SDK .NET 10.0.100 i MSTest.TestFramework 3.6.4. Własny runner uruchamia metody z atrybutem TestMethod, korzystające z asercji MSTest. Ta sama klasa testów jest dostępna przez przycisk w aplikacji MAUI. To **nie** jest standardowy `dotnet test`.

Android zbudowano w środowisku Linux z workload MAUI 10.0.114, pakietem MAUI Controls 10.0.110, Android SDK 36.1, build-tools 36.0.0 i JDK 21. Polecenie (ścieżki zależne od lokalnej instalacji):

```bash
dotnet build MiniHelpDesk/MiniHelpDesk.csproj -f net10.0-android \
  -p:TargetFrameworks=net10.0-android \
  -p:RuntimeIdentifiers=android-arm64 -p:RuntimeIdentifier=android-arm64 \
  -p:AndroidSdkDirectory=/workspace/tools/android \
  -p:JavaSdkDirectory=/workspace/tools/jdk -c Debug
```

APK jest wersją Debug podpisaną automatycznym kluczem testowym, dla urządzeń ARM64. Nie jest publikacją w sklepie. Wymagane pokazanie działania obu platform nauczycielowi pozostaje osobnym krokiem.

## Dostęp do GitHuba

Repozytorium: https://github.com/kamyczekk/mini-helpdesk-maui. Użytkownik utworzył je po zablokowaniu tworzenia repozytoriów przez integrację GitHuba. Bieżący wynik automatycznych kompilacji jest dostępny w zakładce Actions. Sama obecność konfiguracji CI nie stanowi dowodu wykonanej weryfikacji Windows; lokalne wyniki opisano powyżej.

## Zakończony przebieg CI

[Przebieg 41796016558](https://github.com/kamyczekk/mini-helpdesk-maui/actions/runs/41796016558) dla commitu `9c1fe161aab1008288e1b7ebde8dd9af68fdb709` zakończył się wynikiem **success**. Testy logiki przeszły na Ubuntu, a kompilacje Windows i Android na runnerze Windows. Opublikowano artefakty `MiniHelpDesk-Windows-debug` oraz `MiniHelpDesk-Android-debug`, dostępne w tym przebiegu. Późniejsza aktualizacja tego dokumentu nie zmienia kodu aplikacji.

Zakończone kompilacje nie zastępują demonstracji interfejsu na Windows i urządzeniu/emulatorze Android.
