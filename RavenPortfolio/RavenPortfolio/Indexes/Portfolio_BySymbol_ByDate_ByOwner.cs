using Raven.Client.Documents.Indexes;

namespace RavenPortfolio.Indexes
{
    public class Portfolio_BySymbol_ByDate_ByOwner : AbstractIndexCreationTask<Portfolio, Portfolio_BySymbol_ByDate_ByOwner.Entry>
    {
        public class Entry
        {
            public string Id { get; set; }

            public DateTime Date { get; set; }

            public string Owner { get; set; }

            public string Symbol { get; set; }
        }

        public Portfolio_BySymbol_ByDate_ByOwner()
        {
            Map = portfolios => from portfolio in portfolios
                from entry in portfolio.Entries
                    select new Entry
                    {
                        Id = portfolio.Id,
                        Date = portfolio.Date,
                        Owner = portfolio.Owner,
                        Symbol = entry.Symbol
                    };
        }
    }
}
