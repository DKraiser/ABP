using ABP.Domain.Entities;
using ABP.Application.Implementations.Policies.Pricing.HoursPolicy;
using NUnit;

namespace ABP.Tests.Application.Implementations;

[TestFixture]
public class HoursPricePolicyTests
{
    private HoursPricePolicy _calculator = null!;
    private Room _room = null!;

    [SetUp]
    public void SetUp()
    {
        _calculator = new HoursPricePolicy([
            new (new (6, 0), new (9, 0), 0.9m),
            new (new (9, 0), new (12, 0), 1.0m),
            new (new (12, 0), new (14, 0), 1.15m),
            new (new (14, 0), new (18, 0), 1.0m),
            new (new (18, 0), new (23, 0), 0.8m)
        ]);

        _room = new Room("A", 10, 100, []);
    }

    [TestCase(6, 9, 0.9)]
    [TestCase(9, 12, 1.0)]
    [TestCase(12, 14, 1.15)]
    [TestCase(14, 18, 1.0)]
    [TestCase(18, 23, 0.8)]
    public void Calculate_UsesCorrectMultiplier(
        int startHour,
        int endHour,
        decimal multiplier)
    {
        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, startHour, 0, 0),
            new DateTime(2026, 8, 17, endHour, 0, 0),
            []
        );

        var result = _calculator.CalculatePrice(booking);

        var expected = (endHour - startHour) * 100m * multiplier;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Calculate_BookingCrossingMultiplePeriods_CalculatesEachPeriodSeparately()
    {
        // 08:00 - 15:00
        //
        // 08-09: 1 * 100 * 0.9  = 90
        // 09-12: 3 * 100 * 1.0  = 300
        // 12-14: 2 * 100 * 1.15 = 230
        // 14-15: 1 * 100 * 1.0  = 100
        //
        // Total = 720

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 8, 0, 0),
            new DateTime(2026, 8, 17, 15, 0, 0),
            []
        );

        var result = _calculator.CalculatePrice(booking);

        Assert.That(result, Is.EqualTo(720m));
    }

    [Test]
    public void Calculate_BookingWithRequestedServices_AddsServicePrices()
    {
        var service1 = new Service("Coffee", 20m);
        var service2 = new Service("Projector", 50m);

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 9, 0, 0),
            new DateTime(2026, 8, 17, 11, 0, 0),
            []
        );

        booking.RequestedServices.Add(service1);
        booking.RequestedServices.Add(service2);

        var result = _calculator.CalculatePrice(booking);

        // Room: 2 * 100 = 200
        // Services: 20 + 50 = 70
        // Total: 270

        Assert.That(result, Is.EqualTo(270m));
    }

    [Test]
    public void Calculate_BookingOutsidePricingPeriods_DoesNotChargeOutsideHours()
    {
        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 5, 0, 0),
            new DateTime(2026, 8, 17, 7, 0, 0),
            []
        );

        var result = _calculator.CalculatePrice(booking);

        // Only 06:00 - 07:00 is within a pricing period.
        // 1 * 100 * 0.9 = 90

        Assert.That(result, Is.EqualTo(90m));
    }

    [Test]
    public void Calculate_HalfHourBooking_ChargesProportionally()
    {
        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 8, 30, 0),
            new DateTime(2026, 8, 17, 9, 0, 0),
            []
        );

        var result = _calculator.CalculatePrice(booking);

        // 0.5 * 100 * 0.9 = 45

        Assert.That(result, Is.EqualTo(45m));
    }
}