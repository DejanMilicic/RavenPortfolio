using System.Collections.Concurrent;
using System.Diagnostics;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Json;
using RavenPortfolio;
using Sparrow.Json;
using static System.Formats.Asn1.AsnWriter;

class Program
{
    static async Task Main(string[] args)
    {
        List<Portfolio> portfolios = Seeder.CreatePortfolio("p1", new DateTime(2000, 1, 1), new DateTime(2010, 1, 1));

        static void DoBulkInsert(List<Portfolio> portfolios, DocumentStore store, int docs)
        {
            using (var bulk = store.BulkInsert())
            {
                for (int i = 0; i < docs; i++)
                {
                    var portfolio = portfolios[i % portfolios.Count];
                    bulk.Store(portfolio);
                }
            }
        }

        var defaultConventions = new DocumentConventions();
        var cachedData = new ConcurrentDictionary<object, string>();
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
                        if (cachedData.TryGetValue(entity, out var c))
                        {
                            writer.Write(c);
                            return true;
                        }
                        using (var ctx= JsonOperationContext.ShortTermSingleUse())
                        using (var json = defaultConventions.Serialization.DefaultConverter.ToBlittable(entity,metadata, ctx))
                        {
                            c = json.ToString();
                            cachedData.TryAdd(entity, c);
                            writer.Write(c);
                        }
                        return true;
                    }
                }
            }
        })
        {
            store.Initialize();
            Console.WriteLine("Init cache...");
            Parallel.ForEach(portfolios, p =>
            {
                store.Conventions.BulkInsert.TrySerializeEntityToJsonStream(p, new MetadataAsDictionary(),
                    StreamWriter.Null);
            });
            Console.WriteLine("Ready...");

            // here to force a request for RavenDB, nothing else. So the benchmark won't have to create
            // the connection to the server, we can assume that this is already there
            store.Maintenance.Send(new Raven.Client.Documents.Operations.GetStatisticsOperation());

            var docs = 300000;
            int threadsToUse = 1;
            var threads = new Thread[threadsToUse];

            for (int index = 0; index < threads.Length; index++)
            {
                threads[index] = new Thread(() => DoBulkInsert(portfolios, store, docs));
                threads[index].Start();
            }
            var sp = Stopwatch.StartNew();
            foreach (var thread in threads)
            {
                thread.Join();
            }
            Console.WriteLine(sp.Elapsed);

        }

        //var tasks = new List<Task>();

        //foreach (var chunk in portfolios.Chunk(50))
        //{
        //    tasks.Add(
        //        Task.Run(async () =>
        //        {
        //            using (var session = DocumentStoreHolder.Store.OpenAsyncSession())
        //            {
        //                foreach (Portfolio p in chunk)
        //                    await session.StoreAsync(p);

        //                await session.SaveChangesAsync();
        //            }
        //        }
        //    ));
        //}

        //Console.WriteLine($"STARTED @ {DateTime.Now}");

        //long numberOfInserts = 0;

        //foreach (var chunk in portfolios.Chunk(50))
        //    using (var bulkInsert = DocumentStoreHolder.Store.BulkInsert())
        //    {
        //        bulkInsert.OnProgress += (sender, args) => { numberOfInserts = args.Progress.Total; };

        //        foreach (Portfolio p in chunk)
        //            await bulkInsert.StoreAsync(p);
        //    }
        

        //Console.WriteLine($"END @ {DateTime.Now}");
        //Console.WriteLine($"Inserted {numberOfInserts} of {portfolios.Count}");
    }
}

/*
        using (var bulkInsert = DocumentStoreHolder.Store.BulkInsert())
        {
            bulkInsert.OnProgress += (sender, args) =>
            {
                numberOfInserts = args.Progress.Total;
            };

            foreach (Portfolio p in portfolios)
                await bulkInsert.StoreAsync(p);
        }
*/