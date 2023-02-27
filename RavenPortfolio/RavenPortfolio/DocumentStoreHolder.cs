using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace RavenPortfolio
{
    public static class DocumentStoreHolder
    {
        public static IDocumentStore Store => LazyStore.Value;

        private static IDocumentStore GetStore()
        {
            return new DocumentStore
            {
                Urls = new[] { "http://127.0.0.1:8080" },
                Database = "portfolio"
            };
        }

        private static readonly Lazy<IDocumentStore> LazyStore =
            new Lazy<IDocumentStore>(() =>
            {
                IDocumentStore store = GetStore();

                store.Initialize();

                return store;
            });
    }
}
