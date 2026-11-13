using Microsoft.Data.Sqlite;

namespace HolidayMaker;

public static class Database
{
    private const string FileName = "holiday_maker.db";

    // Öppnar en anslutning till databasfilen.
    // "Foreign Keys=True" slår på kontrollen av främmande nycklar,
    // eftersom SQLite annars inte kontrollerar dem.
    public static async Task<SqliteConnection> OpenAsync()
    {
        string path = FindDatabaseFile();
        var connection = new SqliteConnection($"Data Source={path};Foreign Keys=True");
        await connection.OpenAsync();
        return connection;
    }

    // Letar efter holiday_maker.db i programmets mapp och uppåt i mappträdet,
    // så att det fungerar både med "dotnet run" och från Rider/Visual Studio.
    private static string FindDatabaseFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, FileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
            dir = dir.Parent;
        }
        throw new FileNotFoundException(
            $"Hittade inte {FileName}. Lägg filen i projektmappen (samma mapp som HolidayMaker.csproj).");
    }
}
