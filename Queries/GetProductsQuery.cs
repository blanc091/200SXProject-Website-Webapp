using _200SXContact.Models;
using MediatR;

namespace _200SXContact.Queries
{
	public class GetProductsQuery : IRequest<List<Product>>
	{
	}
}
