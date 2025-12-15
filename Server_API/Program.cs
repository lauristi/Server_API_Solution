using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Server_API.Domain.Infrastructure.EncryptionLib;
using Server_API.Domain.Infrastructure.Interface;
using Server_API.Domain.Service.BankService;
using Server_API.Domain.Service.BankService.Interface;
using Server_API.Domain.Service.ExpenseService;
using Server_API.Domain.Service.ExpenseService.Inrterface;
using Server_API.Domain.Service.InfrastrutureService;
using Server_API.Domain.Service.InfrastrutureService.Interface;
using Server_API.Domain.Service.ProcessStatementService;
using Server_API.Domain.Service.ProcessStatementService.Interface;
using Server_API.Infrastructure.Mapper;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

//Set log
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var configuration = builder.Configuration;

// URL da API externa (Server_API - porta 5020)
var apiBaseAddress =
    configuration["ConnectionSettings:ApiBaseAddress"]
    ?? throw new InvalidOperationException(
        "ConnectionSettings:ApiBaseAddress não configurado");

// Porta do Frontend (ServerBB_Web)
var bindPort =
    int.Parse(configuration["ConnectionSettings:BindPort"] ?? "5020");

// Kestrel
// ✔️ Development: Visual Studio / launchSettings controlam
// ✔️ Production: Kestrel escuta na porta configurada

builder.WebHost.ConfigureKestrel(options =>
{
    // escuta em todas as interfaces
    options.ListenAnyIP(bindPort);
});

if (!builder.Environment.IsDevelopment())
{
    // Necessário após publish
    builder.WebHost.UseStaticWebAssets();
}

// Add services to the container.
builder.Services.AddScoped<IBankService, BankService>();

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

builder.Services.AddScoped<IProcessStatementService, ProcessStatrementService>();

builder.Services.AddScoped<IBankService, BankService>();
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

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
app.Run();