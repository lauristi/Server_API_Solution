 using Microsoft.AspNetCore.Mvc;
using Server_API.Model.BB;
using Server_API.Service.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Server_API.Controllers
{
    public class BBController : Controller
    {
        private readonly ILogger<BBController> _logger;
        private readonly IBBService _BBService;

        public BBController(IBBService BBService,
                               ILogger<BBController> logger)
        {
            _BBService = BBService;
            _logger = logger;
        }

        [HttpPost]
        [Route("api/bb/uploadStatement")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Download a file.", typeof(FileContentResult))]
        public async Task<IActionResult> UploadStatement(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                //01 Normaliza IO
                var statementFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "original");

                if (!Directory.Exists(statementFilePath)) Directory.CreateDirectory(statementFilePath);
                statementFilePath = Path.Combine(statementFilePath, "original.csv");

                //02 Apaga arquivo antigo se existir
                System.IO.File.Delete(statementFilePath);

                //03 Cria arquivo no servidor
                using (var stream = new FileStream(statementFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok("original.csv criado no servidor");
            }
            else
            {
                return BadRequest("Erro na criação do arquivo no servidor");
            }
        }

        [HttpPost]
        [Route("api/bb/uploadExpenses")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Download a file.", typeof(FileContentResult))]
        public async Task<IActionResult> UploadExpenses(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                //Normaliza IO
                var expenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expenses");
                if (!Directory.Exists(expenseFilePath)) Directory.CreateDirectory(expenseFilePath);
                expenseFilePath = Path.Combine(expenseFilePath, "expenses.csv");

                //02 Apaga arquivo antigo se existir
                System.IO.File.Delete(expenseFilePath);

                //03 Cria arquivo no servidor
                using (var stream = new FileStream(expenseFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok("expenses.csv criado no servidor");
            }
            else
            {
                return BadRequest("Erro na criação do arquivo no servidor");
            }
        }

        [HttpGet]
        [Route("api/bb/processFile")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Download a file.", typeof(FileContentResult))]
        public IActionResult ProcessFile()
        {
            var statementFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "original", "original.csv");
            var expenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expenses", "expenses.csv");
            var finalFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "final");

            if (System.IO.File.Exists(statementFilePath) && System.IO.File.Exists(expenseFilePath))
            {
                //01 normaliza IO
                if (!Directory.Exists(finalFilePath))
                {
                    Directory.CreateDirectory(finalFilePath);
                }
                else
                {
                    //apaga arquivos antigos
                    System.IO.DirectoryInfo finalDirectory = new System.IO.DirectoryInfo(finalFilePath);
                    foreach (System.IO.FileInfo file in finalDirectory.GetFiles()) file.Delete();
                }

                //02 Processa dados da Origem e disponibiliza arquivo para download
                finalFilePath = _BBService.ProcessStatment(statementFilePath, expenseFilePath, finalFilePath);

                if (string.IsNullOrEmpty(finalFilePath))
                {
                    return BadRequest("Erro no processamento do arquivo");
                }
                else
                {
                    return PhysicalFile(finalFilePath, "application/octet-stream", Path.GetFileName(finalFilePath));
                }
            }
            else
            {
                return BadRequest("Arquivos necessários não encontrados");
            }
        }

        [HttpGet]
        [Route("api/bb/spending")]
        public IActionResult GetSpending()
        {
            var statementFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "original", "original.csv");

            if (!System.IO.File.Exists(statementFilePath))
            {
                return BadRequest("Arquivos necessários não encontrados");
            }

            Spending spending = new Spending();
            spending.Value = _BBService.ProcessMonthSpending(statementFilePath);

            return Ok(spending);
        }
    }
}