using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace VrTeamTesting.Base
{
    public class Validator<T> : AbstractValidator<T> where T : class
    {
        //protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
        //{
        //    if (context.InstanceToValidate == null)
        //    {
        //        result.Errors.Add(new ValidationFailure("", "Model is Null"));
        //        return false;
        //    }
        //    return base.PreValidate(context, result);
        //}

        public Validator()
        {

            //RuleSet(HttpMethod.Post.Method, () =>
            //{
            //    RuleFor(x => x.Id).Empty()
            //      .WithMessage($"Id field cannot be specified when POSTing a new item '{typeof(T).Name}'");
            //});

            //RuleSet(HttpMethod.Patch.Method, () =>
            //{
            //    RuleFor(x => x.Id).NotEmpty();
            //});

        }
    }
}
