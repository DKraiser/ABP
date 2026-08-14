using ABP.Domain;
using NUnit;

namespace ABP.Tests.Domain;

[TestFixture]
public class ServiceTests { 
    Service service;

    [SetUp]
    public void SetUp() { 
        service = new Service("WiFi", 300);
    }   

    [Test]
    public void GetName() {
        Assert.That(service.Name, Is.EqualTo("WiFi"));
    }

    [Test]
    public void SetName_Valid() { 
        service.Name = "Internet";
        Assert.That(service.Name, Is.EqualTo("Internet"));
    }

    [Test]
    public void SetName_Invalid_Null() { 
        Assert.That(() => service.Name = null, Throws.InvalidOperationException);
    }

    [Test]
    public void SetName_Invalid_Empty() { 
        Assert.That(() => service.Name = string.Empty, Throws.InvalidOperationException);
    }

    [Test]
    public void SetName_Invalid_Whitespace() { 
        Assert.That(() => service.Name = " ", Throws.InvalidOperationException);
    }

    [Test]
    public void GetPrice() {
        Assert.That(service.Price, Is.EqualTo(300));
    }

    [Test]
    public void SetPrice_Valid() {
        service.Price = 10.1m;
        Assert.That(service.Price, Is.EqualTo(10.1m));
    }

    [Test]
    public void SetPrice_Invalid() {
        Assert.That(() => service.Price = 0, Throws.InvalidOperationException);
    }
}