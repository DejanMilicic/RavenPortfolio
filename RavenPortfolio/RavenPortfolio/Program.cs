using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Conventions;
using Raven.Client.Http;
using Raven.Client.Json;
using RavenPortfolio;
using Sparrow.Json;

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

            using (var bulk = store.BulkInsert())
            {
                foreach (var portfolio in portfolios)
                {
                    bulk.Store(portfolio.Json, portfolio.Id);
                }
            }

            Console.WriteLine($"Completed, Elapsed time: {sp.Elapsed}");
        }
    }
}
