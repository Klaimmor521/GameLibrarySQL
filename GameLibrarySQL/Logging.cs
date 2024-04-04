using System;
using Npgsql;

namespace GameLibrarySQL
{
    internal class Logging
    {
        private readonly string connectionString;
        public Logging(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public bool loginUser(string login, string password)
        {
            using(var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    int userExists = Convert.ToInt32(command.ExecuteScalar());

                    if (userExists > 0)
                    {
                        //Пользователь найден
                        return true;
                    }
                    else
                    {
                        //Пользователь не найден
                        return false;
                    }
                }
            }
        }
        public void viewAllData()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM Game";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Вывод данных
                            Console.WriteLine($"{reader["name"]}, {reader["genre_id"]}, {reader["platform_id"]}");
                        }
                    }
                }
            }
        }
        public void addGame()
        {

        }

        public void removeGame()
        {

        }
    }
}