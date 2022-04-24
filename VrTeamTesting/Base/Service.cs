using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VrTeamTesting.ConnectionString;

namespace VrTeamTesting.Base
{
    public class Service 
    {
        protected static ServiceClient dynamics;


        private readonly string EntityLogicalName;
        public Service()
        {
            dynamics = ConnnectionManager.Connection.Get();

        }

        public async Task<FetchXmlToQueryExpressionResponse> ToQueryExpression(string fetchXML)
        {
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = fetchXML
            };

            return (FetchXmlToQueryExpressionResponse)await dynamics.ExecuteAsync(conversionRequest);
        }

        public Service(string entityLogicalName)
        {
            EntityLogicalName = entityLogicalName;
            dynamics = ConnnectionManager.Connection.Get();
        }

        public async Task<List<Entity>> GetAll(QueryExpression query = null)
        {
            query ??= new QueryExpression()
            {
                EntityName = EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            if (query.ColumnSet.AllColumns == false && query.ColumnSet.Columns.Count == 0)
                query.ColumnSet = new ColumnSet(true);

            query.Orders.Add(new OrderExpression() { AttributeName = "createdon", EntityName = query?.EntityName, OrderType = OrderType.Descending });

            var result = await dynamics.RetrieveMultipleAsync(query);
            return result.Entities.ToList();
        }
        public async Task<Guid> CreateUpdate(Model request)
        {
            Entity updatedEntity = new Entity(request.EntityLogicalName, request.id);
            if (Guid.Empty.Equals(request.id))
                updatedEntity = await Create(request);
            else
                updatedEntity = await Update(request.id, request);

            return updatedEntity.Id;
        }

        public async Task<Entity> GetById(Guid Id, QueryExpression query = null)
        {
            if (query == null)
                return await dynamics.RetrieveAsync(EntityLogicalName, Id, query?.ColumnSet ?? new ColumnSet(true));

            query.Criteria.AddCondition(EntityLogicalName + "id", ConditionOperator.Equal, Id);

            if (query.ColumnSet.AllColumns == false && query.ColumnSet.Columns.Count == 0)
                query.ColumnSet = new ColumnSet(true);

            var resultSet = await dynamics.RetrieveMultipleAsync(query);


            if (resultSet.Entities.Count == 0)
                throw new Exceptions.NotFoundException($"Record with Id {Id} doesn't exist or is Inactive");

            return resultSet.Entities[0];
        }



        public async Task<bool> Delete(Guid Id)
        {
            await dynamics.DeleteAsync(EntityLogicalName, Id);
            return true;
        }

        public async Task<long> Count(QueryExpression query)
        {
            return (await dynamics.RetrieveMultipleAsync(query)).Entities.Count;
        }

        //public async Task<Entity> Create(Guid Id, ColumnSet columnSet = null)
        //{
        //    return await dynamics.RetrieveAsync(EntityLogicalName, Id, columnSet ?? new ColumnSet(true));
        //}

        public async Task<Entity> Update<T>(Guid Id, T model) where T : Model
        {
            if (Id == null || Guid.Empty.Equals(Id))
            {
                if (model.id == null || Guid.Empty.Equals(model.id))
                    throw new Exceptions.BadRequestException("Update - Unprocessable Entity, no ID");
                Id = model.id;
            }
            Entity existingRecord = await GetById(Id);

            if (existingRecord == null)
                throw new Exceptions.BadRequestException($"Update - Record with Id {Id} doesn't exist");

            Entity updateEntity = new Entity(existingRecord.LogicalName, Id);
            updateEntity = model.To(updateEntity);

            //entity = entity != null ? model.To(entity) : throw new Exceptions.BadRequestException($"Update - Record with Id {Id} doesn't exist");

            if (updateEntity.Attributes.Keys.Count > 0)
                await dynamics.UpdateAsync(updateEntity);

            return updateEntity;
        }

        public async Task<Entity> Create<T>(T model) where T : Model
        {
            Entity entity = new Entity(EntityLogicalName);
            entity = model.To(entity);
            entity.Id = await dynamics.CreateAsync(entity);
            return entity;
        }

        //public async Task<T> Create<T>(T model) where T : Model
        //{
        //    Entity entity = new Entity(EntityLogicalName);
        //    entity = model.To(entity);
        //    entity.Id = await dynamics.CreateAsync(entity);
        //    return (T)Activator.CreateInstance(typeof(T), entity);
        //}


        public static List<ExecuteMultipleResponse> BulkUpsert(DataCollection<Entity> entities)
        {

            List<ExecuteMultipleResponse> responses = new List<ExecuteMultipleResponse>();

            foreach (var chunk in Utils.Common.ChunkBy(entities.ToList(), 900))
            {

                // Create an ExecuteMultipleRequest object.
                var multipleRequest = new ExecuteMultipleRequest()
                {
                    // Assign settings that define execution behavior: continue on error, return responses. 
                    Settings = new ExecuteMultipleSettings()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    },
                    // Create an empty organization request collection.
                    Requests = new OrganizationRequestCollection()
                };

                // Add a UpsertRequest for each entity to the request collection.
                foreach (var entity in chunk)
                {
                    UpsertRequest upsertRequest = new UpsertRequest { Target = entity };
                    multipleRequest.Requests.Add(upsertRequest);
                }

                // Execute all the requests in the request collection using a single web method call.
                responses.Add((ExecuteMultipleResponse)dynamics.Execute(multipleRequest));

            }

            return responses;

        }

        public static void DeactivateRecord(string entityName, Guid recordId)
        {
            var cols = new ColumnSet(new[] { "statecode", "statuscode" });

            //Check if it is Active or not
            var entity = dynamics.Retrieve(entityName, recordId, cols);

            if (entity != null && entity.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
            {
                //StateCode = 1 and StatusCode = 2 for deactivating Account or Contact
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = recordId,
                        LogicalName = entityName,
                    },
                    State = new OptionSetValue(1),
                    Status = new OptionSetValue(2)
                };
                dynamics.Execute(setStateRequest);
            }
        }

        //Activate a record
        public static void ActivateRecord(string entityName, Guid recordId)
        {
            var cols = new ColumnSet(new[] { "statecode", "statuscode" });

            //Check if it is Inactive or not
            var entity = dynamics.Retrieve(entityName, recordId, cols);

            if (entity != null && entity.GetAttributeValue<OptionSetValue>("statecode").Value == 1 && entity.Id != Guid.Empty)
            {
                //StateCode = 0 and StatusCode = 1 for activating Account or Contact
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = recordId,
                        LogicalName = entityName,
                    },
                    State = new OptionSetValue(0),
                    Status = new OptionSetValue(1)
                };
                var response = dynamics.Execute(setStateRequest);
            }
        }

        public static void SetState(Model model, int statecode, int statuscode)
        {
            var cols = new ColumnSet(new[] { "statecode", "statuscode" });

            //Check if it is Inactive or not
            var _entity = dynamics.Retrieve(model.EntityLogicalName, model.id, cols);

            if (_entity != null && _entity.Id != Guid.Empty)
            {
                //StateCode = 0 and StatusCode = 1 for activating Account or Contact
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = _entity.Id,
                        LogicalName = _entity.LogicalName,
                    },
                    State = new OptionSetValue(statecode),
                    Status = new OptionSetValue(statuscode)
                };
                var response = dynamics.Execute(setStateRequest);
            }
        }

        public static void SetState(Entity entity, int statecode, int statuscode)
        {
            var cols = new ColumnSet(new[] { "statecode", "statuscode" });

            //Check if it is Inactive or not
            var _entity = dynamics.Retrieve(entity.LogicalName, entity.Id, cols);

            if (entity != null && _entity.Id != Guid.Empty)
            {
                //StateCode = 0 and StatusCode = 1 for activating Account or Contact
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = entity.Id,
                        LogicalName = entity.LogicalName,
                    },
                    State = new OptionSetValue(statecode),
                    Status = new OptionSetValue(statuscode)
                };
                var response = dynamics.Execute(setStateRequest);
            }
        }

        public static void SetState(EntityReference entityRef, int statecode, int statuscode)
        {
            var cols = new ColumnSet(new[] { "statecode", "statuscode" });

            //Check if it is Inactive or not
            var _entity = dynamics.Retrieve(entityRef.LogicalName, entityRef.Id, cols);

            if (entityRef != null && _entity.Id != Guid.Empty)
            {
                //StateCode = 0 and StatusCode = 1 for activating Account or Contact
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = entityRef.Id,
                        LogicalName = entityRef.LogicalName,
                    },
                    State = new OptionSetValue(statecode),
                    Status = new OptionSetValue(statuscode)
                };
                var response = dynamics.Execute(setStateRequest);
            }
        }

        public static bool IsRecordActive(Entity accountOrContact, IOrganizationService service = null)
        {
            if (accountOrContact != null)
            {
                if (accountOrContact.Contains("statecode"))
                    return (accountOrContact.GetAttributeValue<OptionSetValue>("statecode").Value == 0);
                else if (service != null)
                {
                    Entity entity = service.Retrieve(accountOrContact.LogicalName, accountOrContact.Id, new ColumnSet(new[] { "statecode" }));
                    if (entity == null)
                        return true;
                    else
                        return IsRecordActive(entity);
                }
            }
            return true;
        }

        public async Task<T> GetById<T>(Guid Id, QueryExpression query = null) where T : Model
        {
            Entity entity = await GetById(Id, query) ??
                throw new Exceptions.BadRequestException($"Get By ID - Record with Id {Id} doesn't exist");

            return (T)Activator.CreateInstance(typeof(T), entity);
        }

        public async Task<T> GetById<T>(Guid Id, ColumnSet columnSet) where T : Model
        {
            QueryExpression query = new QueryExpression(EntityLogicalName);
            query.ColumnSet = columnSet ?? new ColumnSet(true);

            Entity entity = await GetById(Id, query) ??
                throw new Exceptions.BadRequestException($"Get By ID - Record with Id {Id} doesn't exist");

            return (T)Activator.CreateInstance(typeof(T), entity);
        }

        public async Task<List<T>> GetAll<T>(QueryExpression query = null) where T : Model
        {
            List<Entity> entities = await GetAll(query);

            return entities.Select(e => (T)Activator.CreateInstance(typeof(T), e)).ToList();
        }

        public async Task<EntityCollection> ExecuteFetch(string fetchXML)
        {
            return await dynamics.RetrieveMultipleAsync(new FetchExpression(fetchXML));
        }

    }
}
