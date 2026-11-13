using Microsoft.Data.Sqlite;

namespace HolidayMaker;

public class BookingRepository(SqliteConnection connection)
{
    // SQL-sats 4: Skapa bokning.
    // INSERT ... SELECT ... WHERE gör att raden bara läggs in om rummet
    // fortfarande är ledigt och rymmer sällskapet. Tillgängligheten
    // kontrolleras alltså igen i samma ögonblick som bokningen sparas.
    // Returnerar det nya bokningsnumret, eller null om bokningen nekades.
    public async Task<int?> CreateBookingAsync(
        int customerId, int roomId, DateOnly checkIn, DateOnly checkOut, int guests)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO booking (customer_id, room_id, start_date, end_date, number_of_guests)
            SELECT @customer_id, @room_id, @check_in, @check_out, @guests
            WHERE @guests <= (SELECT max_guests FROM room WHERE id = @room_id)
              AND NOT EXISTS (
                  SELECT 1 FROM booking
                  WHERE room_id = @room_id
                    AND start_date < @check_out
                    AND end_date   > @check_in);
            """;
        command.Parameters.AddWithValue("@customer_id", customerId);
        command.Parameters.AddWithValue("@room_id", roomId);
        command.Parameters.AddWithValue("@check_in", checkIn.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@check_out", checkOut.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@guests", guests);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            return null; // Rummet hann bli upptaget, eller sällskapet var för stort
        }

        await using var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT last_insert_rowid();";
        return Convert.ToInt32(await idCommand.ExecuteScalarAsync());
    }

    // SQL-sats 5: Visa kundens bokningar med totalpris (nätter × pris per natt).
    public async Task<List<BookingInfo>> GetBookingsForCustomerAsync(int customerId)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT b.id, a.name, r.room_number, b.start_date, b.end_date, b.number_of_guests,
                   CAST(julianday(b.end_date) - julianday(b.start_date) AS INTEGER) AS nights,
                   CAST(julianday(b.end_date) - julianday(b.start_date) AS INTEGER) * r.price_per_night AS total_price
            FROM booking b
            JOIN room r          ON r.id = b.room_id
            JOIN accommodation a ON a.id = r.accommodation_id
            WHERE b.customer_id = @customer_id
            ORDER BY b.start_date;
            """;
        command.Parameters.AddWithValue("@customer_id", customerId);

        await using var reader = await command.ExecuteReaderAsync();
        var bookings = new List<BookingInfo>();
        while (await reader.ReadAsync())
        {
            bookings.Add(new BookingInfo(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetDouble(7)));
        }
        return bookings;
    }

    // SQL-sats 6: Ändra antal personer, bara om det nya antalet ryms i rummet.
    // customer_id i WHERE gör att man bara kan ändra den valda kundens bokningar.
    // Returnerar antal ändrade rader: 1 = ok, 0 = nekad eller fanns inte.
    public async Task<int> UpdateGuestsAsync(int bookingId, int customerId, int guests)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE booking
            SET number_of_guests = @guests
            WHERE id = @booking_id
              AND customer_id = @customer_id
              AND @guests <= (SELECT max_guests FROM room WHERE room.id = booking.room_id);
            """;
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@booking_id", bookingId);
        command.Parameters.AddWithValue("@customer_id", customerId);
        return await command.ExecuteNonQueryAsync();
    }

    // SQL-sats 7: Avboka. Returnerar antal borttagna rader (1 = ok, 0 = fanns inte).
    public async Task<int> DeleteBookingAsync(int bookingId, int customerId)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM booking
            WHERE id = @booking_id
              AND customer_id = @customer_id;
            """;
        command.Parameters.AddWithValue("@booking_id", bookingId);
        command.Parameters.AddWithValue("@customer_id", customerId);
        return await command.ExecuteNonQueryAsync();
    }
}
