namespace _200SXContact.Interfaces.Areas.Products
{
    public interface IProductDto
    {
        int Id { get; set; }
        string Name { get; set; }
        string Category { get; set; }
        string Description { get; set; }
        decimal Price { get; set; }
        string ImagePaths { get; set; }
        string AddedDate { get; set; }
    }
}