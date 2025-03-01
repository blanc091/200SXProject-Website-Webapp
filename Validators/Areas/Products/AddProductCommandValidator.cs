using _200SXContact.Commands.Areas.Products;
using FluentValidation;
using System.Text.RegularExpressions;
namespace _200SXContact.Validators.Areas.Products
{
	public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
	{
		public AddProductCommandValidator()
		{
			RuleFor(x => x.Product.Name)
			   .NotEmpty().WithMessage("Product Name is required.")
			   .MaximumLength(255).WithMessage("Product Name cannot be longer than 255 characters.");

			RuleFor(x => x.Product.Category)
				.NotEmpty().WithMessage("Category is required.")
				.MaximumLength(100).WithMessage("Category cannot be longer than 100 characters.");

			RuleFor(x => x.Product.Description)
				.NotEmpty().WithMessage("Description is required.")
				.MaximumLength(500).WithMessage("Description cannot be longer than 500 characters.");

			RuleFor(x => x.Product.Price)
				.GreaterThan(0).WithMessage("Price must be greater than zero.")
				.LessThanOrEqualTo(9999.99m).WithMessage("Price must be less than or equal to 9999.99.")
				.Must(BeValidPrice).WithMessage("Price must have no more than two decimal places.");
		}
		private bool BeValidPrice(decimal price)
		{
			string priceString = price.ToString("F2");
			Regex regex = new Regex(@"^[0-9]+(\.[0-9]{1,2})?$");

			return regex.IsMatch(priceString);
		}
	}
}
