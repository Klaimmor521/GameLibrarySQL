using System;
using System.Globalization;
using Npgsql;

namespace GameLibrarySQL
{
    internal class Logging
    {
        public int currentUserId { get; private set; }
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

                var query = "SELECT id FROM users WHERE login = @login AND password = @password";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        this.currentUserId = Convert.ToInt32(result);
                        //Console.WriteLine($"Logged in with user ID: {this.currentUserId}"); //Проверка Id
                        return true;
                    }
                    else
                    {
                        this.currentUserId = 0;
                        Console.WriteLine("Login failed. User ID not found.");
                        return false;
                    }
                }
            }
        }
        public void viewAllData(int currentUserId)
        {
            //Console.WriteLine($"Current User ID: {currentUserId}");  //Проверка Id
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT g.name AS game_name, ge.name AS genre_name, p.name AS platform_name 
                    FROM library l
                    JOIN game g ON l.game_id = g.id
                    JOIN genre ge ON g.genre_id = ge.id
                    JOIN platform p ON g.platform_id = p.id
                    WHERE l.user_id = @currentUserId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@currentUserId", currentUserId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("No games found.");
                                return;
                            }
                            Console.WriteLine("---Games---");
                            while (reader.Read())
                            {
                                Console.WriteLine($"{reader["game_name"]}, {reader["genre_name"]}, {reader["platform_name"]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        //Добавление игры
        public void addGame(int currentUserId)
        {
            Console.WriteLine("Adding a new game...");
            string title = Program.GetInput("Enter a game title: ", 30, 3);
            int genreId = getGenreId();
            int platformId = getPlatformId();
            int gameId = 0;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                //Добавление игры в таблицу `game`
                var queryGame = "INSERT INTO game (name, genre_id, platform_id) VALUES (@name, @genre_id, @platform_id) RETURNING id";

                using (var commandGame = new NpgsqlCommand(queryGame, connection))
                {
                    commandGame.Parameters.AddWithValue("@name", title);
                    commandGame.Parameters.AddWithValue("@genre_id", genreId);
                    commandGame.Parameters.AddWithValue("@platform_id", platformId);
                    try
                    {
                        //Выполняем запрос и получаем ID добавленной игры
                        gameId = (int)commandGame.ExecuteScalar();
                        //Console.WriteLine("Game added to game table successfully!"); //Проверка
                    }
                    catch (NpgsqlException e)
                    {
                        Console.WriteLine($"Failed to add game to game table. Error: {e.Message}");
                        return;
                    }
                }

                //Добавление в таблицу `library`
                var queryLibrary = "INSERT INTO library (user_id, game_id) VALUES (@userId, @gameId)";

                using (var commandLibrary = new NpgsqlCommand(queryLibrary, connection))
                {
                    commandLibrary.Parameters.AddWithValue("@userId", currentUserId);
                    commandLibrary.Parameters.AddWithValue("@gameId", gameId);
                    try
                    {
                        commandLibrary.ExecuteNonQuery();
                        Console.WriteLine("Game added to user's library successfully!");
                    }
                    catch (NpgsqlException e)
                    {
                        Console.WriteLine($"Failed to add game to user's library. Error: {e.Message}");
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
                            Console.WriteLine("Game deleted successfully!");
                        else
                            Console.WriteLine("Game not found!");
                    }
                    catch (NpgsqlException e)
                    {
                        Console.WriteLine($"Failed to delete game. Error: {e.Message}");
                    }
                }
            }
        }
        //Добавление друга
        public void addFriend(int currentUserId)
        {
            Console.WriteLine("Enter the nickname of the friend you want to add:");
            string friendNickname = Console.ReadLine().Trim();

            //Проверка на пустой ввод
            if (string.IsNullOrEmpty(friendNickname))
            {
                Console.WriteLine("Nickname cannot be empty!");
                return;
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                //Сначала получаем id user по введенному никнейму
                int friendUserId = -1;
                using (var cmdGetUserId = new NpgsqlCommand("SELECT id FROM users WHERE nickname = @friendNickname", connection))
                {
                    cmdGetUserId.Parameters.AddWithValue("@friendNickname", friendNickname);
                    object result = cmdGetUserId.ExecuteScalar();
                    if (result != null)
                    {
                        friendUserId = Convert.ToInt32(result);
                    }
                    else
                    {
                        Console.WriteLine("No user found with this nickname!");
                        return;
                    }
                }
                //Проверяем, не добавлен ли уже этот пользователь в друзья текущего пользователя
                using (var cmdCheckFriend = new NpgsqlCommand("SELECT COUNT(*) FROM friends WHERE user_id = @currentUserId AND friend_user_id = @friendUserId", connection))
                {
                    cmdCheckFriend.Parameters.AddWithValue("@currentUserId", currentUserId);
                    cmdCheckFriend.Parameters.AddWithValue("@friendUserId", friendUserId);
                    int friendExists = Convert.ToInt32(cmdCheckFriend.ExecuteScalar());
                    if (friendExists > 0)
                    {
                        Console.WriteLine("This friend has already been added!");
                        return;
                    }
                }

                //Добавляем друга
                using (var cmdAddFriend = new NpgsqlCommand("INSERT INTO friends (user_id, friend_user_id) VALUES (@currentUserId, @friendUserId)", connection))
                {
                    cmdAddFriend.Parameters.AddWithValue("@currentUserId", currentUserId);
                    cmdAddFriend.Parameters.AddWithValue("@friendUserId", friendUserId);

                    int rowsAffected = cmdAddFriend.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        Console.WriteLine("Friend added successfully!");
                    else
                        Console.WriteLine("Failed to add friend!");
                }
            }
        }
        //Удалить друга
        public void deleteFriend(int currentUserId)
        {
            Console.WriteLine("Enter the nickname of the friend you want to remove: ");
            string friendNickname = Console.ReadLine().Trim(); //Trim спасает от лишних пробелов

            //Проверка на пустой ввод
            if (string.IsNullOrEmpty(friendNickname))
            {
                Console.WriteLine("Nickname cannot be empty!");
                return;
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int friendUserId = -1;
                using (var getUserIdCommand = new NpgsqlCommand("SELECT id FROM users WHERE nickname = @friendNickname", connection))
                {
                    getUserIdCommand.Parameters.AddWithValue("@friendNickname", friendNickname);

                    object result = getUserIdCommand.ExecuteScalar();
                    if (result != null)
                    {
                        friendUserId = Convert.ToInt32(result);
                    }
                    else
                    {
                        Console.WriteLine("No user found with that nickname!");
                        return;
                    }
                }

                //Удаление друга
                var deleteFriendQuery = "DELETE FROM friends WHERE user_id = @currentUserId AND friend_user_id = @friendUserId";
                using (var deleteFriendCommand = new NpgsqlCommand(deleteFriendQuery, connection))
                {
                    deleteFriendCommand.Parameters.AddWithValue("@currentUserId", currentUserId);
                    deleteFriendCommand.Parameters.AddWithValue("@friendUserId", friendUserId);

                    int rowsAffected = deleteFriendCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        Console.WriteLine("Friend removed successfully!");
                    else
                        Console.WriteLine("No friend found with that nickname under your friends!");
                }
            }
        }
        //Показ всех друзей
        public void seeAllFriends(int currentUserId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var seeAllFriendsQuery = @"SELECT u.nickname FROM friends f JOIN users u ON f.friend_user_id = u.id WHERE f.user_id = @currentUserId";
                using (var seeAllFriendsCommand = new NpgsqlCommand(seeAllFriendsQuery, connection))
                {
                    seeAllFriendsCommand.Parameters.AddWithValue("@currentUserId", currentUserId);

                    using (var reader = seeAllFriendsCommand.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            Console.WriteLine("You have no friends added!");
                        else
                        {
                            Console.WriteLine("Your friends: ");
                            while (reader.Read())
                            {
                                Console.WriteLine(reader["nickname"].ToString());
                            }
                        }
                    }
                }
            }
        }
        public void showAllUsers()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT nickname, login, email FROM users";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            Console.WriteLine("No users found!");
                        else
                        {
                            Console.WriteLine("All users that we have in our GAME LIBRARY: ");
                            while (reader.Read())
                            {
                                string nickname = reader["nickname"].ToString();
                                string login = reader["login"].ToString();
                                string email = reader["email"].ToString();
                                Console.WriteLine($"Nickname: {nickname}, Login: {login}, Email: {email}");
                            }
                        }
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
                        genreId = Convert.ToInt32(result);
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