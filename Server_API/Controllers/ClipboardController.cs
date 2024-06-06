using Microsoft.AspNetCore.Mvc;
using Server_API.Model.clipboard;

namespace Server_API.Controllers
{
    public class ClipboardController : Controller
    {
        //Injetando ICrypto (Because injeção é legal)
        private readonly ICrypto _crypto;

        private readonly ILogger<ClipboardController> _logger;

        private static Dictionary<string, string> clipboardData = new Dictionary<string, string>();

        public ClipboardController(ICrypto crypto,
                                   ILogger<ClipboardController> logger)
        {
            _crypto = crypto;
            _logger = logger;
        }

        [HttpPost]
        [Route("api/clipboard/post")]
        public Task<IActionResult> PostClipBoard([FromBody] Transmition transmition)
        {
            transmition.Content = transmition.Content ?? "";
            clipboardData["0"] = _crypto.Encrypt(transmition.Content);
            //------------------------------------------------------------
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet]
        [Route("api/clipboard/get")]
        public Task<IActionResult> GetClipBoard()
        {
            ClipboardResponse clipboardResponse = new ClipboardResponse();

            if (clipboardData.ContainsKey("0"))
            {
                clipboardResponse.Clipboard = _crypto.Decrypt(clipboardData["0"]);

                return Task.FromResult<IActionResult>(Ok(clipboardResponse));
            }
            else
            {
                return Task.FromResult<IActionResult>(Ok(clipboardResponse));
            }
        }
    }
}