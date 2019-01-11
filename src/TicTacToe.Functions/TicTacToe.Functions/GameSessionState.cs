using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Functions
{
    public class GameSessionState
    {
        [JsonProperty("players")]
        public List<int> Players { get; set; }

        [JsonProperty("board")]
        public int[,] Board { get; set; }

        [JsonProperty("winner")]
        public int? Winner { get; set; }

        [JsonProperty("remainingMoves")]
        public int RemainingMoves { get; set; }

        public GameSessionState(int initialPlayerId)
        {
            Players.Add(initialPlayerId);
            Board = new int[3, 3];
            RemainingMoves = 9;
        }

        public void PlayTurn(int playerId, int xCoord, int yCoord)
        {
            RemainingMoves--;

            Board[xCoord, yCoord] = playerId;

            Winner = FindWinner();
        }

        private int? FindWinner()
        {
            int? playerId = FindRowCrossed();

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


        private int? FindRowCrossed()
        {
            for (int i = 0; i < Board.GetLength(0); i++)
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

        private int? FindColumnCrossed()
        {
            for (int i = 0; i < Board.GetLength(0); i++)
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

        public int? FindDiagonalCrossed()
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
