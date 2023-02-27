using System.Diagnostics;
using RavenPortfolio;

List<Portfolio> portfolios = Seeder.CreatePortfolio("p1", new DateTime(2000, 1, 1), new DateTime(2010, 1, 1));

Console.WriteLine("STARTED");

Stopwatch sw = Stopwatch.StartNew();

foreach (Portfolio portfolio in portfolios)
{
    using (var session = DocumentStoreHolder.Store.OpenSession())
    { 
        session.Store(portfolio);
        session.SaveChanges();
    }
}

sw.Stop();

Console.WriteLine("COMPLETED");
Console.WriteLine($"Elapsed time: {sw.Elapsed.Seconds}s");
