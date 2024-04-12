using System;
using System.Data;
using Npgsql;

namespace GameLibrarySQL
{
    internal class Program
    {
        static string connectionString = "Host = 92.118.170.201; Port = 5432; Username = username19; Password = Gw3MFC6H; Database = username19_db";
        static void Main(string[] args)
        {
            Registration registration = new Registration(connectionString);
            Logging logging = new Logging(connectionString);

            string nickname;
            string login;
            string password;
            string email;

            //Взаимодействие с пользователем
            while (true)
            {
                Console.WriteLine("-------------LOGIN / SIGN UP-----------");
                Console.WriteLine("Sign up(1)\tLogin(2)\tExit(3)");
                string input = Console.ReadLine();
                int chose;
                if (int.TryParse(input, out chose))
                {
                    switch (chose)
                    {
                        //Регистрация
                        case 1:
                            Console.WriteLine("-----------SIGN UP-----------");
                            nickname = GetInput("Your nickname (Maximum 30 characters and minimum 3): ", 30, 3);
                            login = GetInput("Your login (Maximum 30 characters and minimum 3): ", 30, 3);
                            password = GetInput("Your password (Maximum 30 characters and minimum 3): ", 30, 3);
                            email = GetInput("Your email (Maximum 30 characters and minimum 3): ", 30, 3);

                            if (registration.RegisterUser(nickname, login, password, email))
                            {
                                Console.WriteLine("The registration went successfully!");
                            }
                            else
                                Console.WriteLine("Something went wrong or your account are already has on game library!");
                            break;
                        //Войти в аккаунт
                        case 2:
                            Console.WriteLine("-----------LOGIN-----------");
                            Console.WriteLine("Enter your login: ");
                            login = Console.ReadLine();
                            Console.WriteLine("Enter your password: ");
                            password = Console.ReadLine();

                            if (logging.loginUser(login, password))
                            {
                                //Console.WriteLine("Great! Your user ID is: " + logging.currentUserId); //Проверка userId
                                userActions(logging.currentUserId);
                            }
                            else
                                Console.WriteLine("This user is not existing or your login and password are incorrect!");
                            break;
                        //Выйти из консоли
                        case 3:
                            return;
                        //Если был выбран неверный вариант ввода
                        default:
                            Console.WriteLine("Invalid option selected!");
                            break;
                    }
                }
                else
                    Console.WriteLine("Please enter a valid number!");
            }
        }
        public static string GetInput(string prompt, int maxLength, int minLength)
        {
            string input;
            do
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine();
                if(input.Length > maxLength)
                    Console.WriteLine($"Input longer than {maxLength} characters! Try again!");
                else if(input.Length < minLength)
                    Console.WriteLine($"Input less than {minLength} characters! Try again!");
            }
            while(input.Length > maxLength || input.Length < minLength);
            return input;
        }
        static void userActions(int currentUserId)
        {
            Logging logging = new Logging(connectionString);
            while(true)
            {
                Console.WriteLine("-----------GAME LIBRARY-----------");
                Console.WriteLine("Show all games(1)"+" | "+"Add new game(2)"+ " | " + "Delete game(3)"+ " | " + "Add new friend(4)"+ " | " + "Delete friend(5)"+ " | " + "See all friends(6)"+ " | " +"Show all users(7)"+" | "+ "Exit(8)");
                string input = Console.ReadLine();
                int action;
                if (int.TryParse(input, out action))
                {
                    switch (action)
                    {
                        case 1:
                            logging.viewAllData(currentUserId);
                            break;
                        case 2:
                            logging.addGame(currentUserId);
                            break;
                        case 3:
                            logging.removeGame();
                            break;
                        case 4:
                            logging.addFriend(currentUserId);
                            break;
                        case 5:
                            logging.deleteFriend(currentUserId);
                            break;
                        case 6:
                            logging.seeAllFriends(currentUserId);
                            break;
                        case 7:
                            logging.showAllUsers();
                            break;
                        case 8:
                            return;
                        default:
                            Console.WriteLine("Invalid option selected!");
                            break;
                    }
                }
                else
                    Console.WriteLine("Please enter a valid number!");
            }
        }
    }
}