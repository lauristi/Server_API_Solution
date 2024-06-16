using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Server_API.Domain.Model.BB.BLL;
using Server_API.Infrastructure;
using Server_API.Service.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Server_API.Controllers
{
    public class BBController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IBBService _BBService;
        private readonly ILogger<BBController> _logger;

        public BBController(IMapper mapper,
                            IBBService BBService,
                            ILogger<BBController> logger)
        {
            _mapper = mapper;
            _BBService = BBService;
            _logger = logger;
        }

        #region "Upload"

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

        #endregion "Upload"

        #region "Process File"

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
        [Route("api/bb/multiPartProcessFile")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Download a file.", typeof(FileContentResult))]
        public IActionResult MultiPartProcessFile()
        {
            var statementFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "original", "original.csv");
            var expenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expenses", "expenses.csv");
            var finalFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "final");

            if (System.IO.File.Exists(statementFilePath) && System.IO.File.Exists(expenseFilePath))
            {
                // Normaliza IO
                if (!Directory.Exists(finalFilePath))
                {
                    Directory.CreateDirectory(finalFilePath);
                }
                else
                {
                    // Apaga arquivos antigos
                    System.IO.DirectoryInfo finalDirectory = new System.IO.DirectoryInfo(finalFilePath);
                    foreach (System.IO.FileInfo file in finalDirectory.GetFiles()) file.Delete();
                }

                // Processa dados da Origem e disponibiliza arquivo para download
                finalFilePath = _BBService.ProcessStatment(statementFilePath, expenseFilePath, finalFilePath);

                if (string.IsNullOrEmpty(finalFilePath))
                {
                    return BadRequest("Erro no processamento do arquivo");
                }
                else
                {
                    //--------------------------------------------------------------------------------------------
                    // Dados Json
                    //--------------------------------------------------------------------------------------------
                    var jsonData = new
                    {
                        Mensagem = "Arquivo processado com sucesso",
                        DataHora = DateTime.UtcNow
                    };

                    var jsonString = JsonSerializer.Serialize(jsonData);
                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    //--------------------------------------------------------------------------------------------
                    // Arquivo
                    //--------------------------------------------------------------------------------------------

                    var fileStream = new FileStream(finalFilePath, FileMode.Open, FileAccess.Read);
                    var fileContent = new StreamContent(fileStream);

                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = Path.GetFileName(finalFilePath)
                    };

                    //--------------------------------------------------------------------------------------------
                    //Criação do conteudo multipart (Tipo: Mixed | String aleatória de separação: boundary123"
                    //--------------------------------------------------------------------------------------------
                    var multipartContent = new MultipartContent("mixed", "boundary123");
                    multipartContent.Add(fileContent);
                    multipartContent.Add(jsonContent);

                    var response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = multipartContent
                    };

                    return new HttpResponseMessageResult(response);
                }
            }
            else
            {
                return BadRequest("Arquivos necessários não encontrados");
            }
        }

        [HttpGet]
        [Route("api/bb/multiPartProcessFile2")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Download a file.", typeof(MultiPartResponse))]
        public IActionResult MultiPartProcessFile2()
        {
            var statementFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "original", "original.csv");
            var expenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expenses", "expenses.csv");
            var finalFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "final");

            if (System.IO.File.Exists(statementFilePath) && System.IO.File.Exists(expenseFilePath))
            {
                // Normaliza IO
                if (!Directory.Exists(finalFilePath))
                {
                    Directory.CreateDirectory(finalFilePath);
                }
                else
                {
                    // Apaga arquivos antigos
                    System.IO.DirectoryInfo finalDirectory = new System.IO.DirectoryInfo(finalFilePath);
                    foreach (System.IO.FileInfo file in finalDirectory.GetFiles()) file.Delete();
                }

                // Processa dados da Origem e disponibiliza arquivo para download

                var processedData = _BBService.ProcessBBStatment(statementFilePath, expenseFilePath, finalFilePath);

                // Mapeia para o tipo esperado no projeto
                RecoveredData recoveredData = new RecoveredData();
                recoveredData = _mapper.Map<Server_API.Infrastructure.RecoveredData>(processedData);

                if (string.IsNullOrEmpty(finalFilePath))
                {
                    return BadRequest("Erro no processamento do arquivo");
                }
                else
                {
                    MultiPartResponse multiPartResponse = new MultiPartResponse();
                    multiPartResponse.JsonContent = JsonSerializer.Serialize(recoveredData);

                    // Arquivo
                    multiPartResponse.FileContent = System.IO.File.ReadAllBytes(recoveredData.FilePath);

                    // Retorno
                    return Ok(JsonSerializer.Serialize(multiPartResponse));
                }
            }
            else
            {
                return BadRequest("Arquivos necessários não encontrados");
            }
        }

        #endregion "Process File"

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