using System;
using System.Collections.Generic;
using System.Text;
using VrTeamTesting.Core;

namespace VrTeamTesting.Base
{
    public abstract class Controller
    {
        protected readonly Bootstrap Function;
        public Controller(Bootstrap wrapper)
        {
            Function = wrapper;
        }
    }
}
