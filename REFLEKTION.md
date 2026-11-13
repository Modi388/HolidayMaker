# Individuell reflektion – Holiday Maker

## Datamodellen
Vi ritade E/R-diagrammet (v1) i grupp. Databasen har fem tabeller: customer, accommodation, facility, room och booking.
Booking kopplar ihop kund och rum. En kund kan ha många bokningar och ett rum kan bokas många gånger, så de
främmande nycklarna ligger i booking, alltså på många-sidan. Ett boende har många rum, och varje boende har exakt
en facility-rad (UNIQUE på accommodation_id).

I min version (v2) tog vi bort PAYMENT. Betalning ingick inte i kraven, och beloppet kan räknas ut som antal
nätter × pris per natt. Att lagra det skulle vara dubbellagring som kan bli inkonsekvent. Jag tog också bort
has_wifi och has_parking eftersom de inte efterfrågas. Facility fick vara kvar som egen tabell så att fler
faciliteter kan läggas till utan att ändra boendetabellen. Nackdelen är en extra JOIN i sökningen, och det hade
fungerat lika bra att lägga kolumnerna direkt i accommodation.

Dataintegriteten skyddas i databasen och inte bara i programmet: NOT NULL, UNIQUE på e-post och på rumsnummer per
boende, CHECK på omdöme 1–5, avstånd ≥ 0, antal gäster > 0 och utcheckning efter incheckning, samt främmande nycklar.

## Säkerhet
Alla värden från användaren skickas som parametrar (@namn och AddWithValue), så inmatning kan aldrig köras som
SQL-kod. Sorteringen kan inte vara en parameter, så den väljs mellan två fasta strängar i koden. Bokningen sparas
med en INSERT som innehåller samma tillgänglighetskontroll som sökningen, så rummet kontrolleras igen i samma
ögonblick som bokningen sparas och kan inte dubbelbokas. UPDATE och DELETE kräver också att bokningen tillhör den
valda kunden.

En begränsning är att SQLite är en vanlig fil utan användare eller behörigheter. Den som kommer åt filen kommer åt
all data. Med en databasserver som MySQL skulle man kunna ge programmet ett eget konto med bara de rättigheter det
behöver.

## Resultat
Programmet uppfyller grundkraven: registrera och välja kund, söka lediga rum med valfria filter och två
sorteringar, boka med bekräftelse och totalpris, visa bokningar, ändra antal personer och avboka. Testerna i
README visar att överlapp, kapacitet, avbokning och omstart fungerar. Felaktig inmatning ger en ny fråga i stället
för att programmet kraschar.

## Möjliga förbättringar
- Spara priser som heltal (hela kronor) i stället för REAL, eftersom flyttal kan ge avrundningsfel.
- Transaktioner med låsning om flera operatörer ska kunna boka samtidigt.
- Index på booking(room_id, start_date, end_date) om databasen blir stor.
- Möjlighet att byta rum eller datum i en befintlig bokning.
- Begränsa bokningsbara datum till december 2026 i programmet.

## Hur jag har använt AI och kontrollerat resultatet
Jag har använt Claude genom hela projektet. Jag byggde databasen och testdatan utifrån gruppens diagram. SQL-frågorna
skrev och testade jag i DBeaver och DataGrip, bland annat med testfall för överlapp och kapacitet.
Under arbetet lärde jag mig till exempel att INSERT alltid lägger till rader: när jag körde testdata-skriptet två
gånger dubblerades kunder och bokningar, och jag byggde om databasen från skripten.

C#-koden skrevs med hjälp av Claude utifrån mina testade SQL-frågor och samma mönster som i lektionens
ProductRepository. Jag har kört programmet, gått igenom testerna i README och gått igenom koden fil för fil så att jag
kan förklara relationerna, tillgänglighetssökningen och de parametriserade anropen, vilket jag gjorde på redovisningen.
