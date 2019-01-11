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
            DurableOrchestrationClient starter,
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
            int? playerId = null;
            if (sessionParams != null)
            {
                playerId = sessionParams.PlayerId;
                log.LogInformation($"The player identifier provided is: {playerId}");
            }

            // query db for function matching params
            string matchResult = null;
            if (playerId.HasValue)
            {
                matchResult = await DataClient.GetJoinableGameSessionGuid(playerId.Value.ToString());
            }

            /*
            if (matchResult == null)
            {
                // create session
                var initialGameState = new GameSessionState(playerId.Value);
                string instanceId = await starter.StartNewAsync("GameSession", initialGameState);

                log.LogInformation($"Started game session for player {playerId} with ID = '{instanceId}'.");

                // return session info
                // get from customStatus?
                // query from DB?
                matchResult = "someguid"; // TODO: Swap for data store call.. or do loading separately?
            }
            else
            {
                // send 'player join' event to orchestration
                await starter.RaiseEventAsync(matchResult, Constants.PlayerJoinEventTag, playerId);
            }
            */
            
            return new OkObjectResult(matchResult);
        }
    }

    public class GameSessionRequest
    {
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        // extra game session parameters here, e.g. difficulty, board size, # players
    }
}
