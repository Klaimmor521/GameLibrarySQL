using System;
using System.Diagnostics.Eventing.Reader;
using Npgsql;

namespace GameLibrarySQL
{
    internal class Registration
    {
        private readonly string connectionString;
        public Registration(string connectionString) 
        {
            this.connectionString = connectionString;
        }
        public bool RegisterUser(string nickname, string login, string password, string email)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                //Проверяем, существует ли уже пользователь с таким же nickname или email или login
                using (var check = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE nickname = @nickname OR email = @email OR login = @login", connection))
                {
                    check.Parameters.AddWithValue("@nickname", nickname);
                    check.Parameters.AddWithValue("@login", login);
                    check.Parameters.AddWithValue("@email", email);

                    int userExists = Convert.ToInt32(check.ExecuteScalar());
                    if (userExists > 0)
                    {
                        //Пользователь уже существует, выбрасываем исключение
                        return false;
                    }
                }

                using (var command = new NpgsqlCommand("INSERT INTO Users (nickname, login, password, email) VALUES (@nickname, @login, @password, @email)", connection))
                {
                    command.Parameters.AddWithValue("@nickname", nickname);
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@email", email);

                    try
                    {
                        command.ExecuteNonQuery();
                        return true; //Регистрация прошла успешно
                    }
                    catch (NpgsqlException e)
                    {
                        Console.WriteLine(e.Message);
                        return false; //Регистрация не удалась
                    }
                }
            }
        }
    }
}