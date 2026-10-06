using MedCare.Api.Middlewares;
using MedCare.Application;
using MedCare.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register Exception Handling services
builder.Services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
builder.Services.AddProblemDetails(); // Required dependency for IExceptionHandler in .NET 8/9

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // property names in camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "MedCare Hospital Management System API",
        Version = "v1",
        Description = "Core REST API powering MedCare HMS operations."
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// 4. Configure CORS for Angular Frontend (Local Dev & Production)
const string corsPolicyName = "AllowMedCareFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular CLI dev servers
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

   if (app.Environment.IsDevelopment())
{
   // app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedCare API v1");
        c.RoutePrefix = string.Empty; // Serves Swagger UI at: https://localhost:<port>/swagger
    });
}
else
{
    //app.UseExceptionHandler("/error");
    app.UseHsts();
}
app.UseExceptionHandler(_ => { });
app.UseHttpsRedirection();
app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();

//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
