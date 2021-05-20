using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DeadLock
{
    class DB
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;

        public static void FirstTransaction()
        {
            string query = "" +
                "BEGIN TRAN T1 " +
                "UPDATE Film SET Titel = 'Deadlock2' WHERE id = 10" +
                "WAITFOR DELAY '00:00:05'; " +
                "UPDATE UserData SET password = 'parola2' WHERE username = 'gg'" +
                "COMMIT TRAN T1";

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand command = new SqlCommand(query, sqlConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                TryAgain(ex, command, 1);
            }
            finally
            {
                sqlConnection.Close();
                Console.WriteLine("T1 done");
            }

        }

        public static void SecondTransaction()
        {
            string query = "" +
                "BEGIN TRAN T2 " +
                "UPDATE UserData SET password = 'parola1' WHERE username = 'gg'" +
                "WAITFOR DELAY '00:00:05'; " +
                "UPDATE Film SET Titel = 'Deadlock1' WHERE id = 10" +
                "COMMIT TRAN T2 ";

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand command = new SqlCommand(query, sqlConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                TryAgain(ex, command, 2);
            }
            finally
            {
                sqlConnection.Close();
                Console.WriteLine("T2 done");
            }
        }

        private static void TryAgain(SqlException exception, SqlCommand command, int tNr)
        {
            foreach (SqlError error in exception.Errors)
            {
                if (error.Number == 1205) //Deadlock error
                {
                    Console.WriteLine("Deadlock occured, victim was T" + tNr.ToString() + ", trying again:");

                    bool caughtException = false;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlException)
                    {
                        caughtException = true;
                        foreach (SqlError sqlError in sqlException.Errors)
                        {
                            if (sqlError.Number == 1205)
                            {
                                Console.WriteLine("Deadlock occured again");
                            }
                            else
                            {
                                Console.WriteLine(sqlError.ToString());
                            }
                        }
                    }
                    finally
                    {
                        if (caughtException == true)
                        {
                            Console.Write("TryAgain failed\n");
                        }
                        else
                        {
                            Console.Write("TryAgain successfull\n");
                        }

                    }

                }
                else
                {
                    Console.WriteLine(error.ToString());
                }
            }
        }
    }
}
