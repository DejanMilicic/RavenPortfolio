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
using static System.Formats.Asn1.AsnWriter;
using static Raven.Client.Constants;

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

        RequestExecutor requestExecutor = null;
        JsonSerializer jsonSerializer = null;
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
                        JObject jo = JObject.FromObject(entity);

                        var metadataJObject = new JObject();
                        foreach (var keyValue in metadata)
                        {
                            metadataJObject.Add(keyValue.Key, new JValue(keyValue.Value));
                        }

                        jo[Raven.Client.Constants.Documents.Metadata.Key] = metadataJObject;

                        var jsonWriter = new JsonTextWriter(writer);

                        jsonWriter.Flush();
                        jsonSerializer ??= new JsonSerializer();
                        jsonSerializer.Serialize(jsonWriter, jo);
                        jsonWriter.Flush();

                        return true;
                    }
                }
            }
        })
        {
            store.Initialize();

            Console.WriteLine("Ready...");

            // here to force a request for RavenDB, nothing else. So the benchmark won't have to create
            // the connection to the server, we can assume that this is already there
            store.Maintenance.Send(new Raven.Client.Documents.Operations.GetStatisticsOperation());

            var sp = Stopwatch.StartNew();

            using (var bulk = store.BulkInsert())
            {
                foreach (var portfolio in portfolios)
                {
                    bulk.Store(portfolio);
                }
            }

            //var docs = 300000;
            //int threadsToUse = 1;
            //var threads = new Thread[threadsToUse];

            //for (int index = 0; index < threads.Length; index++)
            //{
            //    threads[index] = new Thread(() => DoBulkInsert(portfolios, store, docs));
            //    threads[index].Start();
            //}
            //foreach (var thread in threads)
            //{
            //    thread.Join();
            //}
            Console.WriteLine($"Elapsed time: {sp.Elapsed}");

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