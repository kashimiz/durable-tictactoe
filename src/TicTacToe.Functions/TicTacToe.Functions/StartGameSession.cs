using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TicTacToe.Functions
{
    public static class StartGameSession
    {
        [FunctionName("StartGameSession")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {
            // get request params
            var postData = await req.ReadAsStringAsync();
            GameSessionRequest sessionParams = null;
            try
            {
                sessionParams = JsonConvert.DeserializeObject<GameSessionRequest>(postData);
            }
            catch (JsonException ex)
            {
                return new BadRequestResult();
            }

            // get playerId
            if (ValidateRequest(sessionParams))
            {
                log.LogInformation($"The player identifier provided is: {sessionParams.PlayerId}");
            }
            else
            {
                return new BadRequestResult();
            }

            // query db to join or create session
            SessionInfo sessionInfo = await DataClient.CreateOrJoinGameSession(sessionParams);

            var sessionOrchestrator = await starter.GetStatusAsync(sessionInfo.Guid.ToString());

            if (sessionOrchestrator == null)
            {
                // create session
                var initialGameState = new GameSession(sessionInfo.Id, sessionInfo.Guid, sessionParams.PlayerId);
                string instanceId = await starter.StartNewAsync("GameSession", sessionInfo.Guid.ToString(), initialGameState);
                
                log.LogInformation($"Started game session for player {sessionParams.PlayerId} with ID = '{instanceId}'.");
            }
            else
            {
                // send 'player join' event to session orchestrator
                await starter.RaiseEventAsync(sessionInfo.Guid.ToString(), Constants.PlayerJoinEventTag, sessionParams.PlayerId);
            }

            return new OkObjectResult(starter.CreateHttpManagementPayload(sessionInfo.Guid.ToString()));
        }

        public static bool ValidateRequest(GameSessionRequest req)
        {
            return req != null && req.PlayerId != 0;
        }
    }

    public class GameSessionRequest
    {
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        // extra game session parameters here, e.g. difficulty, board size, # players
    }

    public class SessionInfo
    {
        public long Id { get; set; }
        public Guid Guid { get; set; }
    }
}
