using System.Diagnostics;
using Raven.Client.Documents;
using RavenPortfolio;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    public static async Task BulkInsert()
    {
        Console.WriteLine("Starting seeding...");

        var portfolios = Seeder.CreateJsonPortfolio(
            "p1",
            new DateTime(2000, 1, 1),
            new DateTime(2010, 1, 1),
            3000);

        using (var store = new DocumentStore
        {
            Database = "portfolio",
            Urls = new[] { "http://127.0.0.1:8080" },
            Conventions =
                   {
                       BulkInsert =
                       {
                           TrySerializeEntityToJsonStream = (entity, metadata, writer) =>
                           {
                               writer.Write((string)entity);
                               return true;
                           }
                       }
                   }
        })
        {
            store.Initialize();

            Console.WriteLine("Starting insert...");

            // here to force a request for RavenDB, nothing else. So the benchmark won't have to create
            // the connection to the server, we can assume that this is already there
            store.Maintenance.Send(new Raven.Client.Documents.Operations.GetStatisticsOperation());

            var sp = Stopwatch.StartNew();

            var tasks = new List<Task>();

            foreach (var chunk in portfolios.Chunk(1000))
            {
                tasks.Add(
                    Task.Run(async () =>
                        {
                            await using var bulk = store.BulkInsert();
                            foreach (var p in chunk)
                            {
                                await bulk.StoreAsync(p.Json, p.Id);
                            }
                        }
                    ));
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Completed, Elapsed time: {sp.Elapsed}");
        }
    }

    public static async Task GetPortfolioForDay(string symbol, int year, int month, int day)
    {
        using (var store = new DocumentStore
               {
                   Database = "portfolio",
                   Urls = new[] { "http://127.0.0.1:8080" }
               })
        {
            store.Initialize();

            // here to force a request for RavenDB, nothing else. So the benchmark won't have to create
            // the connection to the server, we can assume that this is already there
            store.Maintenance.Send(new Raven.Client.Documents.Operations.GetStatisticsOperation());

            Console.WriteLine("Starting fetch...");
            var sp = Stopwatch.StartNew();
            var session = store.OpenAsyncSession();
            Portfolio p = await session.LoadAsync<Portfolio>($"{symbol}/{year}/{month}/{day}");
            Console.WriteLine($"Completed, Elapsed time: {sp.Elapsed}");
        }
    }

    static async Task Main(string[] args)
    {
        //await BulkInsert();

        //await GetPortfolioForDay("p1", 2005, 5, 5);
    }

}
