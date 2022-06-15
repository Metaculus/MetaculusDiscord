using System.Security.Cryptography.X509Certificates;
using Raven.Client.Documents;

namespace MetaculusDiscord.Data;

public class DocumentStoreHolder
{
    private static readonly Lazy<IDocumentStore> _store = new(CreateDocumentStore);

    private static IDocumentStore CreateDocumentStore()
    {
        var documentStore = new DocumentStore
        {
            Urls = // urls of the nodes in the RavenDB Cluster
                new string[] {"http://127.0.0.1:8080"},
            // Certificate = 
            //     new X509Certificate2("tasks.pfx"),
            Database = "Tasks"
        };

        documentStore.Initialize();
        return documentStore;
    }

    public static IDocumentStore Store => _store.Value;
}