using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace VrTeamTesting.Common.Models
{
    public class ContactModel : Base.Model
    {
        public ContactModel() : base(Services.ContactService.EntityLogicalName)
        { }
        public ContactModel(Entity entity) : base(entity)
        { }


        [Base.Mapping("fullname")]
        public new string name { get; set; }

        [Base.Mapping("salutation")]
        public string? title { get; set; }

        [Base.Mapping("emailaddress1")]
        public string email { get; set; }

        [Base.Mapping("firstname")]
        public string firstname { get; set; }

        [Base.Mapping("lastname")]
        public string lastname { get; set; }

        [Base.Mapping("jobtitle")]
        public string jobtitle { get; set; }

        [Base.Mapping("telephone1")]
        public string phonenumber { get; set; }

        [Base.Mapping("chps_lastloggedon")]
        public System.DateTime? lastloggedon { get; set; }

        [Base.Mapping("chps_lastloginipaddress")]
        public string ipaddress { get; set; }

        [Base.Mapping("az_password")]
        public string password { get; set; }

        [JsonIgnore]
        public new ContactModel createdby { get; set; }

        [JsonIgnore]
        public new DateTime createdon { get; set; }
    }
}

