using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TicTacToe.Functions
{
    public static class GameSessionOrchestrator
    {
        [FunctionName("GameSession")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var gameState = context.GetInput<GameSessionState>();
            
            if (gameState.Players.Count < 2)
            {
                var playerTwoId = await context.WaitForExternalEvent<int>(Constants.PlayerJoinEventTag);

                gameState.Players.Add(playerTwoId);
                // update game state
                // send notifications
            }
            else
            {
                var turnEvent = context.WaitForExternalEvent<PlayerTurn>(Constants.PlayerTurnEventTag);
                var forfeitEvent = context.WaitForExternalEvent<string>(Constants.PlayerForfeitEventTag);

                var nextEvent = await Task.WhenAny(turnEvent, forfeitEvent);
                if (nextEvent == forfeitEvent)
                {
                    // update game state
                    // notify players
                }
                else if (nextEvent == turnEvent)
                {
                    // update game state
                    var turnData = turnEvent.Result;
                    var x = turnData.Coords.Item1;
                    var y = turnData.Coords.Item2;

                    gameState.PlayTurn(turnData.PlayerId, x, y);

                    // determine if game ends
                    if (gameState.RemainingMoves == 0)
                    {
                        // game is a draw
                        // update database

                        // send notifications
                        return; // return draw
                    }
                    else if (gameState.Winner.HasValue)
                    {
                        // game is won
                        // update game state
                        // send notifications
                        return; // return winning player ID
                    }

                    // update board
                    // send notifications
                }
            }

            context.ContinueAsNew(gameState);
        }

        [FunctionName("UpdateDatabase")]
        public static async Task UpdateBoard([ActivityTrigger] GameSessionState newState, ILogger log)
        {
            log.LogInformation("Updated database!");
            // update in database
            // notify subscribers to update
        }

        [FunctionName("SendPlayerNotification")]
        public static async Task SendPlayerNotification([ActivityTrigger] string playerId, ILogger log)
        {
            log.LogInformation("Notified player!");
        }
    }

    public class PlayerTurn
    {
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        [JsonProperty("coords")]
        public ValueTuple<int,int> Coords { get; set; }
    }
}