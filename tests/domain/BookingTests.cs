using System.Dynamic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using ABP.Domain.Entities;
using ABP.Domain.Exceptions;
using NUnit;

namespace ABP.Tests.Domain;

[TestFixture]
public class BookingTests { 
    private Service _service = null!;
    private Room _room = null!;
    private Room _anotherRoom = null!;
    private Booking _booking = null!;

    [SetUp]
    public void SetUp() {
        _service = new Service("WiFi", 300);
        _room = new Room("A", 50, 2000, [ _service ]);
        _anotherRoom = new Room("B", 100, 3000, []);

        _booking = new Booking(_room, new DateTime(2000, 1, 1, 8, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), [ _service ]);
    }

    [Test]
    public void GetRoom() {
        Assert.That(_booking.Room, Is.EqualTo(_room));
    }

    [Test]
    public void SetRoom_Valid() {
        var room = new Room("B", 100, 3000, []);
        _booking.Room = room;
        Assert.That(_booking.Room, Is.EqualTo(room));
    }

    [Test]
    public void SetRoom_Invalid() {
        Assert.Throws<DomainRulesViolationException>(() => _booking.Room = null);    
    }

    [Test]
    public void GetStartTime() {
        Assert.That(_booking.StartTime, Is.EqualTo(new DateTime(2000, 1, 1, 8, 0, 0)));
    }

    [Test]
    public void SetStartTime_Valid() { 
        _booking.StartTime = new DateTime(2000, 1, 1, 9, 0, 0);
        Assert.That(_booking.StartTime, Is.EqualTo(new DateTime(2000, 1, 1, 9, 0, 0)));
    }

    [Test]
    public void SetStartTime_Invalid() { 
        Assert.Throws<DomainRulesViolationException>(() => _booking.StartTime = new DateTime(2000, 1, 1, 13, 0, 0));
    }

    [Test]
    public void GetEndTime() {
        Assert.That(_booking.EndTime, Is.EqualTo(new DateTime(2000, 1, 1, 12, 0, 0)));
    }

    [Test]
    public void SetEndTime_Valid() { 
        _booking.EndTime = new DateTime(2000, 1, 1, 13, 0, 0);
        Assert.That(_booking.EndTime, Is.EqualTo(new DateTime(2000, 1, 1, 13, 0, 0)));
    }

    [Test]
    public void SetEndTime_Invalid() { 
        Assert.Throws<DomainRulesViolationException>(() => _booking.EndTime = new DateTime(2000, 1, 1, 7, 0, 0));
    }

    [Test]
    public void SetPrice_Valid() { 
        Assert.Multiple(() => {
            Assert.DoesNotThrow(() => _booking.Price = 10);
            Assert.That(_booking.Price, Is.EqualTo(10));
        });
    }

    [Test]
    public void SetPrice_Invalid() { 
        Assert.Throws<DomainRulesViolationException>(() => _booking.Price = -10);
    }

    [Test]
    public void GetRequestedServices() {
        Assert.That(_booking.RequestedServices, Has.Count.EqualTo(1));
        Assert.That(_booking.RequestedServices[0], Is.EqualTo(_room.AvailableServices[0]));
    }

    [Test]
    public void Equals_True_Same() {
        var addressCopy = _booking;
        Assert.That(addressCopy, Is.EqualTo(_booking));
        Assert.That(() => addressCopy == _booking, Is.True);
    }

    [Test]
    public void Equals_True_Copy() {
        var copy = new Booking(_booking);
        Assert.That(copy, Is.EqualTo(_booking));
        Assert.That(() => copy == _booking, Is.False);
    }

    // |------A------|
    // |------B------|
    //
    // ------------------------------> time

    [Test]
    public void Overlaps_False_DifferentRooms() {
        var anotherBooking = new Booking(_anotherRoom, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.False);
    }

    // |------A------|
    //               |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_False_Another_IsLater() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 12, 0, 0), new DateTime(2000, 1, 1, 13, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.False);
    } 

    //               |------B------|
    // |------A------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_False_Another_IsEarlier() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 3, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.False);
    }

    //           |------A------|
    // |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromLeft() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 10, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    // |------B------|
    //           |------A------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromRight() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 10, 0, 0), new DateTime(2000, 1, 1, 14, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    // |----------B----------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromBothSides() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 14, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //        |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsInMiddle() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 10, 0, 0), new DateTime(2000, 1, 1, 11, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsExactly() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //     |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsLeftSegment() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 8, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //           |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsRightSegment() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 6, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    //     |---A---|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_This_OverlapsLeftSegment() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 16, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    //           |---A---|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_This_OverlapsRightSegment() {
        var anotherBooking = new Booking(_room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(_booking.Overlaps(anotherBooking), Is.True);
    }

    [Test]
    public void GetHashCode_DoesNotThrow() {
        Assert.DoesNotThrow(() => _booking.GetHashCode());
    }
}