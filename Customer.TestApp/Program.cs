using Customer.TestApp.Models;
using Newtonsoft.Json;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        var apiUrl = System.Configuration.ConfigurationManager.AppSettings["APIUrl"];
        var httpClient = new HttpClient();

        Console.WriteLine($"Testing on {apiUrl}");

        var postTask = SimulatePostRequests(apiUrl, httpClient);
        var getTask = SimulateGetRequest(apiUrl, httpClient);    

        await Task.WhenAll(postTask, getTask);
    }

    static async Task SimulatePostRequests(string apiUrl, HttpClient httpClient)
    {
        var random = new Random();

        var randomCustomer = new RandomCustomer();

        var customers = new List<Customer.TestApp.Models.Customer>();

        for (int i = 0; i < random.Next(2, 20); i++)
        {
            var c = new Customer.TestApp.Models.Customer
            {
                FirstName = randomCustomer.FirstNameOpt,
                LastName = randomCustomer.LastNameOpt,
                Age = randomCustomer.Age,
                Id = randomCustomer.Id
            };

            customers.Add(c);

        }

        var json = JsonConvert.SerializeObject(customers);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{apiUrl}/customers", content);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"POST Request Status Code: {response.StatusCode}.");
        }
        else
        {
            Console.WriteLine($"POST Request Status Code: {response.StatusCode}. Error: {response.Content.ToString()}");
        }
    }

    static async Task SimulateGetRequest(string apiUrl, HttpClient httpClient)
    {
        var response = await httpClient.GetAsync($"{apiUrl}/customers");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<List<Customer.TestApp.Models.Customer>>(content);

            Console.WriteLine("GET Request Result:");
            foreach (var customer in customers)
            {
                Console.WriteLine($"{customer.FirstName} {customer.LastName}, Age: {customer.Age}, ID: {customer.Id}");
            }
        }
        else
        {
            Console.WriteLine($"GET Request Status Code: {response.StatusCode}");
        }
    }
}