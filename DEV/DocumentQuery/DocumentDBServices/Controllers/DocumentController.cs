using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using DocumentDBBusiness;
using Microsoft.Azure.Documents;

namespace DocumentDBServices.Controllers
{
    public class DocumentController : ApiController
    {
        // GET: api/Document
        //public IEnumerable<Document> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        /// <summary>
        /// Run the query against all the Databases and all the 
        /// Containers in those databaseas for the given service.
        /// </summary>
        /// <param name="endpointUrl">Service URL</param>
        /// <param name="authorizationKey">access key</param>
        /// <param name="query">The query to execute</param>
        /// <returns>Documents found</returns>
        /// <remarks>Please url encode the paramaters - in particular the key
        /// Try the service here - http://www.url-encode-decode.com/ </remarks>
        public HttpResponseMessage Get(String endpointUrl,
            String authorizationKey,
            String query)
        {
                DistributedQueryUtils dbQuery = new DistributedQueryUtils()
            {
                AuthorizationKey = authorizationKey,
                EndpointUrl = new Uri(endpointUrl)

            };

            
            IList<Document> res = dbQuery.QueryAllDatabase(query);
            StringBuilder builder = new StringBuilder();
            
                    foreach (Document d in res)
                    {
                        /*
                        d.SaveTo(ms,
                            SerializationFormattingPolicy.Indented);
                        */
                        builder.Append(d);

                    }


            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(builder.ToString(), Encoding.UTF8, "application/json");
        
            return response;
          
        }

        //// POST: api/Document
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Document/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Document/5
        //public void Delete(int id)
        //{
        //}
    }
}
