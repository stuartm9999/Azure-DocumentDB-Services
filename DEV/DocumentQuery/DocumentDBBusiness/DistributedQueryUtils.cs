using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace DocumentDBBusiness
{
    /// <summary>
    /// This class provides utilities to run queries accross multiple database
    /// and subscriptions.
    /// </summary>
    public class DistributedQueryUtils
    {
        public Uri EndpointUrl { get; set; }

        public String AuthorizationKey { get; set; }
        private DocumentClient GetDocumentClient()
        {
            var client = new DocumentClient(EndpointUrl, AuthorizationKey);
            return client;
        }

        static Dictionary<String, Microsoft.Azure.Documents.DocumentCollection> collections = new Dictionary<string, DocumentCollection>();

        public async Task<DocumentCollection> GetDocumentCollectionAsync(DocumentClient client, Database database, String collection)
        {
            DocumentCollection documentCollection = null;
            // get the week number
            // check to see if we've got it
            lock (collections)
            {
                if (collections.ContainsKey(collection))
                {
                    documentCollection = collections[collection];
                }
            }
            if (null == documentCollection)
            {
                documentCollection = client.CreateDocumentCollectionQuery("dbs/" + database.Id).Where(c => c.Id == collection).AsEnumerable().FirstOrDefault();
                // If the document collection does not exist, create a new collection
                if (documentCollection == null)
                {
                    documentCollection = await client.CreateDocumentCollectionAsync("dbs/" + database.Id,
                        new DocumentCollection
                        {
                            Id = collection
                        });

                }
                lock (documentCollection)
                {
                    if (collections.ContainsKey(collection))
                    {
                        collections[collection] = documentCollection;
                    }
                    else
                    {
                        collections.Add(collection, documentCollection);
                    }
                }
            }

            return documentCollection;

        }

        public IList<Document> QueryAllDatabase(String query)
        {
            IList<Document> docs = QueryAllDatabases(query, GetDocumentClient());

            return docs;
        }
        public static IList<Document>  QueryAllDatabases(String query,
            DocumentClient client)
        {
            IList<Document> docs = QueryAllDocumentCollections(null, query, client);
           
            return docs;
        }

        public IList<Document> QueryAllDocumentCollections(String databaseName,
           String query)
        {
            return QueryAllDocumentCollections(databaseName,
                query,
                GetDocumentClient());
        }

        /// <summary>
        /// Query all document collections in the database name passed in OR 
        /// null for all databases.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="query"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IList<Document> QueryAllDocumentCollections(String databaseName, 
            String query, 
            DocumentClient client)
        {

            // get the collection of documents to look at 
            IEnumerable<Database> databases;
            List<Document> ret = new List<Document>();
            // for thread safety
            ConcurrentStack<Document> retStack = new ConcurrentStack<Document>();
            // get the database matching the name or all
            if (null != databaseName)
            {
                databases = client.CreateDatabaseQuery().Where(db => db.Id == databaseName).AsEnumerable();
            }
            else
            {
                databases = client.CreateDatabaseQuery();
            }
            List<Task> tasks = new List<Task>();
            // create a query for each collection on each database
            foreach (Database item in databases)
            {

                IEnumerable<DocumentCollection> dcollections = client.CreateDocumentCollectionFeedReader(item.CollectionsLink);
                foreach (DocumentCollection dc in dcollections)
                {
                    QueryDocumentCollection(query, client, retStack, tasks, item, dc);

                }
            }
            Task.WaitAll(tasks.ToArray());
            ret.AddRange(retStack);
            return ret;
        }

        /// <summary>
        /// Query thge document collection -
        /// fill the retStack with the Documents matching the query.
        /// The operation is not complete until the list of tasks has finished
        /// - yopu need to Task.WaitAll(tasks.ToArray());
        /// </summary>
        /// <param name="query"></param>
        /// <param name="client"></param>
        /// <param name="retStack"></param>
        /// <param name="tasks"></param>
        /// <param name="item"></param>
        /// <param name="dc"></param>
        private static void QueryDocumentCollection(String query,
            DocumentClient client,
            ConcurrentStack<Document> retStack,
            List<Task> tasks,
            Database item,
            DocumentCollection dc)
        {
            Task<IQueryable<dynamic>> task = new Task<IQueryable<dynamic>>(() => ExecuteQuery(client,
                item,
                dc,
                query
                ));
            Task continuation = task.ContinueWith((prevTask) => AddCollection(retStack, prevTask));
            tasks.Add(continuation);
            task.Start();
        }
        static IQueryable<dynamic> ExecuteQuery(DocumentClient client,
            Database db,
            DocumentCollection dc,
            String query)
        {
            return client.CreateDocumentQuery("dbs/" + db.Id + "/colls/" + dc.Id
                   , query);
        }

        static void AddCollection(ConcurrentStack<Document> retDocs,
            Task<IQueryable<dynamic>> docsToAdd)
        {
            if (!docsToAdd.IsFaulted)
            {
                foreach (Document item in docsToAdd.Result.AsEnumerable())
                {
                    retDocs.Push(item);

                }

            }
            else
            {
                throw docsToAdd.Exception;
            }
        }
    }

}
