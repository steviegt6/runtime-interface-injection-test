namespace Outerface.CompilerServices.Handles;

[TypeDef("src/coreclr/vm/methodtable.h", "struct InterfaceInfo_t")]
public sealed class InterfaceInfoHandle : AbstractHandle, IHandleFactory<InterfaceInfoHandle>
{
    private InterfaceInfoHandle(nint value) : base(value) { }

#region MethodTable
    /// <summary>
    ///     Method table of the interface.
    /// </summary>
    public ref nint MethodTablePtr => ref GetRef<nint>(0);

    /// <summary>
    ///     Gets the method table handle of the interface.
    /// </summary>
    public MethodTableHandle? Methodtable => GetHandle<MethodTableHandle>(0);
#endregion

#region Factory
    public static InterfaceInfoHandle Create(nint value)
    {
        return new InterfaceInfoHandle(value);
    }

    public static InterfaceInfoHandle? CreateAllowInvalid(nint value)
    {
        return value == nint.Zero ? null : new InterfaceInfoHandle(value);
    }
#endregion
}