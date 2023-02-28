namespace RavenPortfolio
{
    public static class Seeder
    {
        public static List<Portfolio> CreatePortfolio(string portfolioId, DateTime start, DateTime end, int symbols)
        {
            var res = new List<Portfolio>();

            int totalDays = Convert.ToInt32(Math.Round((end - start).TotalDays));

            Console.WriteLine($"Seeding {totalDays} days");

            Console.WriteLine($"Seeding {symbols} symbols per day");
            Console.WriteLine($"Total entries: {(totalDays * symbols):n0}");
            
            foreach (int day in Enumerable.Range(0, totalDays))
            {
                var date = start.AddDays(day);

                Portfolio portfolio = new Portfolio();
                portfolio.Date = date;
                portfolio.Id = $"{portfolioId}/{date.Year}/{date.Month}/{date.Day}";

                for (int entry = 0; entry < symbols; entry++)
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

        public static List<SerializedPortfolio> CreateJsonPortfolio(string portfolioId, DateTime start, DateTime end, int symbols)
        {
            List<Portfolio> portfolios = Seeder.CreatePortfolio(portfolioId, start, end, symbols);

            var ret = new List<SerializedPortfolio>();

            foreach (Portfolio portfolio in portfolios)
            {
                ret.Add(
                    new SerializedPortfolio
                    {
                        Id = portfolio.Id,
                        Json = SpanJson.JsonSerializer.Generic.Utf16.Serialize(portfolio)
                    });
            }

            return ret;
        }


        public class SerializedPortfolio
        {
            public string Id { get; set; }

            public string Json { get; set; }
        }
    }
}

/*
from "Portfolios" where startsWith(id(), 'p1/') update {
    del(id(this));
}
*/