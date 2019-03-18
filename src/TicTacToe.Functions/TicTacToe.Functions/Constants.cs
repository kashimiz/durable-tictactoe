using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToe.Functions
{
    public static class Constants
    {
        public const string PlayerForfeitEventTag = "forfeit";
        public const string PlayerJoinEventTag = "addPlayer";
        public const string PlayerTurnEventTag = "takeTurn";

        public const string SignalRHub = "gameupdates";

        public const string ConnStrDatabase = "DbConnectionString";
        public const string ConnStrSignalR = "SRConnectionString";
    }
}
