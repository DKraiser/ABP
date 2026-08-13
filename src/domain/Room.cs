using System.Text.Unicode;

namespace ABP.Domain;

public class Room { 
    private string _name;
    private int _capacity;
    private decimal _basePrice;
    private readonly List<Service> _services = [];

    public string Name { 
        get => _name;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Name must be not empty.");
            _name = value;
        } 
    }
    public int Capacity { 
        get => _capacity; 
        set {
            if (value <= 0) 
                throw new InvalidOperationException("Capacity must be a positive number.");
            _capacity = value;
        } 
    }
    public decimal BasePrice {
        get => _basePrice;
        set {
            if (value <= 0)
                throw new InvalidOperationException("Base price must be a positive number.");
            _basePrice = value;
        } 
    }
    public List<Service> Services => _services;

    public Room (string name, int capacity, decimal basePrice, List<Service> services) {
        Name = name;
        Capacity = capacity;
        BasePrice = basePrice;
        Services.AddRange(services);
    }
}