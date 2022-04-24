using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VrTeamTesting.Common.Models;
using VrTeamTesting.ConnectionString;

namespace VrTeamTesting.Utils
{
    public enum RecordState
    {
        Active,
        Inactive
    }
    public class RichOptionSet
    {

        public string name { get; set; }
        public int value { get; set; }
        public string color { get; set; }

        public RichOptionSet(string name, int value, string color)
        {
            this.name = name;
            this.value = value;
            this.color = color;
        }

    }

}

namespace VrTeamTesting.Utils
{
    public static class Common
    {
        public static Random random = new Random();

        public static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string Environment(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static void CreateNote(Entity forEntity, string title, string description)
        {
            var service = ConnnectionManager.Connection.Get();

            Entity note = new Entity("annotation");

            note["objectid"] = new EntityReference(forEntity.LogicalName, forEntity.Id);
            note["notetext"] = description;
            note["isdocument"] = false;
            service.Create(note);
        }

        public static async Task<Guid> CreateDocument(Entity forEntity, FileModel file)
        {
            var service = ConnnectionManager.Connection.Get();

            Entity note = new Entity("annotation");
            var base64 = Convert.ToBase64String(file.Contents);

            note["objectid"] = new EntityReference(forEntity.LogicalName, forEntity.Id);
            note["stepid"] = file.Type ?? "API";
            note["subject"] = file.Name;
            note["filename"] = file.FileName;
            note["mimetype"] = file.ContentType;
            note["documentbody"] = base64;

            return await service.CreateAsync(note);

        }

        //public static async Task<Guid> CreateDocument(Entity forEntity, RoomImageRequest value)
        //{
        //    var service = Core.ConnnectionManager.Connection.Get();

        //    Entity note = new Entity("annotation");
        //    var base64 = value.document;

        //    note["objectid"] = new EntityReference(forEntity.LogicalName, forEntity.Id);
        //    note["stepid"] = value.description ?? "API";
        //    note["subject"] = value.name;
        //    note["filename"] = value.fileName;
        //    note["mimetype"] = value.mimeType;
        //    note["documentbody"] = base64;

        //    return await service.CreateAsync(note);

        //}

        public static string StrToDate(string dateString)
        {
            if (dateString.Length > 10)
            {
                return dateString.Substring(0, 10);
            }
            return "";
        }

        public static Entity GetUserByEmail(string emailAddress, string entityLogicalName = null)
        {

            var service = ConnnectionManager.Connection.Get();

            QueryExpression qry;
            EntityCollection resultSet;
            Entity entity = new Entity();

            switch (entityLogicalName)
            {

                case "systemuser":
                    qry = new QueryExpression(entityLogicalName);
                    qry.ColumnSet = new ColumnSet("internalemailaddress");
                    qry.Criteria.AddCondition("internalemailaddress", ConditionOperator.Equal, emailAddress);
                    resultSet = service.RetrieveMultiple(qry);
                    entity = resultSet.Entities.Count > 0 ? resultSet.Entities[0] : entity;
                    break;

                case "contact":
                    qry = new QueryExpression(entityLogicalName);
                    qry.ColumnSet = new ColumnSet("emailaddress1");
                    qry.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, emailAddress);
                    resultSet = service.RetrieveMultiple(qry);
                    entity = resultSet.Entities.Count > 0 ? resultSet.Entities[0] : entity; break;

                case "account":
                    qry = new QueryExpression(entityLogicalName);
                    qry.ColumnSet = new ColumnSet("emailaddress1");
                    qry.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, emailAddress);
                    resultSet = service.RetrieveMultiple(qry);
                    entity = resultSet.Entities.Count > 0 ? resultSet.Entities[0] : entity;
                    break;

                default:
                    break;
            }

            return entity;
        }

        public static Entity GetPortalUser(Entity company, ColumnSet? columns)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet = columns ?? new ColumnSet(true);
            query.Criteria.AddCondition(new ConditionExpression("advic_company", ConditionOperator.Equal, company.Id));
            //query.Criteria.AddCondition("advic_portaluser", ConditionOperator.Equal, true);

            var service = ConnnectionManager.Connection.Get();

            var resultSet = service.RetrieveMultiple(query);

            if (resultSet.Entities.Count == 0)
                return null;
            else
                return resultSet.Entities[0];
        }

        public static SendEmailResponse? sendEmail(Entity from, Entity[] to, EntityReference regardingEntity, string subject, string description, bool directionCode = true)
        {
            var service = ConnnectionManager.Connection.Get();

            try
            {
                Entity email = new Entity("email");

                email["from"] = new Entity[] { from };
                email["to"] = to;

                if (regardingEntity != null)
                    email["regardingobjectid"] = regardingEntity;

                email["subject"] = subject;

                email["description"] = description;
                email["directioncode"] = directionCode;

                Guid emailId = service.Create(email);

                SendEmailRequest sendEmailRequest = new SendEmailRequest()
                {

                    EmailId = emailId,
                    TrackingToken = "",
                    IssueSend = true

                };

                return (SendEmailResponse)service.Execute(sendEmailRequest);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static List<string> GetEntityAttributes(string entityLogicalName)
        {

            var service = ConnnectionManager.Connection.Get();

            RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityLogicalName
            };

            RetrieveEntityResponse retrieveAccountEntityResponse = (RetrieveEntityResponse)service.Execute(retrieveEntityRequest);
            EntityMetadata AccountEntity = retrieveAccountEntityResponse.EntityMetadata;

            return AccountEntity.Attributes.Select(attribute => attribute.LogicalName).ToList();

        }

        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}
