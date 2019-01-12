using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TicTacToe.Functions
{
    public static class GameSessionOrchestrator
    {
        [FunctionName("GameSession")]
        public static async Task<GameSession> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var notifications = new List<Notification>();
            var gameState = context.GetInput<GameSession>();

            if (gameState.Players.Count < 2)
            {
                var playerTwoId = await context.WaitForExternalEvent<int>(Constants.PlayerJoinEventTag);

                gameState.Players.Add(playerTwoId);
                gameState.StartGame();

                await context.CallActivityAsync("SetupNotifications", gameState);

                notifications.Add(new Notification {
                    Type = NotificationType.PlayerMessage,
                    ToId = playerTwoId.ToString(),
                    Message = "The game has started. You go first!"
                });
            }
            else
            {
                // listen for events
                var timeoutCts = new CancellationTokenSource();
                var turnTimeoutAt = context.CurrentUtcDateTime.AddSeconds(gameState.TurnTimeoutSec);

                var timeoutTask = context.CreateTimer(turnTimeoutAt, timeoutCts.Token);
                var turnEventTask = context.WaitForExternalEvent<PlayerTurn>(Constants.PlayerTurnEventTag);
                var forfeitEventTask = context.WaitForExternalEvent<int>(Constants.PlayerForfeitEventTag);

                var nextEvent = await Task.WhenAny(timeoutTask, turnEventTask, forfeitEventTask);
                if (!timeoutTask.IsCompleted)
                {
                    // all pending timers must be complete or canceled before the function exits
                    timeoutCts.Cancel();
                }

                // handle events
                if (nextEvent == forfeitEventTask || nextEvent == timeoutTask)
                {
                    // determine who forfeits
                    var forfeitedPlayerId =
                        nextEvent == forfeitEventTask
                        ? forfeitEventTask.Result
                        : gameState.CurrentTurnPlayer;

                    // update game state
                    gameState.Forfeit(forfeitedPlayerId);
                    notifications.Add(new Notification {
                        Type = NotificationType.GroupMessage,
                        ToId = context.InstanceId,
                        Message = $"Player {forfeitedPlayerId} forfeited."
                    });
                }
                else if (nextEvent == turnEventTask)
                {
                    // update game state
                    var turnData = turnEventTask.Result;

                    if (turnData.PlayerId == gameState.CurrentTurnPlayer)
                    {
                        gameState.PlayTurn(turnData.Coordinates.X, turnData.Coordinates.Y);
                    }
                }

                // determine if game ends
                if (gameState.RemainingMoves == 0)  // draw
                {
                    notifications.Add(new Notification
                    {
                        Type = NotificationType.GroupMessage,
                        ToId = context.InstanceId,
                        Message = "The match was a draw."
                    });
                }
                else if (gameState.Winner.HasValue) // win-loss
                {
                    notifications.Add(new Notification
                    {
                        Type = NotificationType.GroupMessage,
                        ToId = gameState.Winner.ToString(),
                        Message = $"Player {gameState.Winner} has won."
                    });
                }
            }
            
            // persist game state
            await context.CallActivityAsync("PersistGameSession", gameState);

            // send notifications
            notifications.Add(new Notification
            {
                Type = NotificationType.StateUpdate,
                ToId = context.InstanceId,
                Message = JsonConvert.SerializeObject(gameState)
            });

            if (gameState.Status != GameSessionStatus.Completed)
            {
                context.ContinueAsNew(gameState);
            }

            await context.CallActivityAsync("CleanupNotifications", gameState);

            return gameState;
        }

        [FunctionName("PersistGameSession")]
        public static async Task UpdateGameSession([ActivityTrigger] GameSession newState,
            ILogger log)
        {
            await DataClient.UpdateGameSessionState(newState);
        }

        [FunctionName("SetupNotifications")]
        public static async Task SetupNotifications([ActivityTrigger] GameSession state,
            [SignalR(ConnectionStringSetting = Constants.ConnStrSignalR, HubName = Constants.SignalRHub)] IAsyncCollector<SignalRGroupAction> groupActions,
            ILogger log)
        {
            // add players to SignalR group for this game session
            foreach(var player in state.Players)
            {
                await groupActions.AddAsync(new SignalRGroupAction
                {
                    UserId = player.ToString(),
                    GroupName = state.Guid.ToString(),
                    Action = GroupAction.Add
                });
            }
        }

        [FunctionName("CleanupNotifications")]
        public static async Task CleanupNotifications([ActivityTrigger] GameSession state,
    [SignalR(ConnectionStringSetting = Constants.ConnStrSignalR, HubName = Constants.SignalRHub)] IAsyncCollector<SignalRGroupAction> groupActions,
    ILogger log)
        {
            // remove players from SignalR group
            foreach (var player in state.Players)
            {
                await groupActions.AddAsync(new SignalRGroupAction
                {
                    UserId = player.ToString(),
                    GroupName = state.Guid.ToString(),
                    Action = GroupAction.Remove
                });
            }
        }

        [FunctionName("SendNotification")]
        public static async Task SendNotification([ActivityTrigger] IList<Notification> notifications,
            [SignalR(ConnectionStringSetting = Constants.ConnStrSignalR, HubName = "gameUpdates")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            foreach (var note in notifications)
            {
                var message = new SignalRMessage
                {
                    Target = signalRMethodMap[note.Type],
                    Arguments = new[] { note.Message }
                };

                if (note.Type == NotificationType.PlayerMessage)
                {
                    message.UserId = note.ToId;
                }
                else
                {
                    message.GroupName = note.ToId;
                }

                await signalRMessages.AddAsync(message);
            }
        }

        public static Dictionary<NotificationType, string> signalRMethodMap = new Dictionary<NotificationType, string>
        {
            { NotificationType.StateUpdate, "stateUpdate" },
            { NotificationType.GroupMessage, "newMessage" },
            { NotificationType.PlayerMessage, "newMessage" }
        };
    }

    public class PlayerTurn
    {
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; }
    }

    public struct Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Notification
    {
        public NotificationType Type { get; set; }
        public string ToId { get; set; }
        public string Message { get; set; }
    }

    public enum NotificationType
    {
        StateUpdate = 0,
        GroupMessage = 1,
        PlayerMessage = 2
    }
}