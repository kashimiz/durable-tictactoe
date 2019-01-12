using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;

namespace TicTacToe.Functions
{
    public class DataClient
    {
        // It's best practice to store credentials in Key Vault, but this simple example uses Function App Settings.
        private static readonly string _connStr = Environment.GetEnvironmentVariable(Constants.ConnStrDatabase);
        
        internal static async Task<SessionInfo> CreateOrJoinGameSession(GameSessionRequest sessionParams)
        {
            // find or create a game session 
            using (MySqlConnection conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("createOrJoinGameSession", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@param_player_id", sessionParams.PlayerId));

                    using (var cursor = await cmd.ExecuteReaderAsync())
                    {
                        cursor.Read();

                        var id = Convert.ToInt64(cursor["id"]);
                        var guid = Guid.Parse(cursor["guid"].ToString());

                        return new SessionInfo
                        {
                            Id = id,
                            Guid = guid
                        };
                    }
                }
            }
        }

        public static async Task<IList<GameSession>> GetGameSessionList(long playerId)
        {
            var sessions = new List<GameSession>();

            using (MySqlConnection conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("getGameSessionList", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@param_player_id", playerId));

                    using (var cursor = await cmd.ExecuteReaderAsync())
                    {
                        while (cursor.Read())
                        {
                            var id = Convert.ToInt64(cursor["id"].ToString());
                            var guid = Guid.Parse(cursor["guid"]?.ToString());
                            var initPlayerId = Convert.ToInt64(cursor["createdplayer_id"]);

                            var session = new GameSession(id, guid, initPlayerId);

                            session.Status = (GameSessionStatus)Convert.ToInt32(cursor["gamestatus"]);
                            session.Board = JsonConvert.DeserializeObject<long[,]>(Convert.ToString(cursor["boardstate"]));
                            session.RemainingMoves = Convert.ToInt32(cursor["movesleft"]?.ToString());
                            if (long.TryParse(cursor["currentturnplayer_id"]?.ToString(), out var currentPlayer))
                            {
                                session.CurrentTurnPlayer = currentPlayer;
                            }
                            if (long.TryParse(cursor["winningplayer_id"]?.ToString(), out var winner))
                            {
                                session.Winner = winner;
                            }

                            sessions.Add(session);
                        }
                    }
                }
            }

            return sessions;
        }

        public static async Task<string> UpdateGameSessionState(GameSession state)
        {
            // update a game session
            using (MySqlConnection conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("updateGameSession", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@param_guid", state.Guid.ToString()));
                    cmd.Parameters.Add(new MySqlParameter("@param_status", state.Status));
                    cmd.Parameters.Add(new MySqlParameter("@param_boardstate", JsonConvert.SerializeObject(state.Board)));
                    cmd.Parameters.Add(new MySqlParameter("@param_movesleft", state.RemainingMoves));
                    cmd.Parameters.Add(new MySqlParameter("@param_currentturnplayer_id", state.CurrentTurnPlayer));
                    if (state.Winner.HasValue)
                    {
                        cmd.Parameters.Add(new MySqlParameter("@param_winningplayer_id", state.Winner.Value));
                    }
                    else
                    {
                        cmd.Parameters.Add(new MySqlParameter("@param_winningplayer_id", DBNull.Value));
                    }

                    var value = await cmd.ExecuteScalarAsync();

                    return value != null ? value.ToString() : null;
                }
            }
        }

        public static async Task<GameSession> LoadGameSessionState(Guid gameId)
        {
            GameSession session = null;
            using (MySqlConnection conn = new MySqlConnection(_connStr))
            {
                await conn.OpenAsync();

                using (MySqlCommand cmd = new MySqlCommand("getGameSession", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@param_guid", gameId.ToString()));

                    using (var cursor = await cmd.ExecuteReaderAsync())
                    {
                        long initPlayerId = 0;
                        while (cursor.Read())
                        {
                            var id = Convert.ToInt64(cursor["id"].ToString());
                            var guid = Guid.Parse(cursor["guid"]?.ToString());
                            initPlayerId = Convert.ToInt64(cursor["createdplayer_id"]);

                            session = new GameSession(id, guid, initPlayerId);

                            session.Status = (GameSessionStatus)Convert.ToInt32(cursor["gamestatus"]);
                            session.Board = JsonConvert.DeserializeObject<long[,]>(Convert.ToString(cursor["boardstate"]));
                            session.RemainingMoves = Convert.ToInt32(cursor["movesleft"]?.ToString());
                            if (long.TryParse(cursor["currentturnplayer_id"]?.ToString(), out var currentPlayer))
                            {
                                session.CurrentTurnPlayer = currentPlayer;
                            }
                            if (long.TryParse(cursor["winningplayer_id"]?.ToString(), out var winner))
                            {
                                session.Winner = winner;
                            }
                        }

                        cursor.NextResult();

                        while (cursor.Read() && session != null)
                        {
                            var playerId = Convert.ToInt64(cursor["player_id"]);

                            if (session != null && playerId != 0)
                            {
                                session.Players.Add(playerId);
                            }
                        }
                    }
                }
            }

            return session;
        }
    }
}
