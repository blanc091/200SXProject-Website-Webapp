namespace _200SXContact.Interfaces.Areas.Products
{
    public interface IProduct
    {
        int Id { get; set; }
        string Name { get; set; }
        string Category { get; set; }
        string Description { get; set; }
        decimal Price { get; set; }
        List<string> ImagePaths { get; set; }
        DateTime DateAdded { get; set; }
    }
}