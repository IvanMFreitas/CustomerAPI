using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Customer.API.Controllers
{
    /// <summary>
    /// Controller Class
    /// </summary>
    public class CustomerController : ControllerBase
    {
        /// <summary>
        /// Memory cache
        /// </summary>
        private readonly IMemoryCache _memoryCache;
        
        /// <summary>
        /// Key to hold info into memoryCache
        /// </summary>
        private const string CustomerCacheKey = "Customers";
        
        /// <summary>
        /// Backup file path
        /// </summary>
        private const string FilePath = "customerData.json";

        /// <summary>
        /// Instance of a LifeTime monitor of the API
        /// </summary>
        private readonly IHostApplicationLifetime _appLifetime;

        /// <summary>
        /// Constructor Initialized  - For Debug purpose
        /// </summary>
        /// <param name="memoryCache"></param>
        public CustomerController(IMemoryCache memoryCache, IHostApplicationLifetime appLifetime)
        {
            //Receives the MemoryCache as param
            _memoryCache = memoryCache;

            // Load data from file when the API starts
            LoadDataFromFile();

            //Receives the instance
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));

            // Registering an application-stopping event
            _appLifetime.ApplicationStopping.Register(OnShutdown);
        }

        /// <summary>
        /// Method GET to return all customers
        /// </summary>
        /// <returns></returns>
        [HttpGet("customers")]
        public IActionResult GetCustomers()
        {
            var existingCustomers = _memoryCache.Get<List<Models.Customer>>(CustomerCacheKey) ?? new List<Models.Customer>();

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
            var existingCustomers = _memoryCache.Get<List<Models.Customer>>(CustomerCacheKey) ?? new List<Models.Customer>();

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
        public IActionResult AddCustomers([FromBody] List<Models.Customer> customers)
        {
            //Customer is null or empty
            if (customers == null || !customers.Any())
            {
                return BadRequest("No customers provided in the request.");
            }

            var existingCustomers = _memoryCache.Get<List<Models.Customer>>(CustomerCacheKey) ?? new List<Models.Customer>();

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
                    
                    //ATTENTION: This code is just for Debug purpose. 
                    //Instead of ignoring the user, because of the repeated ID
                    //we're changing the ID to accept all records;
                    customer.Id = lastIdInserted + 1;
                    

                    //ATTENTION: This is the code VALIDATED with IDs that already exists
                    /*return BadRequest($"Invalid customer data. The customer Id for the Customer {customer.FirstName + " " + customer.LastName} already exists. " +
                    $"Please specify another valid Id. Last ID inserted on \"Database\": {lastIdInserted}");*/
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

        /// <summary>
        /// Method to Save data into a file, even after the API shutdown
        /// </summary>
        private void SaveDataToFile()
        {
            var existingCustomers = _memoryCache.Get<List<Models.Customer>>(CustomerCacheKey) ?? new List<Models.Customer>();

            var serializedData = JsonConvert.SerializeObject(existingCustomers);
            System.IO.File.WriteAllText(FilePath, serializedData);
        }

        /// <summary>
        /// Method that loads all Data from a file (previnting losing data)
        /// </summary>
        private void LoadDataFromFile()
        {
            if (System.IO.File.Exists(FilePath))
            {
                var serializedData = System.IO.File.ReadAllText(FilePath);
                var existingCustomers = JsonConvert.DeserializeObject<List<Models.Customer>>(serializedData);
                _memoryCache.Set(CustomerCacheKey, existingCustomers);
            }
        }

        /// <summary>
        /// Save data to file when the API shuts down 
        /// </summary>
        private void OnShutdown()
        {
            SaveDataToFile();
        }

    }
}
