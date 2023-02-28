namespace RavenPortfolio
{
    public static class Seeder
    {
        public static List<Portfolio> CreatePortfolio(string portfolioId, DateTime start, DateTime end)
        {
            var res = new List<Portfolio>();

            int totalDays = Convert.ToInt32(Math.Round((end - start).TotalDays));

            Console.WriteLine($"Seeding {totalDays} days");

            int entries = 3000;
            Console.WriteLine($"Seeding {entries} entries per day");
            Console.WriteLine($"Total entries: {(totalDays * entries):n0}");
            
            foreach (int day in Enumerable.Range(0, totalDays))
            {
                var date = start.AddDays(day);

                Portfolio portfolio = new Portfolio();
                portfolio.Date = date;
                portfolio.Id = $"{portfolioId}/{date.Year}/{date.Month}/{date.Day}";

                for (int entry = 0; entry < entries; entry++)
                {
                    Portfolio.Entry pe = new Portfolio.Entry
                    {
                        Symbol = "GOOG",
                        Price = 100.12m,
                        Quantity = 100,
                        Factor1 = 111,
                        Factor2 = 222,
                        Factor3 = 333
                    };

                    portfolio.Entries.Add(pe);
                }

                res.Add(portfolio);
            }
            
            Console.WriteLine();

            return res;
        }
    }
}

/*
from "Portfolios" where startsWith(id(), 'p1/') update {
    del(id(this));
}
*/