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
| Windows | Kod i CI przygotowane; kompilacja nieuruchomiona w Linux | Platforms/Windows, .github/workflows/build.yml |
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

Próby utworzenia `kamyczekk/mini-helpdesk-maui` przez GraphQL i REST zakończyły się odpowiednio `Resource not accessible by integration (createRepository)` oraz HTTP 403. Nie utworzono zdalnego repozytorium i nie deklarujemy udanego pushu. Projekt jest gotowy do wysłania po udostępnieniu pustego repozytorium. Konfiguracja CI zacznie działać dopiero po pushu i nie jest przedstawiana jako wykonana weryfikacja Windows.
