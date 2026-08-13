namespace ABP.Domain;

public class Service (long id, string name, decimal price) { 
    public long Id { get; init; } = id;
    public string Name { get; set; } = name;
    public decimal Price { get; set; } = price;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        return Id == (obj as Service)?.Id && Name == (obj as Service)?.Name && Price == (obj as Service)?.Price;
    }
}