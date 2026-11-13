using HolidayMaker;
using Microsoft.Data.Sqlite;

await using var connection = await Database.OpenAsync();
var customers = new CustomerRepository(connection);
var rooms = new RoomRepository(connection);
var bookings = new BookingRepository(connection);

Customer? currentCustomer = null; // Den kund operatören arbetar med just nu

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== HOLIDAY MAKER ===");
    Console.WriteLine($"Vald kund: {(currentCustomer == null ? "(ingen)" : $"{currentCustomer.Name} (id {currentCustomer.Id})")}");
    Console.WriteLine("1. Registrera ny kund");
    Console.WriteLine("2. Välj kund");
    Console.WriteLine("3. Sök lediga rum och boka");
    Console.WriteLine("4. Visa kundens bokningar");
    Console.WriteLine("5. Ändra antal personer i en bokning");
    Console.WriteLine("6. Avboka");
    Console.WriteLine("0. Avsluta");

    int choice = Input.ReadInt("Val: ", 0, 6);
    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case 1: currentCustomer = await RegisterCustomer(); break;
            case 2: currentCustomer = await SelectCustomer() ?? currentCustomer; break;
            case 3: await SearchAndBook(); break;
            case 4: await ShowBookings(); break;
            case 5: await ChangeGuests(); break;
            case 6: await CancelBooking(); break;
            case 0: return;
        }
    }
    catch (SqliteException ex)
    {
        // Databasfel ska inte krascha programmet
        Console.WriteLine($"Databasfel: {ex.Message}");
    }
}

// ---------- 1. Registrera kund ----------
async Task<Customer?> RegisterCustomer()
{
    string name = Input.ReadText("Namn: ");
    string email = Input.ReadText("E-post: ");
    string phone = Input.ReadText("Telefon: ");

    try
    {
        int id = await customers.CreateCustomerAsync(name, email, phone);
        Console.WriteLine($"Kunden registrerades med id {id} och är nu vald.");
        return new Customer(id, name, email, phone);
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // 19 = constraint, här UNIQUE på email
    {
        Console.WriteLine("Det finns redan en kund med den e-postadressen.");
        return currentCustomer;
    }
}

// ---------- 2. Välj kund ----------
async Task<Customer?> SelectCustomer()
{
    string search = Input.ReadText("Sök kund (del av namnet): ");
    var found = await customers.SearchCustomersAsync(search);

    if (found.Count == 0)
    {
        Console.WriteLine("Ingen kund hittades.");
        return null;
    }

    for (int i = 0; i < found.Count; i++)
    {
        var c = found[i];
        Console.WriteLine($"{i + 1}. {c.Name}, {c.Email}, {c.PhoneNumber}");
    }

    int index = Input.ReadInt("Välj nummer (0 = avbryt): ", 0, found.Count);
    if (index == 0) return null;

    Console.WriteLine($"{found[index - 1].Name} är vald.");
    return found[index - 1];
}

// ---------- 3. Sök lediga rum och boka ----------
async Task SearchAndBook()
{
    if (currentCustomer == null)
    {
        Console.WriteLine("Välj eller registrera en kund först.");
        return;
    }

    string city = Input.ReadText("Ort: ");
    DateOnly checkIn = Input.ReadDate("Incheckning (ÅÅÅÅ-MM-DD): ");
    DateOnly checkOut = Input.ReadDate("Utcheckning (ÅÅÅÅ-MM-DD): ");
    while (checkOut <= checkIn)
    {
        Console.WriteLine("  Utcheckningen måste vara efter incheckningen.");
        checkOut = Input.ReadDate("Utcheckning (ÅÅÅÅ-MM-DD): ");
    }
    int guests = Input.ReadInt("Antal personer: ", 1, 20);

    Console.WriteLine("-- Valfria filter --");
    bool needPool = Input.ReadYesNo("Måste ha pool?");
    bool needRestaurant = Input.ReadYesNo("Måste ha restaurang?");
    int? maxBeach = Input.ReadOptionalInt("Max avstånd till strand i meter (Enter = inget filter): ");
    int? maxCenter = Input.ReadOptionalInt("Max avstånd till centrum i meter (Enter = inget filter): ");
    int sort = Input.ReadInt("Sortera: 1 = lägst pris först, 2 = högst omdöme först: ", 1, 2);
    var sortOrder = sort == 2 ? SortOrder.RatingHighestFirst : SortOrder.PriceLowestFirst;

    var found = await rooms.SearchAvailableRoomsAsync(
        city, checkIn, checkOut, guests, needPool, needRestaurant, maxBeach, maxCenter, sortOrder);

    if (found.Count == 0)
    {
        Console.WriteLine("Inga lediga rum matchar sökningen.");
        return;
    }

    Console.WriteLine();
    for (int i = 0; i < found.Count; i++)
    {
        var r = found[i];
        Console.WriteLine($"{i + 1}. {r.AccommodationName}, {r.City} | rum {r.RoomNumber} | " +
                          $"max {r.MaxGuests} pers | {r.PricePerNight} kr/natt | omdöme {r.Rating}/5");
    }

    int index = Input.ReadInt("Boka rum nummer (0 = avbryt): ", 0, found.Count);
    if (index == 0) return;

    var room = found[index - 1];
    int? bookingId = await bookings.CreateBookingAsync(currentCustomer.Id, room.RoomId, checkIn, checkOut, guests);

    if (bookingId == null)
    {
        Console.WriteLine("Bokningen nekades: rummet är inte längre ledigt för perioden.");
        return;
    }

    int nights = checkOut.DayNumber - checkIn.DayNumber;
    Console.WriteLine($"Bokning bekräftad! Bokningsnummer: {bookingId}");
    Console.WriteLine($"{nights} nätter × {room.PricePerNight} kr = {nights * room.PricePerNight} kr");
}

// ---------- 4. Visa kundens bokningar ----------
async Task<List<BookingInfo>> ShowBookings()
{
    if (currentCustomer == null)
    {
        Console.WriteLine("Välj eller registrera en kund först.");
        return [];
    }

    var list = await bookings.GetBookingsForCustomerAsync(currentCustomer.Id);
    if (list.Count == 0)
    {
        Console.WriteLine($"{currentCustomer.Name} har inga bokningar.");
        return list;
    }

    Console.WriteLine($"Bokningar för {currentCustomer.Name}:");
    foreach (var b in list)
    {
        Console.WriteLine($"Nr {b.Id}: {b.AccommodationName}, rum {b.RoomNumber} | " +
                          $"{b.StartDate} – {b.EndDate} ({b.Nights} nätter) | " +
                          $"{b.NumberOfGuests} pers | totalt {b.TotalPrice} kr");
    }
    return list;
}

// ---------- 5. Ändra antal personer ----------
async Task ChangeGuests()
{
    var list = await ShowBookings();
    if (list.Count == 0) return;

    int bookingId = Input.ReadInt("Bokningsnummer att ändra: ", 1, int.MaxValue);
    int guests = Input.ReadInt("Nytt antal personer: ", 1, 20);

    int changed = await bookings.UpdateGuestsAsync(bookingId, currentCustomer!.Id, guests);
    Console.WriteLine(changed == 1
        ? "Antalet personer är ändrat."
        : "Ändringen nekades: bokningen finns inte för kunden, eller så ryms inte så många i rummet.");
}

// ---------- 6. Avboka ----------
async Task CancelBooking()
{
    var list = await ShowBookings();
    if (list.Count == 0) return;

    int bookingId = Input.ReadInt("Bokningsnummer att avboka: ", 1, int.MaxValue);
    if (!Input.ReadYesNo($"Vill du verkligen avboka nr {bookingId}?")) return;

    int deleted = await bookings.DeleteBookingAsync(bookingId, currentCustomer!.Id);
    Console.WriteLine(deleted == 1
        ? "Bokningen är avbokad och rummet är ledigt igen."
        : "Hittade ingen sådan bokning för kunden.");
}
