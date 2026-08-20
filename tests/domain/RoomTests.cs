using ABP.Domain.Entities;
using ABP.Domain.Exceptions;
using NUnit;

namespace ABP.Tests.Domain;

[TestFixture]
public class RoomTests
{
    private Room _room = null!;
    private Service _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Service("WiFi", 300);

        List<Service> services = [_service];
        _room = new Room("A", 50, 2000, services);
    }

    [Test]
    public void GetName()
    {
        Assert.That(_room.Name, Is.EqualTo("A"));
    }

    [Test]
    public void SetName_Valid()
    {
        _room.Name = "B";
        Assert.That(_room.Name, Is.EqualTo("B"));
    }

    [Test]
    public void SetName_Invalid_Null()
    {
        Assert.Throws<DomainRulesViolationException>(() => _room.Name = null);
    }

    [Test]
    public void SetName_Invalid_Empty()
    {
        Assert.Throws<DomainRulesViolationException>(() => _room.Name = string.Empty);
    }

    [Test]
    public void SetName_Invalid_Whitespace()
    {
        Assert.Throws<DomainRulesViolationException>(() => _room.Name = " ");
    }

    [Test]
    public void GetCapacity()
    {
        Assert.That(_room.Capacity, Is.EqualTo(50));
    }

    [Test]
    public void SetCapacity_Valid()
    {
        _room.Capacity = 10;
        Assert.That(_room.Capacity, Is.EqualTo(10));
    }

    [Test]
    public void SetCapacity_Invalid()
    {
        Assert.Throws<DomainRulesViolationException>(() => _room.Capacity = 0);
    }

    [Test]
    public void GetBasePrice()
    {
        Assert.That(_room.BasePrice, Is.EqualTo(2000));
    }

    [Test]
    public void SetBasePrice_Valid()
    {
        _room.BasePrice = 10.1m;
        Assert.That(_room.BasePrice, Is.EqualTo(10.1m));
    }

    [Test]
    public void SetBasePrice_Invalid()
    {
        Assert.Throws<DomainRulesViolationException>(() => _room.BasePrice = 0);
    }

    [Test]
    public void GetServices()
    {
        Assert.That(_room.AvailableServices, Has.Count.EqualTo(1));
        Assert.That(_room.AvailableServices[0], Is.EqualTo(_service));
    }

    [Test]
    public void Equals_True_Same()
    {
        var addressCopy = _room;
        Assert.That(addressCopy, Is.EqualTo(_room));
        Assert.That(() => addressCopy == _room, Is.True);
    }

    [Test]
    public void Equals_True_Copy()
    {
        var copy = new Room(_room);
        Assert.That(copy, Is.EqualTo(_room));
        Assert.That(() => copy == _room, Is.False);
    }

    [Test]
    public void GetHashCode_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _room.GetHashCode());
    }
}