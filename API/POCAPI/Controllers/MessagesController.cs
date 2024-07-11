using Microsoft.AspNetCore.Mvc;

namespace POCAPI.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private static List<string> messages = new List<string>();

        // POST api/messages
        [HttpPost]
        public IActionResult Post([FromBody] string message)
        {
            messages.Add(message);
            return Ok();
        }

        // GET api/messages
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(messages);
        }
    }
}
