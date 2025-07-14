using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Supabase Connection with updated parameters...");
        
        // Use the same connection string format as your application
        string connectionString = "Host=db.ekmnikbqyhgiomqkgcet.supabase.co;Database=postgres;Username=postgres;Password=postgres;Port=5432;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Timeout=30;CommandTimeout=30;InternalCommandTimeout=60";
        
        Console.WriteLine($"Testing connection with updated parameters...");
        
        try
        {
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
                
                Console.WriteLine("🎉 Connection successful! Your Supabase is working with the updated parameters.");
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