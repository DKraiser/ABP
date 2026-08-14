namespace ABP.Domain;

public class Service { 
    private string _id;
    private string _name;
    private decimal _price;

    public string Id => _id;
    public string Name { 
        get => _name;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Name must be not empty.");
            _name = value;
        } 
    }
    public decimal Price {
        get => _price;
        set {
            if (value <= 0)
                throw new InvalidOperationException("Price must be a positive number.");
            _price = value;
        } 
    }

    public Service (string name, decimal price) {
        _id = new Guid().ToString();
        Name = name;
        Price = price;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        return Id == (obj as Service)?.Id && Name == (obj as Service)?.Name && Price == (obj as Service)?.Price;
    }
}