using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VrTeamTesting.Common.Models;

namespace VrTeamTesting.Common.Services
{
    public class AccountService : Base.Service
    {
        public static readonly string EntityLogicalName = "account";

        public AccountService() : base(EntityLogicalName) { }

        public async Task<bool> Create(AccountModel request)
        {
            var result = await base.Create(request);
            return true;
        }

        public async Task<bool> Update(Guid Id, AccountModel request)
        {
            var result = await base.Update<AccountModel>(Id, request);
            return true;
        }

        public async Task<AccountModel> GetById(System.Guid Id)
        {
            var result = await base.GetById<AccountModel>(Id);
            return result;
        }

        public async Task<AccountModel?> GetByStripeId(string stripeID)
        {
            QueryExpression query = new QueryExpression(EntityLogicalName);
            query.Criteria.AddCondition("advic_stripeid", ConditionOperator.Equal, stripeID);

            var result = await base.GetAll<AccountModel>(query);

            if (result.Count == 0)
                return null;
            return result[0];
        }


        public async Task<List<AccountModel>> GetAll()
        {
            var result = await base.GetAll<AccountModel>();
            return result;
        }

    }
}
