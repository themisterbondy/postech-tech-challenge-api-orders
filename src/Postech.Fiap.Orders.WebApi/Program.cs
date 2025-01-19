using Postech.Fiap.Orders.WebApi.Common.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = AppSettings.Configuration();
builder.Services.AddWebApi(configuration);
builder.Services.AddSerilogConfiguration(builder, configuration);


var app = builder.Build();
app.ApplyMigrations();
app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();
app.UseHttpsRedirection();
app.UseHealthChecksConfiguration();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestContextLoggingMiddleware>();
app.MapCarter();
app.Run();

namespace Postech.Fiap.Orders.WebApi
{
    [ExcludeFromCodeCoverage]
    public partial class Program;
}