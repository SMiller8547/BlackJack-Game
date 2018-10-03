using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Casino;
using Casino.TwentyOne;
using System.Data.SqlClient;
using System.Data;

namespace newtwentyoneGame
{
    class Program
    {
        static void Main(string[] args)
        {
            const string casinoName = "City Casino";
          
            Console.WriteLine(string.Format("Welcome to the {0}. Let's start by telling me your name.",casinoName));
            string playerName = Console.ReadLine();
            if (playerName.ToLower() == "admin")
            {
                List<ExceptionEntity> Exceptions = ReadExceptions();
                foreach (var exception in Exceptions)
                {
                    Console.Write(exception.Id + " | ");
                    Console.Write(exception.ExceptionType + " | ");
                    Console.Write(exception.ExceptionMessage + " | ");
                    Console.Write(exception.TimeStamp + " | ");
                    Console.WriteLine();
                    Console.Read();
                }
                Console.Read();
                return;
            }
            bool validAnswer = false;
            int bank = 0;
            while (!validAnswer)
            {
                Console.WriteLine("And how much money did you bring today?");
                validAnswer = int.TryParse(Console.ReadLine(), out bank);
                if (!validAnswer) Console.WriteLine("Please enter digits only.");

            }

            Console.WriteLine("Hello, {0}. Would you like to join a game of 21 right now?", playerName);
            string answer = Console.ReadLine().ToLower();

            if (answer == "yes" || answer == "yeah" || answer== "y" || answer =="yeah")
            {
                Player player = new Player(playerName, bank);
                player.Id = Guid.NewGuid();
                using (StreamWriter file = new StreamWriter(@"C:\Users\Mr. Miller\Documents\logfile.txt", true))
                {
                    file.WriteLine(player.Id);
                }
                Game game = new TwentyOneGame();
                game += player;
                player.isActivelyPlaying = true;
                while (player.isActivelyPlaying && player.Balance >0)
                {
                    try
                    {
                      game.Play();
                    }
                    catch(FraudException ex)
                    {
                        Console.WriteLine(ex.Message);
                        UpdateDbWithException(ex);
                        Console.Read();
                        return;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("An error has happened. Please contact system admin.");
                        UpdateDbWithException(ex);
                        Console.ReadLine();
                        return;
                    }
                }
                game -= player;
                Console.WriteLine("Thank you for playing.");
            }
            Console.WriteLine("Feel free to look around the casino. Goodbye for now.");
            Console.ReadLine();
            
        }

        private static void UpdateDbWithException (Exception ex)
        {
            //The below uses the ADO.net part of the system library.
            string connectionString = @"Data Source=DESKTOP-D81QDRJ;Initial Catalog=newtwentyoneGame;
                                        Integrated Security=True;Connect Timeout=30;Encrypt=False;
                                        TrustServerCertificate=True;ApplicationIntent=ReadWrite;
                                        MultiSubnetFailover=False";
                
            string queryString = @"INSERT INTO Exceptions (ExceptionType, ExceptionMessage, TimeStamp) VALUES
                                    (@ExceptionType, @ExceptionMessage, @TimeStamp)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                //The below lines are setting the Datatype for the listed parameter (preventing sql injection)
                command.Parameters.Add("@ExceptionType", SqlDbType.VarChar);
                command.Parameters.Add("@ExceptionMessage", SqlDbType.VarChar);
                command.Parameters.Add("@TimeStamp", SqlDbType.DateTime);
                //Now below we are setting the value. 
                command.Parameters["@ExceptionType"].Value = ex.GetType().ToString();
                command.Parameters["@ExceptionMessage"].Value = ex.Message;
                command.Parameters["@TimeStamp"].Value = DateTime.Now;

                connection.Open();
                //You use nonquery below because a query would be a SELECT (QUERIYING) statement
                command.ExecuteNonQuery();
                connection.Close();
            }
                        
        }   
        
        private static List<ExceptionEntity> ReadExceptions()
        {
            string connectionString = @"Data Source=DESKTOP-D81QDRJ;Initial Catalog=newtwentyoneGame;
                                        Integrated Security=True;Connect Timeout=30;Encrypt=False;
                                        TrustServerCertificate=True;ApplicationIntent=ReadWrite;
                                        MultiSubnetFailover=False";
            string queryString = @"SELECT Id, ExceptionType, ExceptionMessage, TimeStamp FROM Exceptions";

            List<ExceptionEntity> Exceptions = new List<ExceptionEntity>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand commad = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = commad.ExecuteReader();
                while (reader.Read())
                {
                    ExceptionEntity exception = new ExceptionEntity();
                    exception.Id = Convert.ToInt32(reader["Id"]);
                    exception.ExceptionType = reader["ExceptionType"].ToString();
                    exception.ExceptionMessage = reader["ExceptionMessage"].ToString();
                    exception.TimeStamp = Convert.ToDateTime(reader["TimeStamp"]);
                    Exceptions.Add(exception);
                }
                connection.Close();
            }
            return Exceptions;
        }
    }
}
