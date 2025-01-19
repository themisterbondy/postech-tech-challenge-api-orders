using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = AppSettings.Configuration();
builder.Services.AddWebApi(configuration);
builder.Services.AddSerilogConfiguration(builder, configuration);


var app = builder.Build();
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

[ExcludeFromCodeCoverage]
public partial class Program;