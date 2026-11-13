# Holiday Maker

Konsolapplikation för en resebyråoperatör som söker lediga rum, registrerar kunder och hanterar bokningar.
Byggd med C# (.NET 8 eller senare), SQLite och NuGet-paketet `Microsoft.Data.Sqlite`.

## Innehåll

| Fil / mapp | Innehåll |
|---|---|
| `ER-diagram-v2.png` | E/R-diagram (tabeller, relationer, PK och FK) |
| `create_database.sql` | Skapar tabellerna med NOT NULL, UNIQUE, CHECK och främmande nycklar |
| `testdata.sql` | 5 boenden på 3 orter, 20 rum, 5 kunder, 25 bokningar |
| `queries.sql` | Alla SQL-frågor, testade i databasklient (DBeaver/DataGrip) innan de användes i C# |
| `holiday_maker.db` | Färdig SQLite-databas |
| `HolidayMaker/` | Konsolapplikationens källkod |
| `REFLEKTION.md` | Individuell reflektion |

## Skapa databasen

En färdig `holiday_maker.db` med testdatan finns med. Så här skapar du den från början:

1. Skapa en tom SQLite-databas med namnet `holiday_maker.db` i den här mappen (t.ex. i DataGrip eller DBeaver).
2. Kör `create_database.sql` (tar bort och skapar tabellerna).
3. Kör `testdata.sql`. Kör skriptet bara en gång, annars dubbleras kunder och bokningar.

## Köra programmet

```
cd HolidayMaker
dotnet run
```

Programmet letar efter `holiday_maker.db` i sin egen mapp och i mapparna ovanför, så databasen hittas när den ligger här bredvid mappen `HolidayMaker`.
Stäng databasklienten först så att filen inte är låst. Paketet `Microsoft.Data.Sqlite` laddas ner automatiskt första gången.

Arbetsflöde: välj kund (2) → sök lediga rum och boka (3) → visa bokningar (4) → ändra antal personer (5) → avboka (6).
Datum skrivs som ÅÅÅÅ-MM-DD. Felaktig inmatning ger en ny fråga i stället för att programmet avslutas.

## Kodstruktur

| Fil | Ansvar |
|---|---|
| `Program.cs` | Meny och arbetsflöde, ingen SQL |
| `Database.cs` | Hittar och öppnar databasen med främmande nycklar påslagna |
| `CustomerRepository.cs` | Registrera och söka kund |
| `RoomRepository.cs` | Sökning efter lediga rum med valfria filter och två sorteringar |
| `BookingRepository.cs` | Skapa, visa, ändra antal personer och avboka |
| `Models.cs` | Records för kund, rum och bokning |
| `Input.cs` | Säker inmatning |

All SQL använder parametrar (`@namn` + `AddWithValue`). Sorteringen väljs mellan två fasta strängar i koden.

## Tester

Manuella tester i konsolappen och i DataGrip med testdatan.

| # | Krav | Så testades det | Resultat |
|---|---|---|---|
| 1 | Ett ledigt rum visas och ett upptaget filtreras bort | Båstad 2026-12-11 till 12-14, 2 pers: rum 201 (bokat 11–14) saknas, rum 202 visas | OK |
| 2 | Rum kan bokas samma dag som förra kunden checkar ut | Ystad 2026-12-13 till 12-15: rum 101 (bokat 10–13) visas och kan bokas | OK |
| 3 | Överlappande bokningar nekas, även omslutande | Bokning av rum 101 2026-12-09 till 12-16 (omsluter 10–13) ger 0 rader | OK |
| 4 | För stort sällskap kan inte boka | 5 personer i ett 2-personersrum ger 0 rader; sökningen visar inte rummet | OK |
| 5 | Kombinerade filter och båda sorteringarna | Båstad med restaurang = j: Villa Ljung försvinner. Sortering 1 = lägst pris först, 2 = högst omdöme först | OK |
| 6 | Ändring av antal personer följer kapaciteten | 3 personer i rum med plats för 2: nekas. 1 person: ändras | OK |
| 7 | Avbokning frigör rummet | Avbokat rum 202 visas igen i samma sökning och kan bokas igen | OK |
| 8 | Data finns kvar efter omstart | Programmet avslutat och startat igen: bokningen visas under val 4 | OK |
| 9 | Inga sökträffar | Malmö, 6 personer: "Inga lediga rum matchar sökningen" | OK |
| 10 | Felaktig inmatning | "abc" som menyval och fel datumformat ger ny fråga, ingen krasch | OK |
| 11 | SQL-injektion | Kundsökning på `'; DROP TABLE booking; --` ger "Ingen kund hittades", tabellen finns kvar | OK |
| 12 | Dubbel e-post | Ny kund med befintlig e-post nekas av UNIQUE med ett meddelande | OK |
