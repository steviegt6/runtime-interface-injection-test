using NUnit.Framework;

namespace Outerface.Tests;

[TestFixture]
public static class InterfaceCountsTests
{
    private sealed class Zero;

    private sealed class One : IOne;

    private sealed class Two : IOne, ITwo;

    private interface IOne;

    private interface ITwo;

    [Test]
    public static void UnmodifiedClasses()
    {
        var methodTableZero = InterfaceInjector.GetMethodTable(typeof(Zero));
        var methodTableOne  = InterfaceInjector.GetMethodTable(typeof(One));
        var methodTableTwo  = InterfaceInjector.GetMethodTable(typeof(Two));

        Assert.Multiple(
            () =>
            {
                Assert.That(methodTableZero.InterfaceCount, Is.EqualTo(0));
                Assert.That(methodTableOne.InterfaceCount,  Is.EqualTo(1));
                Assert.That(methodTableTwo.InterfaceCount,  Is.EqualTo(2));
            }
        );
    }

    [Test]
    public static void ModifiedClasses()
    {
        var methodTableZero = InterfaceInjector.GetMethodTable(typeof(Zero));
        var methodTableOne  = InterfaceInjector.GetMethodTable(typeof(One));
        var methodTableTwo  = InterfaceInjector.GetMethodTable(typeof(Two));

        Assert.Multiple(
            () =>
            {
                Assert.That(methodTableZero.InterfaceCount, Is.EqualTo(0));
                Assert.That(methodTableOne.InterfaceCount,  Is.EqualTo(1));
                Assert.That(methodTableTwo.InterfaceCount,  Is.EqualTo(2));
            }
        );

        InterfaceInjector.InjectInterfaces(typeof(Zero), InterfaceIdentity.CreateEmpty(typeof(IOne)));
        InterfaceInjector.InjectInterfaces(typeof(One),  InterfaceIdentity.CreateEmpty(typeof(ITwo)));

        Assert.Multiple(
            () =>
            {
                Assert.That(methodTableZero.InterfaceCount, Is.EqualTo(1));
                Assert.That(methodTableOne.InterfaceCount,  Is.EqualTo(2));
                Assert.That(methodTableTwo.InterfaceCount,  Is.EqualTo(2));
            }
        );
    }
}