# Coding test .NET
This is the second part of the test, developing an API that creates and retrieves Customers, by following some rules.

## Summary

The solution has 2(two) projects: a WebAPI and a Console Application to test the API with parallel processes.

- [WebAPI](#WebAPI)
- [ConsoleApplication](#ConsoleApplication)

# WebAPI

## Requests

- The API should have 2(two) endpoints: basic POST (to create a customer) and basic GET (to retrieve all customers);
- Multiple customers should be accepted;

## Validations


The customer should have some fields checked:
- First Name and Last Name are mandatory;
- Age should be above 18;
- The ID should not be used before;

The API also shouldn't use any sorting method;


## Running the API

Simply run the default commands for a .Net (former "core") project:
`dotnet build`
`dotnet run`

Just remember that these commands should be run on the API directory: `CustomerAPI/Customer.API`

## Endpoints

The API has 2 endpoints

### POST

#### :arrow_up:Creates a new customer
```http
POST /customer
```
and the body for this POST invoke, should be like this:

```
[
	{
		"FirstName"  :  "Ivan",
		"LastName":  "Freitas",
		"Age":  39,
		"Id":  1
	}
]
```

### GET

#### :arrow_down:Retrieves all customers
```http
GET /customer
```

## Persisting data

The approach that was chosen, is to work, on the lifetime of API with MemoryCache, to save the array with customers.
And, when the API lifecycle ends (by shutting down correctly - NOT in DEBUG mode), the array is saved onto a file in the directory of the Application. `customerData.json`
By that, when the API is restarted (using `dotnet run`) if the file was not deleted, all the arrays previously created should be populated again.


# ConsoleApplication

## Used approach

A Console application was developed to send multiple and parallel requests to our API.
It is easy to use, including the configuration

## Configuration and usage

In the same directory of the project (Customer.TestApp), you can see an "app.config" file. 
- After the API is running, the app.config file should be changed, to point to the correct API URL, by changing the code:  `<add  key="APIUrl"  value="http://localhost:5031"/>`;
- Run the command `dotnet build` and then `dotnet run`, to run the TestApp; 
- All information should be presented at the console;
