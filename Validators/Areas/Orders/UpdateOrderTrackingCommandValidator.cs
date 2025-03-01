using _200SXContact.Commands.Areas.Orders;
using FluentValidation;

namespace _200SXContact.Validators.Areas.Orders
{
    public class UpdateOrderTrackingCommandValidator : AbstractValidator<UpdateOrderTrackingCommand>
    {
        public UpdateOrderTrackingCommandValidator()
        {
            RuleFor(x => x.UpdateDto.OrderId)
                .GreaterThan(0).WithMessage("Valid Order ID is required.");

            RuleFor(x => x.UpdateDto.Status)
                .NotEmpty().WithMessage("Order status is required.")
                .MaximumLength(50).WithMessage("Status cannot exceed 50 characters.");

            RuleFor(x => x.UpdateDto.Carrier)
                .MaximumLength(50).WithMessage("Carrier name cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.UpdateDto.Carrier));

            RuleFor(x => x.UpdateDto.TrackingNumber)
                .MaximumLength(100).WithMessage("Tracking number cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.UpdateDto.TrackingNumber));
        }
    }
}