using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

/*
 Реально работающий код 
вводим имя базы
поиск происходит по списку из App.config в корне программы
выводит имя сервера и размер базы и лога
в случае ошибки соединения с скл сервером, ищет дальше по списку
Firstoff 17.03.2023 21:30 MSK
 */

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string databaseName = GetDatabaseNameFromUser();
            List<string> serverNames = GetServerNamesFromConfig();

            foreach (string serverName in serverNames)
            {
                try
                {
                    string connectionString = GetConnectionStringFromConfig(serverName);
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = $"SELECT name, size FROM sys.master_files WHERE DB_NAME(database_id) = '{databaseName}'";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            SqlDataReader reader = command.ExecuteReader();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string name = reader.GetString(0);
                                    int size = reader.GetInt32(1);
                                    Console.WriteLine($"Server: {serverName}, Database: {name}, Size: {size} KB");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to server {serverName}: {ex.Message}");
                }
            }

            Console.ReadLine();
        }

        static string GetDatabaseNameFromUser()
        {
            Console.Write("Enter database name: ");
            return Console.ReadLine();
        }

        static List<string> GetServerNamesFromConfig()
        {
            string serversString = ConfigurationManager.AppSettings["Servers"];
            string[] serverNames = serversString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(serverNames);
        }

        static string GetConnectionStringFromConfig(string serverName)
        {
            string connectionStringTemplate = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return string.Format(connectionStringTemplate, serverName);
        }
    }
}
