namespace RavenPortfolio
{
    public static class Seeder
    {
        public static List<Portfolio> Seed()
        {
            List<string> portfolioIdentifiers = new List<string>
            {
                "p1_u1", "p2_u1", "p3_u2", "p4_u3"
            };

            List<string> symbols = new List<string>
            {
                "APPL", "GOOG", "ALPHA"
            };

            DateTime start = new DateTime(2000, 1, 1);

            Random days = new Random();

            List<Portfolio> portfolios = new List<Portfolio>();

            foreach (string identifier in portfolioIdentifiers)
            {
                string portfolioId = identifier.Split('_')[0];
                string ownerId = identifier.Split('_')[1];

                portfolios.AddRange(CreatePortfolio(
                    portfolioId,
                    ownerId,
                    start,
                    days.Next(300, 400),
                    symbols
                    ));
            }

            return portfolios;
        }


        public static List<Portfolio> CreatePortfolio(string portfolioId, string ownerId, DateTime start, int days, List<string> symbols)
        {
            var res = new List<Portfolio>();

            Console.WriteLine($"\nSeeding portfolio {portfolioId} for {ownerId}");

            Console.WriteLine($"Seeding {symbols} symbols per day");
            //Console.WriteLine($"Total entries: {(totalDays * symbols):n0}");
            
            foreach (int day in Enumerable.Range(0, days))
            {
                var date = start.AddDays(day);

                Portfolio portfolio = new Portfolio();
                portfolio.Date = date;
                portfolio.Id = $"{portfolioId}/{date.Year}/{date.Month}/{date.Day}";
                portfolio.Owner = ownerId;

                foreach (string symbol in symbols.OrderBy(x => Guid.NewGuid()))
                {
                    Portfolio.Entry pe = new Portfolio.Entry
                    {
                        Symbol = symbol,
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

        public static List<SerializedPortfolio> CreateJsonPortfolio()
        {
            List<Portfolio> portfolios = Seeder.Seed();

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