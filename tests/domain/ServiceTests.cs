using ABP.Domain.Entities;
using ABP.Domain.Exceptions;
using NUnit;

namespace ABP.Tests.Domain;

[TestFixture]
public class ServiceTests { 

    [Test]
    public void Constructor_Valid() { 
        Assert.DoesNotThrow(() => new Service("WiFi", 300));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Constructor_Invalid_Name(string? name) { 
        Assert.Throws<DomainRulesViolationException>(() => new Service(name, 300));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_Invalid_Price(decimal price) { 
        Assert.Throws<DomainRulesViolationException>(() => new Service("WiFi", price));
    }

    [Test]
    public void GetHashCode_DoesNotThrow() {
        Assert.DoesNotThrow(() => new Service("WiFi", 300).GetHashCode());
    }
}