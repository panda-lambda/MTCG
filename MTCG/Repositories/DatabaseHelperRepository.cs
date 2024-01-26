using MTCG.Data;


namespace MTCG.Repositories
{
    public class DatabaseHelperRepository : IDatabaseHelperRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public DatabaseHelperRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void DropTables(List<string> tableNames)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    foreach (string tableName in tableNames)
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"DROP TABLE IF EXISTS {tableName} CASCADE";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Exception dropping tables !!");
                }
            }
        }

        public void CreateTables()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                try
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS USERS (id UUID PRIMARY KEY, name VARCHAR(255) UNIQUE, password VARCHAR(255))";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS USERDATA (Id UUID PRIMARY KEY, Name VARCHAR(55), Bio VARCHAR(255), Image VARCHAR(50), Coins INTEGER)";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS USERSTATS (Id UUID PRIMARY KEY, Name VARCHAR(55), Elo INTEGER, WINS INTEGER, LOSSES INTEGER, GAMES INTEGER)";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS CARDS " +
                 "(Id UUID PRIMARY KEY," +
                 " Name VARCHAR(100)," +
                 " Damage DECIMAL(7, 2)," +
                 " Type VARCHAR(50)," +
                 " Locked BOOLEAN ," +
                 " Element VARCHAR(50)," +
                 " Monster VARCHAR(50), " +
                 "OwnerId UUID REFERENCES Users(Id))";
                        cmd.ExecuteNonQuery();


                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS DECKS " +
                      "(Id UUID PRIMARY KEY," +
                      "OwnerId UUID UNIQUE REFERENCES Users(Id)," +
                      "Description VARCHAR(250)," +
                      "CardId1 UUID," +
                      "CardId2 UUID," +
                      "CardId3 UUID," +
                      "CardId4 UUID," +
                      "FOREIGN KEY (CardId1) REFERENCES CARDS(Id)," +
                      "FOREIGN KEY (CardId2) REFERENCES CARDS(Id)," +
                      "FOREIGN KEY (CardId3) REFERENCES CARDS(Id)," +
                      "FOREIGN KEY (CardId4) REFERENCES CARDS(Id))";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Package (Id UUID PRIMARY KEY," +
                        "CardId1 UUID," +
                        "CardId2 UUID," +
                        "CardId3 UUID," +
                        "CardId4 UUID," +
                        "CardId5 UUID," +
                        "FOREIGN KEY (CardId1) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId2) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId3) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId4) REFERENCES CARDS(Id)," +
                        "FOREIGN KEY (CardId5) REFERENCES CARDS(Id))";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS TRADES " +
                   "(Id UUID PRIMARY KEY," +
                   "OwnerId UUID REFERENCES Users(Id)," +
                   "CARDTOTRADE UUID UNIQUE REFERENCES CARDS(Id)," +
                   "TYPE VARCHAR(250)," +
                   "MINDAMAGE DECIMAL(7, 2))";

                        cmd.ExecuteNonQuery();


                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Exception creating tables !!");
                }
            }





        }
    }
}