using System;
using System.Globalization;
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
            using (var connection = new NpgsqlConnection(connectionString))
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
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = @"
            SELECT g.name, ge.name AS genre_name, p.name AS platform_name 
            FROM game g
            JOIN genre ge ON g.genre_id = ge.id
            JOIN platform p ON g.platform_id = p.id";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("No games found.");
                                return;
                            }

                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["name"]}, {reader["genre_name"]}, {reader["platform_name"]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errror: {ex.Message}");
            }
        }

        //Добавление игры
        public void addGame()
        {

            Console.WriteLine("Adding a new game:");
            string title = Program.GetInput("Enter a game title: ", 30, 3);
            int genreId = getGenreId();
            int platformId = getPlatformId();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var query = "INSERT INTO Game (name, genre_id, platform_id) VALUES (@name, @genre_id, @platform_id)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", title);
                    command.Parameters.AddWithValue("@genre_id", genreId);
                    command.Parameters.AddWithValue("@platform_id", platformId);
                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Game added successfully!");
                    }
                    catch (NpgsqlException e)
                    {
                        Console.WriteLine($"Failed to add game. Error: {e.Message}");
                    }
                }
            }
        }
        //Удаление игры
        public void removeGame()
        {
            Console.WriteLine("Deleting a game...");
            int gameId = GetGameId();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var queryLibrary = "DELETE FROM library WHERE game_id = @game_id";
                using (var commandLibrary = new NpgsqlCommand(queryLibrary, connection))
                {
                    commandLibrary.Parameters.AddWithValue("@game_id", gameId);
                    commandLibrary.ExecuteNonQuery(); 
                }

                var queryGame = "DELETE FROM game WHERE id = @id";
                using (var commandGame = new NpgsqlCommand(queryGame, connection))
                {
                    commandGame.Parameters.AddWithValue("@id", gameId);

                    try
                    {
                        int result = commandGame.ExecuteNonQuery();
                        if (result > 0)
                            Console.WriteLine("Game deleted successfully.");
                        else
                            Console.WriteLine("Game not found.");
                    }
                    catch (NpgsqlException e)
                    {
                        Console.WriteLine($"Failed to delete game. Error: {e.Message}");
                    }
                }
            }
        }
        public void addFriend(int currentUserId)
        {
            string friendNickname = Program.GetInput("Enter your friend's nickname: ", 30, 3);

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                //Проверяем, существует ли пользователь с таким никнеймом
                var checkUserQuery = "SELECT id FROM users WHERE nickname = @nickname";
                int friendUserId;
                using (var checkUserCommand = new NpgsqlCommand(checkUserQuery, connection))
                {
                    checkUserCommand.Parameters.AddWithValue("@nickname", friendNickname);
                    var result = checkUserCommand.ExecuteScalar();

                    if (result != null)
                    {
                        friendUserId = Convert.ToInt32(result);
                    }
                    else
                    {
                        Console.WriteLine("No user found with that nickname.");
                        return;
                    }
                }

                //Проверяем, не добавлен ли уже этот пользователь в друзья
                var checkFriendQuery = "SELECT COUNT(*) FROM friends WHERE userid = @userid AND friendid = @friendid";
                using (var checkFriendCommand = new NpgsqlCommand(checkFriendQuery, connection))
                {
                    checkFriendCommand.Parameters.AddWithValue("@userid", currentUserId);
                    checkFriendCommand.Parameters.AddWithValue("@friendid", friendUserId);
                    var friendCount = (long)checkFriendCommand.ExecuteScalar();

                    if (friendCount > 0)
                    {
                        Console.WriteLine("This user is already your friend.");
                        return;
                    }
                }

                // Добавляем запись о дружбе
                var insertFriendQuery = "INSERT INTO friends (userid, friendid) VALUES (@userid, @friendid)";
                using (var insertFriendCommand = new NpgsqlCommand(insertFriendQuery, connection))
                {
                    insertFriendCommand.Parameters.AddWithValue("@userid", currentUserId);
                    insertFriendCommand.Parameters.AddWithValue("@friendid", friendUserId);

                    try
                    {
                        insertFriendCommand.ExecuteNonQuery();
                        Console.WriteLine("Friend added successfully.");
                    }
                    catch (NpgsqlException e)
                    {
                        Console.WriteLine($"Failed to add friend. Error: {e.Message}");
                    }
                }
            }
        }
        public int getGenreId()
        {
            string genreName = Program.GetInput("Enter game genre: ", 30, 1);
            int genreId = 0;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var checkQuery = "SELECT id FROM genre WHERE name = @name";
                using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@name", genreName);
                    var result = checkCommand.ExecuteScalar();

                    if (result != null)
                    {
                        genreId = Convert.ToInt32(result);
                    }
                    else
                    {
                        var insertQuery = "INSERT INTO genre (name) VALUES (@name) RETURNING id";
                        using (var insertCommand = new NpgsqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@name", genreName);
                            genreId = Convert.ToInt32(insertCommand.ExecuteScalar());
                        }
                    }
                }
            }
            return genreId;
        }
        public int GetGameId()
        {
            Console.WriteLine("Enter the game title: ");
            string title = Console.ReadLine().Trim();
            int gameId = -1; 

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT id FROM game WHERE name = @name";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", title);

                    var result = command.ExecuteScalar();
                    if (result != null)
                        gameId = Convert.ToInt32(result);
                    else
                        Console.WriteLine("Game not found.");
                }
            }
            return gameId;
        }
        public int getPlatformId()
        {
            string platformName = Program.GetInput("Enter game platform: ", 30, 1);
            int platformId = 0;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var checkQuery = "SELECT id FROM platform WHERE name = @name";
                using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@name", platformName);
                    var result = checkCommand.ExecuteScalar();

                    if (result != null)
                        platformId = Convert.ToInt32(result);
                    else
                    {
                        var insertQuery = "INSERT INTO platform (name) VALUES (@name) RETURNING id";
                        using (var insertCommand = new NpgsqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@name", platformName);
                            platformId = Convert.ToInt32(insertCommand.ExecuteScalar());
                        }
                    }
                }
            }
            return platformId;
        }
    }
}