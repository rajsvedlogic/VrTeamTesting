using System;
using System.Collections.Generic;
using System.Text;

namespace VrTeamTesting.API.Login.Model
{
    public class LoginRequest
    {
        public string email { get; set; }
        public string password { get; set; }
        //public string usertype { get; set; }
    }

    public class Session
    {
        public string email { get; set; }
        public string usertype { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public Guid contact_id { get; set; }
        //public Guid company_id { get; set; }
        public string companyname { get; set; }
        public string token { get; set; }
        //public bool isindividual { get; set; }

    }

}
