namespace MyAPI.Models
{
    public class Item
    {

        public int Id { get; set; } //автоматически добавляется EF Core

        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
