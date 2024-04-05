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

            int chose;
            string nickname;
            string login;
            string password;
            string email;

            //Взаимодействие с пользователем
            while (true)
            {
                Console.WriteLine("What you wanna do?\nSing up(1)\nLogin(2)\nExit(3)");
                chose = Convert.ToInt32(Console.ReadLine());
                switch(chose)
                {
                    //Регистрация
                    case 1:
                        nickname = GetInput("Enter your nickname (Maximum 30 characters and minimum 3): ", 30, 3);
                        login = GetInput("Enter your login (Maximum 30 characters and minimum 3): ", 30, 3);
                        password = GetInput("Enter your password (Maximum 30 characters and minimum 3): ", 30, 3);
                        email = GetInput("Enter your email (Maximum 30 characters and minimum 3): ", 30, 3);

                        if (registration.RegisterUser(nickname, login, password, email))
                        {
                            Console.WriteLine("The registration went successfully!");
                        }
                        else
                            Console.WriteLine("Something went wrong or your account are already has on library!");
                        break;

                    //Войти в аккаунт
                    case 2:
                        Console.WriteLine("Enter your login: ");
                        login = Console.ReadLine();
                        Console.WriteLine("Enter your password: ");
                        password = Console.ReadLine();

                        if(logging.loginUser(login, password))
                        {
                            Console.WriteLine("Great!");
                            userActions();
                        }
                        else
                            Console.WriteLine("This user is not existing!");
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
        }
        public static string GetInput(string prompt, int maxLength, int minLength)
        {
            string input;
            do
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine();
                if(input.Length > maxLength)
                    Console.WriteLine($"Input longer than {maxLength} characters! Try again");
                else if(input.Length < minLength)
                    Console.WriteLine($"Input less than {minLength} characters! Try again");
            }
            while(input.Length > maxLength || input.Length < minLength);
            return input;
        }
        static void userActions()
        {
            Logging logging = new Logging(connectionString);
            int action;
            while(true)
            {
                Console.WriteLine("You are in your game library!\nView all data(1)\nAdd game(2)\nDelete game(3)\nAdd a friend(4)\nDelete a friend(5)\nSee all friends(6)\nExit(7)");
                action = Convert.ToInt32(Console.ReadLine());
                switch (action) 
                {
                    case 1:
                        logging.viewAllData(); 
                        break;
                    case 2:
                        logging.addGame();
                        break;
                    case 3:
                        logging.removeGame();
                        break;
                    case 4:
                        logging.addFriend(1);
                        break;
                    case 5:
                        //deleteFriend();
                        break;
                    case 6:
                        //seeFriends();
                        break;
                    case 7:
                        return;
                    default:
                        Console.WriteLine("Invalid option selected!");
                        break;
                }
            }
        }
    }
}