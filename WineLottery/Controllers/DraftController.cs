using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WineLottery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DraftController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> NewDraft()
        {

            return "";
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Participants(string draftId)
        {
            return new[] {"eka", "toka"};
        }

        [HttpPost("{draftId}/{name}")]
        public ActionResult Participant(string draftId, string name)
        {

            return Ok();
        }

        [HttpGet("{draftId}")]
        public ActionResult<string> NextWinner(string draftId)
        {

            return "";
        }

        [HttpDelete("{draftId}")]
        public ActionResult Delete(string draftId)
        {

            return Ok();
        }
    }
}
