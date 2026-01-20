using FunctionalDdd;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers
builder.Services.AddControllers();

// Enable automatic value object validation for MVC
// - [FromBody] JSON: validated via JSON converter
// - [FromQuery], [FromRoute], [FromForm]: validated via model binder
builder.Services.AddValueObjectModelBinding();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable validation scope for [FromBody] JSON
app.UseValueObjectValidation();

app.UseAuthorization();
app.MapControllers();

app.Run();
