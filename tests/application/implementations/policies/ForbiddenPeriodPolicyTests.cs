using ABP.Domain.Entities;
using ABP.Application.Implementations.Policies.Booking;
using NUnit;

namespace ABP.Tests.Application.Implementations.Policies;

[TestFixture]
public class ForbiddenPeriodPolicyTests
{
    private Room _room = null!;

    [SetUp]
    public void SetUp()
    {
        _room = new Room("A", 10, 100, []);
    }

    [Test]
    public void Constructor_WhenStartEqualsEnd_ThrowsArgumentException()
    {
        Assert.That(
            () => new ForbiddenPeriodPolicy(
                new TimeOnly(10, 0),
                new TimeOnly(10, 0)),
            Throws.ArgumentException);
    }

    [Test]
    public void Constructor_WhenStartIsAfterEnd_Sucseeds()
    {
        Assert.DoesNotThrow(() => new ForbiddenPeriodPolicy(
                new TimeOnly(12, 0),
                new TimeOnly(10, 0)));
    }

    [Test]
    public void Constructor_WhenStartIsBeforeEnd_Sucseeds()
    {
        Assert.DoesNotThrow(() => new ForbiddenPeriodPolicy(
                new TimeOnly(10, 0),
                new TimeOnly(12, 0)));
    }

    [Test]
    public void IsAllowed_BookingBeforeForbiddenPeriod_ReturnsTrue()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 8, 0, 0),
            new DateTime(2026, 8, 17, 9, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.True);
    }

    [Test]
    public void IsAllowed_BookingAfterForbiddenPeriod_ReturnsTrue()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 13, 0, 0),
            new DateTime(2026, 8, 17, 14, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.True);
    }

    [Test]
    public void IsAllowed_BookingCompletelyInsideForbiddenPeriod_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 10, 30, 0),
            new DateTime(2026, 8, 17, 11, 30, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.False);
    }

    [Test]
    public void IsAllowed_BookingStartsBeforeForbiddenPeriodAndEndsInside_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 9, 0, 0),
            new DateTime(2026, 8, 17, 11, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.False);
    }

    [Test]
    public void IsAllowed_BookingStartsInsideForbiddenPeriodAndEndsAfter_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 11, 0, 0),
            new DateTime(2026, 8, 17, 13, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.False);
    }

    [Test]
    public void IsAllowed_BookingSpansEntireForbiddenPeriod_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 9, 0, 0),
            new DateTime(2026, 8, 17, 13, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.False);
    }

    [Test]
    public void IsAllowed_BookingEndsExactlyAtForbiddenStart_ReturnsTrue()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 8, 0, 0),
            new DateTime(2026, 8, 17, 10, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.True);
    }

    [Test]
    public void IsAllowed_BookingStartsExactlyAtForbiddenEnd_ReturnsTrue()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 12, 0, 0),
            new DateTime(2026, 8, 17, 13, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.True);
    }

    [Test]
    public void IsAllowed_BookingCrossesMidnight_WhenOutsideForbiddenPeriod_ReturnsTrue()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 13, 0, 0),
            new DateTime(2026, 8, 18, 9, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.True);
    }

    [Test]
    public void IsAllowed_BookingCrossesMidnightAndStartsBeforeForbiddenPeriod_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 9, 0, 0),
            new DateTime(2026, 8, 18, 11, 0, 0),
            []);
        var result = policy.IsAllowed(booking);
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsAllowed_BookingCrossesMidnightAndEndsAfterForbiddenPeriod_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 13, 0, 0),
            new DateTime(2026, 8, 18, 11, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.False);
    }

    [Test]
    public void IsAllowed_BookingLongerThanOneDay_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(10, 0),
            new TimeOnly(12, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 13, 0, 0),
            new DateTime(2026, 8, 19, 9, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.False);
    }

    [Test]
    public void IsAllowed_BookingAndForbiddenPeriodCrossMidnight_ReturnsFalse()
    {
        var policy = new ForbiddenPeriodPolicy(
            new TimeOnly(23, 0),
            new TimeOnly(6, 0));

        var booking = new Booking(
            _room,
            new DateTime(2026, 8, 17, 22, 0, 0),
            new DateTime(2026, 8, 18, 11, 0, 0),
            []);

        Assert.That(policy.IsAllowed(booking), Is.False);
    }
}