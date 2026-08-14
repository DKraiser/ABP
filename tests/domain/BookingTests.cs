using System.Dynamic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using ABP.Domain.Entities;
using NUnit;

namespace ABP.Tests.Domain;

[TestFixture]
public class BookingTests { 
    Room room;
    Room anotherRoom;
    Booking booking;

    [SetUp]
    public void SetUp() {
        room = new Room("A", 50, 2000, []);
        anotherRoom = new Room("B", 100, 3000, []);

        booking = new Booking(room, new DateTime(2000, 1, 1, 8, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0));
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
        Assert.That(() => booking.Room = null, Throws.ArgumentException);    
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
        Assert.That(() => booking.StartTime = new DateTime(2000, 1, 1, 13, 0, 0), Throws.ArgumentException);
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
        Assert.That(() => booking.EndTime = new DateTime(2000, 1, 1, 7, 0, 0), Throws.ArgumentException);
    }

    // |------A------|
    // |------B------|
    //
    // ------------------------------> time

    [Test]
    public void Overlaps_False_DifferentRooms() {
        var anotherBooking = new Booking(anotherRoom, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.False);
    }

    // |------A------|
    //               |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_False_Another_IsLater() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 12, 0, 0), new DateTime(2000, 1, 1, 13, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.False);
    } 

    //               |------B------|
    // |------A------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_False_Another_IsEarlier() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 3, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.False);
    }

    //           |------A------|
    // |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromLeft() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 10, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    // |------B------|
    //           |------A------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromRight() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 10, 0, 0), new DateTime(2000, 1, 1, 14, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    // |----------B----------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsFromBothSides() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 14, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //        |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsInMiddle() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 10, 0, 0), new DateTime(2000, 1, 1, 11, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsExactly() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //     |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsLeftSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 8, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |------A------|
    //           |---B---|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_Another_OverlapsRightSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 6, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //     |---A---|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_This_OverlapsLeftSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 3, 0, 0), new DateTime(2000, 1, 1, 16, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }

    //           |---A---|
    //     |------B------|
    //
    // ------------------------------> time
    [Test]
    public void Overlaps_True_This_OverlapsRightSegment() {
        var anotherBooking = new Booking(room, new DateTime(2000, 1, 1, 1, 0, 0), new DateTime(2000, 1, 1, 12, 0, 0));
        Assert.That(booking.Overlaps(anotherBooking), Is.True);
    }
}