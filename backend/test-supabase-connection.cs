using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Supabase Connection...");
        
        // Your Supabase connection string from appsettings.json
        string connectionString = "Host=db.ekmnikbqyhgiomqkgcet.supabase.co;Database=postgres;Username=postgres;Password=postgres;Port=5432;SSL Mode=Require;Trust Server Certificate=true";
        
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                Console.WriteLine("Attempting to connect to Supabase...");
                await connection.OpenAsync();
                Console.WriteLine("✅ Successfully connected to Supabase!");
                
                // Test a simple query
                using (var command = new NpgsqlCommand("SELECT version();", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    Console.WriteLine($"Database version: {result}");
                }
                
                // Test if we can query the database
                using (var command = new NpgsqlCommand("SELECT current_database(), current_user;", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Console.WriteLine($"Connected to database: {reader.GetString(0)}");
                            Console.WriteLine($"Connected as user: {reader.GetString(1)}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to connect to Supabase: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine("\nTest completed.");
    }
} 