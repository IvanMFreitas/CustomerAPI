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
    private static readonly Random Random = new Random();

    private static readonly string[] FirstNames = { "John", "Alice", "Bob", "Eve", "Charlie", "David" };
    private static readonly string[] LastNames = { "Smith", "Johnson", "Doe", "Williams", "Brown", "Jones" };

    public string FirstNameOpt => FirstNames[Random.Next(FirstNames.Length)];
    public string LastNameOpt => LastNames[Random.Next(LastNames.Length)];
    public int Age => Random.Next(10, 91);

    public int Id;
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

    /// <summary>
    /// Method to create a Customer
    /// </summary>
    /// <param name="customers"></param>
    /// <returns></returns>
    [HttpPost("customers")]
    public IActionResult AddCustomers([FromBody]List<Customer> customers)
    {
        //Customer is null or empty
        if (customers == null || !customers.Any())
        {
            return BadRequest("No customers provided in the request.");
        }

        var existingCustomers = _memoryCache.Get<List<Customer>>(CustomerCacheKey) ?? new List<Customer>();

        foreach (var customer in customers)
        {

            //Should have First name
            if (string.IsNullOrWhiteSpace(customer.FirstName))
            {
                return BadRequest("Invalid customer data. The customer should have a valid First Name.");
            }

            //Should have Last Name
            if (string.IsNullOrWhiteSpace(customer.LastName))
            {
                return BadRequest("Invalid customer data. The customer should have a valid Last Name.");
            }

            //Should have more than 18 years
            if (customer.Age <= 18)
            {
                return BadRequest("Invalid customer data. The customer should have more than 18 years.");
            }

            //It should be a not-existing Id
            if (existingCustomers.Any(c => c.Id == customer.Id))
            {
                //This ORDER it was just to get the LAST ID. We're not alter anything on the "existingCustomers".
                var lastIdInserted = existingCustomers.OrderByDescending(x => x.Id).First().Id;
                return BadRequest($"Invalid customer data. The customer Id for the Customer {customer.FirstName + " " + customer.LastName} already exists. " + 
                $"Please specify another valid Id. Last ID inserted on \"Database\": {lastIdInserted}");
            }

            //Try to find, using string comparison, where the new record should be putted.
            //First compares the Last Name, OR, compares the Last Name + First Name
            //As soon it finds the correct position on array, insert on that specific position
            int index = existingCustomers.FindIndex(c => string.Compare(c.LastName, customer.LastName, StringComparison.OrdinalIgnoreCase) > 0 ||
                                                       (string.Compare(c.LastName, customer.LastName, StringComparison.OrdinalIgnoreCase) == 0 &&
                                                        string.Compare(c.FirstName, customer.FirstName, StringComparison.OrdinalIgnoreCase) > 0));

            //If didn't find anything, just add to the array
            if (index == -1)
            {
                existingCustomers.Add(customer);
            }
            //If find, insert on the specific index, pushing the array forward
            else
            {
                existingCustomers.Insert(index, customer);
            }
        }

        //Persist the array
        _memoryCache.Set(CustomerCacheKey, existingCustomers);

        //Returns OK
        return Ok("Customers added successfully.");
    }

}