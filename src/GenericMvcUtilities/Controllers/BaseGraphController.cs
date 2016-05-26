using GenericMvcUtilities.Models;
using GenericMvcUtilities.Repositories;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Controllers
{
    public class BaseGraphController<T, TKey> : BaseApiController<T, TKey>, IBaseGraphController<T, TKey>
        where T : class, IModel<TKey>
        where TKey : IEquatable<TKey>
    {

        public BaseGraphController(BaseEntityFrameworkRepositroy<T> repository, ILogger<T> logger) : base(repository, logger)
        {

        }

        private static IEnumerable<Microsoft.Data.Entity.Metadata.IEntityType> EntityTypes;

        //todo more design work
        //todo: finish
        //todo: add unit test
        //[NonAction]
        [HttpDelete, Route("[controller]/[action]/")]
        public async Task<IActionResult> DeleteChild([FromBody]Newtonsoft.Json.Linq.JObject child)
        {
            try
            {
                if (child != null)
                {
                    if (ModelState.IsValid)
                    {
                        //first make sure type isn't the root of the object graph, in this case type T
                        if (EntityTypes == null)
                        {
                            EntityTypes = Repository.DataContext.Model.GetEntityTypes();
                        }

                        object dbObj = null;

                        foreach (var type in EntityTypes)
                        {
                            //EntityTypes.Any(x => x.ClrType.FullName == child["$type"].ToString())
                            if (type.ClrType.FullName == child["$type"].ToString())
                            {
                                dbObj = child.ToObject(type.ClrType);
                                break;
                            }
                        }

                        //EntityTypes.Any(x => x.ClrType == child.GetType())
                        if (dbObj != null)
                        {
                            if (Repository.DataContext.Entry(dbObj).State == EntityState.Detached)
                            {
                                Repository.DataContext.Attach(dbObj, GraphBehavior.IncludeDependents);
                            }

                            Repository.DataContext.Remove(dbObj);

                            if (await Repository.DataContext.SaveChangesAsync() > 0)
                            {
                                return new NoContentResult();
                            }
                            else
                            {
                                throw new Exception("Object was not removed from DB");
                            }
                        }
                        else
                        {
                            return HttpNotFound("Object Must Support a '$type' field or property");
                        }

                        //todo: maybe cache entity types in a field?
                        //todo: check type T in controller constructor as well
                        //todo: check type T in repository constructor as well
                        //Second make sure the type is present in the data-context 
                        //One possibility
                        //third delete the object
                        //Repository.DataContext.
                        //forth save changes
                    }
                }

                return HttpBadRequest(ModelState);
            }
            catch (Exception ex)
            {
                string Message = "Delete Child - HTTP Delete Request Failed";

                this.Logger.LogError(this.FormatLogMessage(Message, this.Request));

                throw new Exception(this.FormatExceptionMessage(Message), ex);
            }
        }
    }
}
