using System.Dynamic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using ABP.Domain.Entities;
using ABP.Domain.Exceptions;
using NUnit;

namespace ABP.Tests.Domain;

[TestFixture]
public class BookingTests { 
    Service service;
    Room room;
    Room anotherRoom;
    Booking booking;

    [SetUp]
    public void SetUp() {
        service = new Service("WiFi", 300);
        room = new Room("A", 50, 2000, [ service ]);
        anotherRoom = new Room("B", 100, 3000, []);

        booking = new Booking(room, new DateTime(2000, 1, 1, 8, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), [ service ]);
    }

    [Test]
    public void GetRoom() {
        Assert.That(booking.Room, Is.EqualTo(room));
    }

    [Test]
    public void SetRoom_Valid() {
        var room = new Room("B", 100, 3000, []);
        booking.Room = room;
        Assert.That(booking.Room, Is.EqualTo(room));
    }

    [Test]
    public void SetRoom_Invalid() {
        Assert.Throws<DomainRulesViolationException>(() => booking.Room = null);    
    }

    [Test]
    public void GetStartTime() {
        Assert.That(booking.StartTime, Is.EqualTo(new DateTime(2000, 1, 1, 8, 0, 0)));
    }

    [Test]
    public void SetStartTime_Valid() { 
        booking.StartTime = new DateTime(2000, 1, 1, 9, 0, 0);
        Assert.That(booking.StartTime, Is.EqualTo(new DateTime(2000, 1, 1, 9, 0, 0)));
    }

    [Test]
    public void SetStartTime_Invalid() { 
        Assert.Throws<DomainRulesViolationException>(() => booking.StartTime = new DateTime(2000, 1, 1, 13, 0, 0));
    }

    [Test]
    public void GetEndTime() {
        Assert.That(booking.EndTime, Is.EqualTo(new DateTime(2000, 1, 1, 12, 0, 0)));
    }

    [Test]
    public void SetEndTime_Valid() { 
        booking.EndTime = new DateTime(2000, 1, 1, 13, 0, 0);
        Assert.That(booking.EndTime, Is.EqualTo(new DateTime(2000, 1, 1, 13, 0, 0)));
    }

    [Test]
    public void SetEndTime_Invalid() { 
        Assert.Throws<DomainRulesViolationException>(() => booking.EndTime = new DateTime(2000, 1, 1, 7, 0, 0));
    }

    [Test]
    public void GetRequestedServices() {
        Assert.That(booking.RequestedServices, Has.Count.EqualTo(1));
        Assert.That(booking.RequestedServices[0], Is.EqualTo(room.AvailableServices[0]));
    }

    [Test]
    public void Equals_True_Same() {
        var addressCopy = booking;
        Assert.That(addressCopy, Is.EqualTo(booking));
        Assert.That(() => addressCopy == booking, Is.True);
    }

    [Test]
    public void Equals_True_Copy() {
        var copy = new Booking(booking);
        Assert.That(copy, Is.EqualTo(booking));
        Assert.That(() => copy == booking, Is.False);
    }

    // |------A------|
    // |------B------|
    //
    // ------------------------------> time

    [Test]
    public void Overlaps_False_DifferentRooms() {
        var anotherBooking = new Booking(anotherRoom, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.False);
    }

    // |------A------|
    //               |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_False_Another_IsLater() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 12, 0, 0), new DateTime(2000, 1, 1, 13, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.False);
    } 

    //               |------B------|
    // |------A------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_False_Another_IsEarlier() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 3, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.False);
    }

    //           |------A------|
    // |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromLeft() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 10, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    // |------B------|
    //           |------A------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromRight() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 10, 0, 0), new DateTime(2000, 1, 1, 14, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    // |----------B----------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromBothSides() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 14, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //        |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsInMiddle() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 10, 0, 0), new DateTime(2000, 1, 1, 11, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsExactly() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //     |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsLeftSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 8, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //           |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsRightSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 6, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |---A---|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_This_OverlapsLeftSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 16, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //           |---A---|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_This_OverlapsRightSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0), []);
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    [Test]
    public void GetHashCode_DoesNotThrow() {
        Assert.DoesNotThrow(() => booking.GetHashCode());
    }
}