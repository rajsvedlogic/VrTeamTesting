using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace VrTeamTesting.Common.Models
{
    public class AccountModel : Base.Model
    {
        public AccountModel() : base(Services.AccountService.EntityLogicalName)
        { }

        public AccountModel(Entity entity) : base(entity)
        { }

        //public AccountModel(Entity entity, string prefix) : base(entity, prefix)
        //{ }

        public enum StatusReason
        {
            //Active
            Active = 1,

            //In-Active
            Inactive = 2,
            AwaitingManualVerification = 100000000,
            Suspended = 100000001,
            PendingEmailVerification = 100000002,
        }

        [Base.Mapping("emailaddress1")]
        public string email { get; set; }

        public string message { get; set; }

        [Base.Mapping("address1_addresstypecode")]
        public string addresstype { get; set; }

        [Base.Mapping("address1_city")]
        public string city { get; set; }

        [Base.Mapping("address1_country")]
        public string country { get; set; }

        [Base.Mapping("telephone1")]
        public string phone { get; set; }

        [Base.Mapping("address1_line1")]
        public string addressline1 { get; set; }

        [Base.Mapping("address1_line2")]
        public string addressline2 { get; set; }

        [Base.Mapping("address1_line3")]
        public string addressline3 { get; set; }

        [Base.Mapping("address1_postalcode")]
        public string postalcode { get; set; }

        [Base.Mapping("primarycontactid")]
        public ContactModel primarycontact { get; set; }

        [Base.Mapping("statuscode")]
        public StatusReason status { get; set; }
    }

}
