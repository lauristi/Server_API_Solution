using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Server_API.Domain.Infrastructure.EncryptionLib;
using Server_API.Domain.Infrastructure.Interface;
using Server_API.Domain.Service.BBService;
using Server_API.Domain.Service.BBService.Interface;
using Server_API.Domain.Service.InfrastrutureService;
using Server_API.Domain.Service.InfrastrutureService.Interface;
using Server_API.Infrastructure.Mapper;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

//Set log
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var configuration = builder.Configuration;

//APPSETTINGS
var apiBaseAddress = configuration["ConnectionSettings:ApiBaseAddress"];
var bindAddress = configuration["ConnectionSettings:BindAddress"];
var bindPort = int.Parse(configuration["ConnectionSettings:BindPort"] ?? "5020");

//==============================================================================================
// Configure o Kestrel para ouvir em todas as interfaces de rede na porta 5020
//==============================================================================================

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(System.Net.IPAddress.Parse(bindAddress), bindPort);
    });
}
//==============================================================================================

// Add services to the container.
builder.Services.AddScoped<IBBService, BBService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //Sem isso todas as propriedades do Json ficam minusculas no retorno e causam problemas no blazor
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Serviços do Dominio
builder.Services.AddSingleton<ICrypto, Crypto>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddScoped<INormalizeService, NormalizeService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

builder.Services.AddScoped<IBBService, BBService>();
builder.Services.AddScoped<IXlsService, XlsService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

//Determinando o uso de Pt-br para a App
var supportedCultures = new[] { new CultureInfo("pt-BR") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

//Garatir que seja usado em todas as threads
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//REMOVIDO PRA EVITAR ERRO DO CORS
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//==============================================================================================
//Configuracao de Cabecalho encaminhado para funcionar com proxy reverso... Ngnix
//==============================================================================================
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

//==============================================================================================

app.UseAuthentication();
app.Run();