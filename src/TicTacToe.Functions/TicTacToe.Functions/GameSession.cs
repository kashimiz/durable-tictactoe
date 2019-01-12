using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Functions
{
    public class GameSession
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("guid")]
        public Guid Guid { get; }

        [JsonProperty("players")]
        public List<long> Players { get; set; }

        [JsonProperty("status")]
        public GameSessionStatus Status { get; set; }

        [JsonProperty("board")]
        public long[,] Board { get; set; }

        [JsonProperty("remainingMoves")]
        public int RemainingMoves { get; set; }

        [JsonProperty("turnTimeoutSec")]
        public int TurnTimeoutSec { get; set; }

        [JsonProperty("currentTurnPlayer")]
        public long CurrentTurnPlayer { get; set; }

        [JsonProperty("winner")]
        public long? Winner { get; set; }

        public GameSession(long id, Guid guid, long initialPlayerId, int turnTimeoutSec = 600)
        {
            Id = id;
            Guid = guid;
            Players = new List<long>();
            Status = GameSessionStatus.SeekingMatch;
            Board = new long[3, 3];
            RemainingMoves = 9;
            TurnTimeoutSec = turnTimeoutSec;
        }

        public void StartGame(int? firstPlayer = null)
        {
            Status = GameSessionStatus.InProgress;

            Random rand = new Random();
            CurrentTurnPlayer = Players[rand.Next(0, Players.Count)];
        }

        public void PlayTurn(int xCoord, int yCoord)
        {
            Board[xCoord, yCoord] = CurrentTurnPlayer;

            RemainingMoves--;

            var currentTurnPlayerIndex = Players.IndexOf(CurrentTurnPlayer);
            CurrentTurnPlayer = Players[(currentTurnPlayerIndex + 1) % Players.Count];

            Winner = FindWinner();

            if (Winner.HasValue || RemainingMoves == 0)
            {
                Status = GameSessionStatus.Completed;
            }
        }

        public void Forfeit(long forfeitingPlayer)
        {
            Players.Remove(forfeitingPlayer);
            if (Players.Count == 1)
            {
                Status = GameSessionStatus.Completed;
                Winner = Players[0];
            }
        }

        private long? FindWinner()
        {
            long? playerId = FindRowCrossed();

            if (playerId != 0)
            {
                playerId = FindColumnCrossed();
            }

            if (playerId != 0)
            {
                playerId = FindDiagonalCrossed();
            }

            return playerId;
        }


        private long? FindRowCrossed()
        {
            for (var i = 0; i < Board.GetLength(0); i++)
            {
                if (Board[i, 0] > 0 &&
                    Board[i, 0] == Board[i, 1] &&
                    Board[i, 1] == Board[i, 2])
                {
                    return Board[i, 0];
                }
            }
            return null;
        }

        private long? FindColumnCrossed()
        {
            for (var i = 0; i < Board.GetLength(0); i++)
            {
                if (Board[0, i] > 0 &&
                    Board[0, i] == Board[1, i] &&
                    Board[1, i] == Board[2, i])
                {
                    return Board[0, i];
                }
            }
            return null;
        }

        public long? FindDiagonalCrossed()
        {
            if (Board[0, 0] > 0 &&
                Board[0, 0] == Board[1, 1] &&
                Board[1, 1] == Board[2, 2])
            {
                return Board[0, 0];
            }

            if (Board[0, 2] > 0 &&
                Board[0, 2] == Board[1, 1] &&
                Board[1, 1] == Board[2, 0])
            {
                return Board[0, 2];
            }

            return null;
        }
    }
}
