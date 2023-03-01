using Raven.Client.Documents.Indexes;

namespace RavenPortfolio.Indexes
{
    public class Portfolio_BySymbol_ByDate : AbstractIndexCreationTask<Portfolio, Portfolio_BySymbol_ByDate.Entry>
    {
        public class Entry
        {
            public DateTime Date { get; set; }

            public string Symbol { get; set; }

            public string[] Owners { get; set; }
        }

        public Portfolio_BySymbol_ByDate()
        {
            Map = portfolios => from portfolio in portfolios
                    from entry in portfolio.Entries
                        select new Entry
                        {
                            Date = portfolio.Date,
                            Symbol = entry.Symbol,
                            Owners = new [] { portfolio.Owner }
                        };

            Reduce = results => from result in results
                group result by new
                {
                    result.Date,
                    result.Symbol
                }
                into g
                select new Entry
                {
                    Date = g.Key.Date,
                    Symbol = g.Key.Symbol,
                    Owners = g.SelectMany(x => x.Owners).Distinct().ToArray()
                };
        }
    }
}
