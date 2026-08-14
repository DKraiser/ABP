using ABP.Domain.Entities;
using NUnit;

namespace ABP.Tests.Domain;

[TestFixture]
public class RoomTests { 
    Room room;
    Service service;

    [SetUp]
    public void SetUp() { 
        service = new Service("WiFi", 300);

        List<Service> services = [ service ]; 
        room = new Room("A", 50, 2000, services);
    }   

    [Test]
    public void GetName() {
        Assert.That(room.Name, Is.EqualTo("A"));
    }

    [Test]
    public void SetName_Valid() { 
        room.Name = "B";
        Assert.That(room.Name, Is.EqualTo("B"));
    }

    [Test]
    public void SetName_Invalid_Null() { 
        Assert.That(() => room.Name = null, Throws.ArgumentException);
    }

    [Test]
    public void SetName_Invalid_Empty() { 
        Assert.That(() => room.Name = string.Empty, Throws.ArgumentException);
    }

    [Test]
    public void SetName_Invalid_Whitespace() { 
        Assert.That(() => room.Name = " ", Throws.ArgumentException);
    }

    [Test]
    public void GetCapacity() {
        Assert.That(room.Capacity, Is.EqualTo(50));
    }

    [Test]
    public void SetCapacity_Valid() {
        room.Capacity = 10;
        Assert.That(room.Capacity, Is.EqualTo(10));
    }

    [Test]
    public void SetCapacity_Invalid() {
        Assert.That(() => room.Capacity = 0, Throws.ArgumentException);
    }

    [Test]
    public void GetBasePrice() {
        Assert.That(room.BasePrice, Is.EqualTo(2000));
    }

    [Test]
    public void SetBasePrice_Valid() {
        room.BasePrice = 10.1m;
        Assert.That(room.BasePrice, Is.EqualTo(10.1m));
    }

    [Test]
    public void SetBasePrice_Invalid() {
        Assert.That(() => room.BasePrice = 0, Throws.ArgumentException);
    }

    [Test]
    public void GetServices() {
        Assert.That(room.Services, Has.Count.EqualTo(1));
        Assert.That(room.Services[0], Is.EqualTo(service));
    }
}