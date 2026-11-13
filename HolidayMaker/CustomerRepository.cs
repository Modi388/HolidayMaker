using Microsoft.Data.Sqlite;

namespace HolidayMaker;

public class CustomerRepository(SqliteConnection connection)
{
    // SQL-sats 1: Registrera kund. Returnerar den nya kundens id.
    public async Task<int> CreateCustomerAsync(string name, string email, string phone)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO customer (name, email, phone_number)
            VALUES (@name, @email, @phone);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@phone", phone);

        var newId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newId);
    }

    // SQL-sats 2: Sök kund på del av namnet.
    public async Task<List<Customer>> SearchCustomersAsync(string search)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, name, email, phone_number
            FROM customer
            WHERE name LIKE '%' || @search || '%'
            ORDER BY name;
            """;
        command.Parameters.AddWithValue("@search", search);

        await using var reader = await command.ExecuteReaderAsync();
        var customers = new List<Customer>();
        while (await reader.ReadAsync())
        {
            customers.Add(new Customer(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)));
        }
        return customers;
    }
}
