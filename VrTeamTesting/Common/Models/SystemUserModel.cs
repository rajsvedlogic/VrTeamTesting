using System;
using System.Collections.Generic;
using System.Text;

namespace VrTeamTesting.Common.Models
{
    public class SystemUserModel : Base.Model
    {
        public SystemUserModel(System.Guid id, string name) : base("systemuser")
        {
            this.id = id;
            this.name = name;
        }
    }
}
