using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Supabase Connection...");
        
        // Your exact PostgreSQL URL format
        string postgresUrl = "postgresql://postgres:postgres@db.ekmnikbqyhgiomqkgcet.supabase.co:5432/postgres";
        
        Console.WriteLine($"Testing connection with URL: {postgresUrl}");
        
        try
        {
            // Parse the PostgreSQL URL and create connection string
            var uri = new Uri(postgresUrl);
            var connectionString = $"Host={uri.Host};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};Port={uri.Port};SSL Mode=Require;Trust Server Certificate=true";
            
            Console.WriteLine($"Parsed connection string: {connectionString}");
            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                Console.WriteLine("Attempting to connect...");
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
                
                Console.WriteLine("🎉 Connection successful! Your Supabase is working.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to connect: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine("\nTest completed.");
    }
} 