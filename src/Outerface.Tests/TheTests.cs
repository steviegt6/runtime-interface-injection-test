namespace Outerface.Tests;

[TestFixture]
public static class TheTests
{
#region Sanity
    private interface IConfirmInterfaceCountsForUnmodifiedTypes_One;

    private interface IConfirmInterfaceCountsForUnmodifiedTypes_Two;

    private sealed class ConfirmInterfaceCountsForUnmodifiedTypes_Zero;

    private sealed class ConfirmInterfaceCountsForUnmodifiedTypes_One : IConfirmInterfaceCountsForUnmodifiedTypes_One;

    private sealed class ConfirmInterfaceCountsForUnmodifiedTypes_Two : IConfirmInterfaceCountsForUnmodifiedTypes_One, IConfirmInterfaceCountsForUnmodifiedTypes_Two;

    [Test]
    public static void ConfirmInterfaceCountsForUnmodifiedTypes()
    {
        var methodTableZero = InterfaceInjector.GetMethodTable(typeof(ConfirmInterfaceCountsForUnmodifiedTypes_Zero));
        Assert.That(methodTableZero.InterfaceCount, Is.EqualTo(0));

        var methodTableOne = InterfaceInjector.GetMethodTable(typeof(ConfirmInterfaceCountsForUnmodifiedTypes_One));
        Assert.That(methodTableOne.InterfaceCount, Is.EqualTo(1));

        var methodTableTwo = InterfaceInjector.GetMethodTable(typeof(ConfirmInterfaceCountsForUnmodifiedTypes_Two));
        Assert.That(methodTableTwo.InterfaceCount, Is.EqualTo(2));
    }

    private sealed class CanWePatternMatchThis_Class;

    private interface ICanWePatternMatchThis_Interface;

    [Test]
    public static void CanWePatternMatchThis()
    {
        var a = new CanWePatternMatchThis_Class();
        Assert.That(a is ICanWePatternMatchThis_Interface, Is.False);
        
        // InterfaceInjector.InjectInterface(typeof(CanWePatternMatchThis_Class), new InterfaceIdentity(typeof(ICanWePatternMatchThis_Interface), null));
        var b = new CanWePatternMatchThis_Class();
        // Assert.That(b is ICanWePatternMatchThis_Interface, Is.True);
        try
        {
            var c = (ICanWePatternMatchThis_Interface)(object)b;
        }
        catch { }
    }
#endregion
}