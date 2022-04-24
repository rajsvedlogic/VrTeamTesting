using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using VrTeamTesting.API.Login.Model;

namespace VrTeamTesting.API.Login.Validator
{
    public class LoginValidator : Base.Validator<LoginRequest>
    {
        public LoginValidator()
        {

            RuleFor(x => x.email).NotEmpty().EmailAddress().WithMessage("Email address is required");
            RuleFor(x => x.password).NotEmpty().MaximumLength(15).WithMessage("Password is required");
            //RuleFor(x => x.usertype).Must(userType => Constants.UserTypeOptionSet.Keys.ToArray().Contains(userType))
            //    .WithMessage("User type is required");
        }
    }
}
