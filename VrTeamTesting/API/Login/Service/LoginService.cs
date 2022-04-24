using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VrTeamTesting.API.Login.Model;
using VrTeamTesting.Common.Models;
using VrTeamTesting.Common.Services;

namespace VrTeamTesting.API.Login.Service
{
    public class LoginService : Base.Service
    {
        public LoginService() : base() { }

        public async Task<Session> Login(LoginRequest credentials, string IPAddress)
        {
            QueryExpression query = new QueryExpression()
            {
                Distinct = false,
                EntityName = "contact",
                ColumnSet = new ColumnSet("firstname", "lastname", "emailaddress1"),
            };

            query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, credentials.email);

            //var passwordHash = Utils.Common.ComputeSha256Hash(credentials.password);
            var passwordHash = credentials.password;

            query.Criteria.AddCondition("az_password", ConditionOperator.Equal, passwordHash);

            //query.LinkEntities.Add(new LinkEntity("contact", "account", "advic_company", "accountid", JoinOperator.LeftOuter)
            //{
            //    EntityAlias = "company",
            //    Columns = new ColumnSet("name", "statecode", "statuscode", "advic_activeregistrationstep", "advic_isindividual"),
            //});

            var resultSet = await dynamics.RetrieveMultipleAsync(query);

            if (resultSet.Entities.Count == 0)
            {
                throw new Exceptions.BadRequestException("Invalid username or password");
            }

            var contact = new ContactModel(resultSet.Entities[0]);



            var session = new Session()
            {
                //usertype = credentials.usertype,
                email = contact.email,
                contact_id = contact.id,
                firstname = contact.firstname,
                lastname = contact.lastname
            };

            session.token = Utils.AuthUtils.CreateToken(session);

            UpdateLoginMetadata(session.contact_id, IPAddress);

            return session;

        }

        public async Task<Session> Status(string jwtToken)
        {
            var session = Utils.AuthUtils.ValidateJWTUser(jwtToken);

            AccountService service = new AccountService();
            QueryExpression query = new QueryExpression("account");
            query.ColumnSet = new ColumnSet("name");
            //query.Criteria.AddCondition("accountid", ConditionOperator.Equal, session.company_id);
            var resultSet = await service.GetAll<AccountModel>(query);
            AccountModel account = resultSet[0];

            if (resultSet.Count == 0)
                throw new Exceptions.BadRequestException("Invalid Session");

            session.companyname = account.name;
            //session.isindividual = account.isindividual;


            return session;
        }

        private async void UpdateLoginMetadata(Guid contactId, string IPAddress)
        {
            var contact = new Entity("contact", contactId);
            contact["chps_lastloggedon"] = DateTime.Now;
            contact["chps_lastloginipaddress"] = IPAddress;

            await dynamics.UpdateAsync(contact);
        }
    }
}
