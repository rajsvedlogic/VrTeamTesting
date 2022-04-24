using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VrTeamTesting.Common.Models;

namespace VrTeamTesting.Common.Services
{
    public class ContactService : Base.Service
    {
        public static readonly string EntityLogicalName = "contact";
        public ContactService() : base(EntityLogicalName) { }
        public async Task VerifyEmail(string emailAddress)
        {
            QueryExpression query = new QueryExpression("contact");
            //query.ColumnSet = new ColumnSet("advic_company");
            query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, emailAddress);

            var contacts = await GetAll<ContactModel>(query);

            if (contacts.Count == 0)
                throw new Exceptions.BadRequestException("Cannot verify, Invalid Email");
        }


        public async Task<bool> Create(ContactModel request)
        {
            var result = await base.Create(request);
            return true;
        }
        public async Task<bool> Update(Guid Id, ContactModel request)
        {
            var result = await base.Update<ContactModel>(Id, request);
            return true;
        }

        public async Task<ContactModel> GetById(System.Guid Id)
        {
            var result = await base.GetById<ContactModel>(Id);
            return result;
        }
        public async Task<ContactModel> GetPrimaryContact(System.Guid AccountId)
        {
            var account = await new AccountService().GetById<AccountModel>(AccountId);

            if (account.primarycontact == null)
                return null;

            var result = await base.GetById<ContactModel>(account.primarycontact.id);
            return result;
        }


        public async Task<List<ContactModel>> GetAll()
        {
            var result = await base.GetAll<ContactModel>();
            return result;
        }


    }
}
