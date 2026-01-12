using FunctionalDdd.Asp.ModelBinding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // OPTION 1: Route/Query/Form parameter validation only
    // options.AddValueObjectModelBinding();
    
    // OPTION 2: Full support including JSON request bodies (uses reflection)
    options.AddValueObjectJsonInputFormatter(); // Enable JSON body validation
    options.AddValueObjectModelBinding();       // Enable route/query/form validation
});

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
