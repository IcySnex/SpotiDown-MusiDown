using SpotiDown_MusiDown.Helpers;
using SpotiDown_MusiDown.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opt => opt.InvalidModelStateResponseFactory = context => Error.New(400, "Bad Request.", "Invalid Parameters. Verify your request."))
    .AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();
app.UseMiddleware<ErrorHandler>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();