using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace ReliefManagementSystem.Application.Features.User
{
    public class UpdateUserProfileValidator
        : AbstractValidator<UpdateUserProfileRequest>
    {
        public UpdateUserProfileValidator()
        {
            RuleFor(x => x.DisplayName)
                .MaximumLength(100);

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(0|\+84)[0-9]{9}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid Vietnamese phone number");

            RuleFor(x => x.Address)
                .MaximumLength(255);

            RuleFor(x => x.Gender)
                .Must(g => g == "Male" || g == "Female" || g == "Other")
                .When(x => !string.IsNullOrEmpty(x.Gender))
                .WithMessage("Gender must be Male, Female or Other");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow)
                .When(x => x.DateOfBirth.HasValue)
                .WithMessage("Date of birth must be in the past");
        }
    }
}
