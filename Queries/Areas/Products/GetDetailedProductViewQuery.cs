using _200SXContact.Models.DTOs.Areas.Products;
using MediatR;

namespace _200SXContact.Queries.Areas.Products
{
	public class GetDetailedProductViewQuery : IRequest<ProductDto>
	{
		public int Id { get; set; }
		public GetDetailedProductViewQuery(int id)
		{
			Id = id;
		}
	}
}
