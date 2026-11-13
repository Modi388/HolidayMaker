using Microsoft.Data.Sqlite;

namespace HolidayMaker;

public enum SortOrder
{
    PriceLowestFirst,
    RatingHighestFirst
}

public class RoomRepository(SqliteConnection connection)
{
    // SQL-sats 3: Sök lediga rum.
    // Valfria filter skickas som 0/1 eller NULL, så att samma SQL fungerar
    // oavsett vilka filter operatören väljer.
    public async Task<List<AvailableRoom>> SearchAvailableRoomsAsync(
        string city, DateOnly checkIn, DateOnly checkOut, int guests,
        bool needPool, bool needRestaurant, int? maxBeach, int? maxCenter,
        SortOrder sortOrder)
    {
        // Sorteringen väljs mellan två FASTA strängar. Användarens inmatning
        // hamnar aldrig i SQL-texten.
        string orderBy = sortOrder == SortOrder.RatingHighestFirst
            ? "a.rating DESC, r.price_per_night ASC"
            : "r.price_per_night ASC";

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT r.id, a.name, a.city, r.room_number, r.max_guests, r.price_per_night, a.rating
            FROM room r
            JOIN accommodation a ON a.id = r.accommodation_id
            JOIN facility f      ON f.accommodation_id = a.id
            WHERE a.city = @city
              AND r.max_guests >= @guests
              AND NOT EXISTS (
                  SELECT 1 FROM booking b
                  WHERE b.room_id = r.id
                    AND b.start_date < @check_out
                    AND b.end_date   > @check_in)
              AND (@need_pool = 0 OR f.has_pool = 1)
              AND (@need_restaurant = 0 OR f.has_restaurant = 1)
              AND (@max_beach IS NULL OR a.distance_to_beach_m <= @max_beach)
              AND (@max_center IS NULL OR a.distance_to_center_m <= @max_center)
            ORDER BY {orderBy};
            """;
        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@check_in", checkIn.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@check_out", checkOut.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@need_pool", needPool ? 1 : 0);
        command.Parameters.AddWithValue("@need_restaurant", needRestaurant ? 1 : 0);
        // C#:s null måste skickas som DBNull.Value till databasen
        command.Parameters.AddWithValue("@max_beach", (object?)maxBeach ?? DBNull.Value);
        command.Parameters.AddWithValue("@max_center", (object?)maxCenter ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync();
        var rooms = new List<AvailableRoom>();
        while (await reader.ReadAsync())
        {
            rooms.Add(new AvailableRoom(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetDouble(5),
                reader.GetInt32(6)));
        }
        return rooms;
    }
}
