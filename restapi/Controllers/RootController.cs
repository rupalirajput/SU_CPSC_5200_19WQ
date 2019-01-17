using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using restapi.Models;

namespace restapi.Controllers
{
    public class RootController : Controller
    {
        // GET api/values
        [Route("~/")]
        [HttpGet]
        [Produces(ContentTypes.Root)]
        [ProducesResponseType(typeof(IDictionary<ApplicationRelationship, IList<DocumentLink>>), 200)]
        public IDictionary<ApplicationRelationship, IList<DocumentLink>> Get()
        {
            return new Dictionary<ApplicationRelationship, IList<DocumentLink>>()
            {  
                { 
                    ApplicationRelationship.Timesheets, new List<DocumentLink>() 
                    {
                         new DocumentLink() 
                        { 
                            Method = Method.Get,
                            Type = ContentTypes.Timesheets,
                            Relationship = DocumentRelationship.Timesheets,
                            Reference = "/timesheets"
                        },
                        new DocumentLink()
                        {
                            Method = Method.Post,
                            Type = ContentTypes.Timesheets,
                            Relationship = DocumentRelationship.CreateTimesheet,
                            Reference = "/timesheets"
                        },
                        new DocumentLink()
                        {
                            Method = Method.Delete,
                            Type = ContentTypes.Timesheets,
                            Relationship = DocumentRelationship.DeleteTimesheet,
                            Reference = "/timesheets/{id}"
                        },
                        new DocumentLink()
                        {
                            Method = Method.Put,
                            Type = ContentTypes.Timesheets,
                            Relationship = DocumentRelationship.ReplaceTimeline,
                            Reference = "/timesheets/{id}/lines/{uniqueIdentifier}"
                        },
                        new DocumentLink()
                        {
                            Method = Method.Patch,
                            Type = ContentTypes.Timesheets,
                            Relationship = DocumentRelationship.UpdateTimeline,
                            Reference = "/timesheets/{id}/lines/{uniqueIdentifier}"
                        }
                    }
                }
            };
        }
    }
}
