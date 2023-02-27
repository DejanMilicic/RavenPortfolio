﻿namespace RavenPortfolio
{
    public static class Seeder
    {
        public static List<Portfolio> CreatePortfolio(string portfolioId, DateTime start, DateTime end)
        {
            var res = new List<Portfolio>();

            int totalDays = Convert.ToInt32(Math.Round((end - start).TotalDays));

            foreach (int day in Enumerable.Range(0, totalDays))
            {
                var date = start.AddDays(day);

                Portfolio portfolio = new Portfolio();
                portfolio.Date = date;
                portfolio.Id = $"{portfolioId}/{date.Year}/{date.Month}/{date.Day}";

                for (int entry = 0; entry < 3000; entry++)
                {
                    Portfolio.Entry pe = new Portfolio.Entry
                    {
                        Symbol = "x",
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

            return res;
        }
    }
}

/*
from "Portfolios" where startsWith(id(), 'p1/') update {
    del(id(this));
}
*/