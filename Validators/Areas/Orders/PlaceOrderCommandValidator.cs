using _200SXContact.Commands.Areas.Orders;
using FluentValidation;

namespace _200SXContact.Validators.Areas.Orders
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(x => x.Model).
                NotNull().WithMessage("Order data is required.");

            RuleFor(x => x.Model.FullName)
                .NotEmpty().WithMessage("Full Name is required.");

            RuleFor(x => x.Model.AddressLine1)
                .NotEmpty().WithMessage("Address Line 1 is required.");

            RuleFor(x => x.Model.City)
                .NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.Model.County)
                .NotEmpty().WithMessage("County is required.");

            RuleFor(x => x.Model.PhoneNumber)
                .NotEmpty().WithMessage("Phone Number is required.")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format.");

            RuleFor(x => x.Model.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
