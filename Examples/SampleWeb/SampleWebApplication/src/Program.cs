using FunctionalDdd;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddScalarValueValidation(); // ? Enables automatic value object validation

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseScalarValueValidation(); // ? Must be before routing for validation error collection

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();