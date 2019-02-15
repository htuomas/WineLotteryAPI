using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace WineLottery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DraftController : ControllerBase
    {
        private readonly IConfiguration config;
        private DocumentClient dbClient;
        protected internal string DbName => config["DbName"];

        public DraftController(IConfiguration config)
        {
            this.config = config;
            dbClient = new DocumentClient(new Uri(config["CosmosEndpoint"]), config["CosmosKey"]);
            dbClient.CreateDatabaseIfNotExistsAsync(new Database{ Id = DbName }).Wait();
        }

        [HttpGet("New")]
        public ActionResult<string> NewDraft()
        {
            var random = new Random();
            string draftId = "rnd"+ random.Next(9999);
            dbClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DbName), new DocumentCollection{ Id = draftId}).Wait();
            return Ok(draftId);
        }

        [HttpGet("{draftId}/Participants")]
        public ActionResult<IEnumerable<string>> Participants(string draftId)
        {
            IEnumerable<string> participants = dbClient.CreateDocumentQuery<Participant>(UriFactory.CreateDocumentCollectionUri(DbName, draftId))
                .Select(d => d.Name);
            return Ok(participants);
        }

        [HttpPost("{draftId}/{name}")]
        public ActionResult Participant(string draftId, string name)
        {
            dbClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(DbName, draftId),
                new {Name = name, DraftId = draftId});
            return Ok();
        }

        [HttpGet("{draftId}/Winner")]
        public ActionResult<string> NextWinner(string draftId)
        {
            var participants = dbClient.CreateDocumentQuery<Participant>(UriFactory.CreateDocumentCollectionUri(DbName, draftId));
            int count = participants.Count();
            var random = new Random();
            var winner = participants.ToArray()[random.Next(count)];
            dbClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DbName, draftId, winner.Id)).Wait();
            return winner.Name;
        }

        [HttpDelete("{draftId}")]
        public ActionResult Delete(string draftId)
        {
            dbClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DbName, draftId)).Wait();
            return Ok();
        }
    }

    public class Participant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DraftId { get; set; }
    }
}
