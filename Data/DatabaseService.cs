using System;
using System.Data;
using System.Data.SqlClient;

namespace Bot.Api.Data
{
    public class DatabaseService
    {
        private readonly string connectionString;

        public DatabaseService(string serverName, string databaseName, string username, string password)
        {
            // Create the connection string
            connectionString = $"Server=tcp:{serverName},1433;Initial Catalog={databaseName};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        public void ExecuteQuery(string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public SqlDataReader ExecuteReader(string query)
        {
            var connection = new SqlConnection(connectionString);
            
            Console.WriteLine(connection);
            
            connection.Open();

            var command = new SqlCommand(query, connection);

            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);

            return reader;
        }

    }
}
