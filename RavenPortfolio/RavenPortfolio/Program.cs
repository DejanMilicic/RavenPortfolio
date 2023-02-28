using System.Diagnostics;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using RavenPortfolio;

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

    public static async Task GetPortfolioForMonth(string symbol, int year, int month)
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
            Portfolio[] monthlyPortfolio = 
                await session.Query<Portfolio>()
                .Where(x => x.Id.StartsWith($"{symbol}/{year}/{month}/"))
                .ToArrayAsync();
            Console.WriteLine($"Completed, Elapsed time: {sp.Elapsed}");
        }
    }

    public static async Task DeletePortfolio(string portfolioId)
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

            Console.WriteLine("Starting deletion...");
            var sp = Stopwatch.StartNew();
            
            var operation = await store
                .Operations
                .SendAsync(new DeleteByQueryOperation(new IndexQuery
                {
                    Query = $"from 'Portfolios' where startsWith(id(), '{portfolioId}/')"
                }));

            await operation.WaitForCompletionAsync(TimeSpan.FromSeconds(15));


            Console.WriteLine($"Completed, Elapsed time: {sp.Elapsed}");
        }
    }

    static async Task Main(string[] args)
    {
        //await BulkInsert();

        //await GetPortfolioForDay("p1", 2005, 5, 5);

        //await GetPortfolioForMonth("p1", 2005, 5);
        
        //await DeletePortfolio("p1");
    }
}
