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

        // GET: api/Document/5
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
