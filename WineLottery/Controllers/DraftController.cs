using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using WineLottery.Models;

namespace WineLottery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DraftController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly DocumentClient dbClient;
        private string DbName => config["DbName"];

        public DraftController(IConfiguration config)
        {
            this.config = config;
            dbClient = new DocumentClient(new Uri(config["CosmosEndpoint"]), config["CosmosKey"]);
            dbClient.CreateDatabaseIfNotExistsAsync(new Database{ Id = DbName }).Wait();
        }

        [HttpGet("New")]
        public ActionResult<string> NewDraft()
        {
            WinebotContainer.StartAsync();

            var random = new Random();
            string draftId = $"{random.Next(9)}{random.Next(9)}{random.Next(9)}{random.Next(9)}";
            dbClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DbName), new DocumentCollection{ Id = draftId}).Wait();
            return Ok(draftId);
        }

        [HttpGet("Drafts")]
        public ActionResult<IEnumerable<string>> Drafts()
        {
            var drafts = dbClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(DbName)).Select(c => c.Id);
            return Ok(drafts);
        }
        
        [HttpGet("{draftId}/Participants")]
        public ActionResult<IEnumerable<string>> Participants(string draftId)
        {
            if (!CollectionExists(draftId))
                return NotFound();

            IEnumerable<string> participants = dbClient.CreateDocumentQuery<Participant>(UriFactory.CreateDocumentCollectionUri(DbName, draftId))
//                .GroupBy(p => p.UserId).Select(g => g.FirstOrDefault())
                .Where(d => !d.HasWon)
                .Select(d => d.Name);
            return Ok(participants);
        }

        [HttpGet("{draftId}/Winners")]
        public ActionResult<IEnumerable<string>> Winners(string draftId)
        {
            if (!CollectionExists(draftId))
                return NotFound();

            IEnumerable<string> participants = dbClient.CreateDocumentQuery<Participant>(UriFactory.CreateDocumentCollectionUri(DbName, draftId))
                .Where(d => d.HasWon)
                .Select(d => d.Name);
            return Ok(participants);
        }

        [HttpPost("Participate")]
        public ActionResult Participant([FromBody]Participant participant)
        {
            if (!CollectionExists(participant.DraftId))
                return BadRequest();

            IEnumerable<Participant> participants = dbClient.CreateDocumentQuery<Participant>(UriFactory.CreateDocumentCollectionUri(DbName, participant.DraftId))
                .Where(d => participant.UserId == d.UserId);
            if (participants.Any())
                return Ok("Already participated.");

            dbClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(DbName, participant.DraftId),
                new { participant.DraftId, participant.Name, participant.UserId, HasWon = false });
            return Ok();
        }

        [HttpGet("{draftId}/Winner")]
        public ActionResult<string> NextWinner(string draftId)
        {
            if (!CollectionExists(draftId))
                return NotFound();

            var participants = dbClient.CreateDocumentQuery<Participant>(UriFactory.CreateDocumentCollectionUri(DbName, draftId))
                .Where(p => !p.HasWon);
            int count = participants.Count();
            var random = new Random();
            Participant winner = participants.ToArray()[random.Next(count)];
            winner.HasWon = true;
            dbClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DbName, draftId, winner.Id), winner).Wait();
            return winner.Name;
        }

        [HttpDelete("{draftId}")]
        public ActionResult Delete(string draftId)
        {
            if (!CollectionExists(draftId))
                return NotFound();

            WinebotContainer.Stop();

            dbClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DbName, draftId)).Wait();
            return Ok();
        }

        private bool CollectionExists(string draftId)
        {
            IQueryable<DocumentCollection> draft = dbClient.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(DbName)).Where(c => c.Id == draftId);
            return draft.Any();
        }
    }
}
