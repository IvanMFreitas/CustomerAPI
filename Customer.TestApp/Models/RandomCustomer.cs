namespace Customer.TestApp.Models
{
    /// <summary>
    /// Class with names to Randomize inserts
    /// </summary>
    public class RandomCustomer
    {
        private bool over18 = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["OnlyOver18"]);

        private static readonly Random Random = new Random();

        private static readonly string[] FirstNames = { "John", "Alice", "Bob", "Eve", "Charlie", "David" };
        private static readonly string[] LastNames = { "Smith", "Johnson", "Doe", "Williams", "Brown", "Jones" };

        public int Id => Random.Next(1, 999);
        public string FirstNameOpt => FirstNames[Random.Next(FirstNames.Length)];
        public string LastNameOpt => LastNames[Random.Next(LastNames.Length)];
        public int Age => over18 ? Random.Next(18, 91) : Random.Next(10, 91);

    }
}
