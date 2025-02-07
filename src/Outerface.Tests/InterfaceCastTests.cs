using NUnit.Framework;

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable ConvertTypeCheckToNullCheck
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type

namespace Outerface.Tests;

[TestFixture]
public static class InterfaceCastTests
{
    private class UnsealedClass;

    private sealed class SealedClass;

    private class ImplementedUnsealedClass : IEmptyInterface;

    private sealed class ImplementedSealedClass : IEmptyInterface;

    private interface IEmptyInterface;

    [Test]
    public static void TestCastsWithoutInjection()
    {
        Assert.Multiple(
            () =>
            {
                Assert.That(new UnsealedClass() as IEmptyInterface,       Is.Null);
                Assert.That((object)new SealedClass() as IEmptyInterface, Is.Null);

                Assert.That(new ImplementedUnsealedClass() as IEmptyInterface, Is.Not.Null);
                Assert.That(new ImplementedSealedClass() as IEmptyInterface,   Is.Not.Null);

                Assert.That<IEmptyInterface>(new ImplementedUnsealedClass(), Is.Not.Null);
                Assert.That<IEmptyInterface>(new ImplementedSealedClass(),   Is.Not.Null);

                Assert.Throws<InvalidCastException>(static () => _ = (IEmptyInterface)new UnsealedClass());
                Assert.Throws<InvalidCastException>(static () => _ = (IEmptyInterface)(object)new SealedClass());

                Assert.DoesNotThrow(() => _ = (IEmptyInterface)new ImplementedUnsealedClass());
                Assert.DoesNotThrow(() => _ = (IEmptyInterface)new ImplementedSealedClass());

                Assert.That(new UnsealedClass() is IEmptyInterface, Is.False);
                Assert.That(new SealedClass() is IEmptyInterface,   Is.False);

                Assert.That(new ImplementedUnsealedClass() is IEmptyInterface, Is.True);
                Assert.That(new ImplementedSealedClass() is IEmptyInterface,   Is.True);
            }
        );
    }

    [Test]
    public static void TestCastsWithInjection()
    {
        Assert.Multiple(
            () =>
            {
                Assert.That(new UnsealedClass() as IEmptyInterface,       Is.Null);
                Assert.That((object)new SealedClass() as IEmptyInterface, Is.Null);

                Assert.That(new ImplementedUnsealedClass() as IEmptyInterface, Is.Not.Null);
                Assert.That(new ImplementedSealedClass() as IEmptyInterface,   Is.Not.Null);

                Assert.That<IEmptyInterface>(new ImplementedUnsealedClass(), Is.Not.Null);
                Assert.That<IEmptyInterface>(new ImplementedSealedClass(),   Is.Not.Null);

                Assert.Throws<InvalidCastException>(static () => _ = (IEmptyInterface)new UnsealedClass());
                Assert.Throws<InvalidCastException>(static () => _ = (IEmptyInterface)(object)new SealedClass());

                Assert.DoesNotThrow(() => _ = (IEmptyInterface)new ImplementedUnsealedClass());
                Assert.DoesNotThrow(() => _ = (IEmptyInterface)new ImplementedSealedClass());
            }
        );

        InterfaceInjector.InjectInterfaces(typeof(UnsealedClass), InterfaceIdentity.CreateEmpty(typeof(IEmptyInterface)));
        InterfaceInjector.InjectInterfaces(typeof(SealedClass),   InterfaceIdentity.CreateEmpty(typeof(IEmptyInterface)));

        Assert.Multiple(
            () =>
            {
                Assert.That(new UnsealedClass() as IEmptyInterface,       Is.Not.Null);
                Assert.That((object)new SealedClass() as IEmptyInterface, Is.Not.Null);

                Assert.That(new ImplementedUnsealedClass() as IEmptyInterface, Is.Not.Null);
                Assert.That(new ImplementedSealedClass() as IEmptyInterface,   Is.Not.Null);

                Assert.That<IEmptyInterface>(new ImplementedUnsealedClass(), Is.Not.Null);
                Assert.That<IEmptyInterface>(new ImplementedSealedClass(),   Is.Not.Null);

                Assert.DoesNotThrow(static () => _ = (IEmptyInterface)new UnsealedClass());
                Assert.DoesNotThrow(static () => _ = (IEmptyInterface)(object)new SealedClass());

                Assert.DoesNotThrow(() => _ = (IEmptyInterface)new ImplementedUnsealedClass());
                Assert.DoesNotThrow(() => _ = (IEmptyInterface)new ImplementedSealedClass());

                Assert.That(new UnsealedClass() is IEmptyInterface, Is.True);
                Assert.That(new SealedClass() is IEmptyInterface,   Is.False); // JIT gets to this one first...

                Assert.That(new ImplementedUnsealedClass() is IEmptyInterface, Is.True);
                Assert.That(new ImplementedSealedClass() is IEmptyInterface,   Is.True);
            }
        );
    }

    /*
     *     private class CanWePatternMatchThis_Class;

    private interface ICanWePatternMatchThis_Interface;

    [Test]
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    public static void CanWePatternMatchThis()
    {
        var beforeInjection = new CanWePatternMatchThis_Class();
        Assert.That(beforeInjection is ICanWePatternMatchThis_Interface, Is.False);

        InterfaceInjector.InjectInterfaces(typeof(CanWePatternMatchThis_Class), InterfaceIdentity.CreateEmpty(typeof(ICanWePatternMatchThis_Interface)));

        var afterInjection = new CanWePatternMatchThis_Class();
        Assert.Multiple(
            () =>
            {
                Assert.That(afterInjection is ICanWePatternMatchThis_Interface, Is.True);
                Assert.That(beforeInjection is ICanWePatternMatchThis_Interface, Is.True);
                Assert.DoesNotThrow(() => _ = (ICanWePatternMatchThis_Interface)afterInjection);
            }
        );
    }
     */
}