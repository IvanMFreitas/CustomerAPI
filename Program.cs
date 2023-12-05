using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

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

/// <summary>
/// Class Structure
/// </summary>
public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set;}
    public int Id { get; set;}
}

/// <summary>
/// Class with names to Randomize inserts
/// </summary>
public class RandomCustomer
{
    public List<string> FirstNameOpt = new List<string>
    {
        "Leia", "Sadie", "Jose", "Sara", "Frank", "Dewey", "Tomas", "Joel", "Lukas", "Carlos"
    };

    public List<string> LastNameOpt = new List<string>
    {
        "Liberty", "Ray", "Harrison", "Ronan", "Drew", "Powell", "Larsen", "Chan", "Anderson", "Lane"
    };
}

/// <summary>
/// Controller Class
/// </summary>
public class CustomerController : ControllerBase 
{
    private readonly IMemoryCache _memoryCache;
    private const string CustomerCacheKey = "Customers";

    /// <summary>
    /// Constructor Initialized  - For Debug purpose
    /// </summary>
    /// <param name="memoryCache"></param>
    public CustomerController(IMemoryCache memoryCache)
    {
        //Receives the MemoryCache as param
        _memoryCache = memoryCache;

        // Initialize memory cache with preloaded data if it's not already populated
        if (!_memoryCache.TryGetValue(CustomerCacheKey, out _))
        {
            var initialData = new List<Customer>
            {
                new Customer { FirstName = "John", LastName = "Doe", Age = 30, Id = 1 },
                new Customer { FirstName = "Alice", LastName = "Smith", Age = 25, Id = 2 },
            };

            _memoryCache.Set(CustomerCacheKey, initialData);
        }
    }

    /// <summary>
    /// Method GET to return all customers
    /// </summary>
    /// <returns></returns>
    [HttpGet("customers")]
    public IActionResult GetCustomers()
    {
        var existingCustomers = _memoryCache.Get<List<Customer>>(CustomerCacheKey) ?? new List<Customer>();

        return Ok(existingCustomers);
    }

    /// <summary>
    /// Method to return the last Id to insert
    /// </summary>
    /// <returns></returns>
    [HttpGet("customers/getLastId")]
    public IActionResult GetLastId()
    {
        //Gets the list of Customers
        var existingCustomers = _memoryCache.Get<List<Customer>>(CustomerCacheKey) ?? new List<Customer>();
        
        //Returns the last ID or "1" if the list is Empty
        return Ok(existingCustomers.Any() 
            ? existingCustomers.OrderByDescending(x => x.Id).First().Id
            : 1);
    }


}





