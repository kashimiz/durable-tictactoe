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
    public static class LoadGameSession
    {
        [FunctionName("LoadGameSession")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string idString = req.Query["id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            idString = idString ?? data?.id;

            if (Guid.TryParse(idString, out var id))
            {
                var gameState = await DataClient.LoadGameSessionState(id);
                return new OkObjectResult(gameState);
            }
            else
            {
                return new BadRequestObjectResult("Please pass a valid 'id' on the query string or in request body");
            }
        }
    }
}
