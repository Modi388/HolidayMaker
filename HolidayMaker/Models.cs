namespace HolidayMaker;

// En rad i tabellen customer
public record Customer(int Id, string Name, string Email, string PhoneNumber);

// En träff i sökningen efter lediga rum (data från room + accommodation)
public record AvailableRoom(
    int RoomId,
    string AccommodationName,
    string City,
    string RoomNumber,
    int MaxGuests,
    double PricePerNight,
    int Rating);

// En bokning som den visas för kunden, med uträknat totalpris
public record BookingInfo(
    int Id,
    string AccommodationName,
    string RoomNumber,
    string StartDate,
    string EndDate,
    int NumberOfGuests,
    int Nights,
    double TotalPrice);
