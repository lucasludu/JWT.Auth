using FluentValidation;
using System.Text.RegularExpressions;

namespace JWT.Auth.BlazorUI.ViewModels.Accounts
{
    public class LoginValidationVM : AbstractValidator<LoginVM>
    {
        public LoginValidationVM()
        {
            RuleFor(x => x.Email)
               .NotEmpty()
               .EmailAddress()
               .WithMessage("Invalid Email.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Invalid Credentials.")
                .MinimumLength(6).WithMessage("Invalid Credentials.")
                .MaximumLength(16).WithMessage("Invalid Credentials.")
                .Matches(@"[A-Z]+").WithMessage("Invalid Credentials.")
                .Matches(@"[a-z]+").WithMessage("Invalid Credentials.")
                .Matches(@"[0-9]+").WithMessage("Invalid Credentials.")
                .Matches(@"[\@\!\?\*\.]+").WithMessage("Invalid Credentials.");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<LoginVM>.CreateWithOptions((LoginVM)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
