var builder = WebApplication.CreateBuilder(args);

//Injection
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Add the CustomersController as a route.
app.MapControllers();

// Run the application.
app.Run();