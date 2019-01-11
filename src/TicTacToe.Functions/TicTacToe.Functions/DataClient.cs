using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Numerics;

namespace TicTacToe.Functions
{
    public class DataClient
    {
        public static async Task<string> GetJoinableGameSessionGuid(string playerId)
        {
            // returnedValue
            string sessionGuid = null;

            // It's best practice to store credentials in Key Vault, but this simple example uses Function App Settings.
            var connStr = Environment.GetEnvironmentVariable("DbConnectionString");
            
            // locate an existing open game
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("gamesession_SELECT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@param_playerid", BigInteger.Parse(playerId)));
                    cmd.Parameters.Add(new MySqlParameter("@param_limit", 1));

                    var value = await cmd.ExecuteScalarAsync();

                    if (value.GetType() == typeof(string) && !String.IsNullOrWhiteSpace((string)value))
                    {
                        sessionGuid = (string)value;
                    }
                }
            }

            // retrieve game ID or null if nothing
            return sessionGuid;
        }

        public static async Task<GameSessionState> LoadGameSessionState(int gameId)
        {
            return null;
        }
    }
}
