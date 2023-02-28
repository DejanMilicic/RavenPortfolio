using System.Diagnostics;
using Raven.Client.Documents;
using RavenPortfolio;

class Program
{
    static async Task Main(string[] args)
    {
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
}
