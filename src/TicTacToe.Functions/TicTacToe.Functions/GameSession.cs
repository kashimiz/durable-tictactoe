using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Functions
{
    public class GameSession
    {
        public long ID { get; set; }
        public Guid GUID { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public GameSessionStatus GameStatus { get; set; }
        public GameSessionState BoardState { get; set; }
        public long CurrentTurnPlayerID { get; set; }
    }
}
