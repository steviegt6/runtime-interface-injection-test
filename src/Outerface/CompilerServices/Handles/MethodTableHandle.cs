namespace Outerface.CompilerServices.Handles;

[TypeDef("src/coreclr/vm/methodtable.h", "class MethodTable")]
public sealed class MethodTableHandle : AbstractHandle, IHandleFactory<MethodTableHandle>
{
    private static int ParentMethodTableOffset => 0x10 + InternalConstants.DEBUG_CLASS_NAME_PTR;

    private static int InterfaceMapOffset => nint.Size == 8 ? 0x38 + InternalConstants.DEBUG_CLASS_NAME_PTR : 0x24 + InternalConstants.DEBUG_CLASS_NAME_PTR;

    private MethodTableHandle(nint value) : base(value) { }

#region ComponentSize
    /// <summary>
    ///     The low WORD of the first field is the component size for array and
    ///     string types.
    /// </summary>
    public ref ushort ComponentSize => ref GetRef<ushort>(0);
#endregion

#region Flags
    /// <summary>
    ///     The flags for the current method table (only for not array or string
    ///     types).
    /// </summary>
    public ref uint Flags => ref GetRef<uint>(0);
#endregion

#region BaseSize
    /// <summary>
    ///     Gets the base size of the type (used when allocating an instance on
    ///     the heap).
    /// </summary>
    public ref uint BaseSize => ref GetRef<uint>(4);
#endregion

    // See additional native members in methodtable.h, not needed here yet.
    // 0x8: m_dwFlags2 (additional flags and token in upper 24 bits)

#region VirtualCount
    public ref ushort VirtualCount => ref GetRef<ushort>(0x0C);
#endregion


#region InterfaceCount
    /// <summary>
    ///     Gets the number of interfaces implemented by this type.
    /// </summary>
    public ref ushort InterfaceCount => ref GetRef<ushort>(0x0E);
#endregion

    // For DEBUG builds, there is a conditional field here (see methodtable.h again).
    // 0x10: debug_m_szClassName (display name of the class, for the debugger)

#region ParentMethodTable
    /// <summary>
    ///     Gets the parent method table pointer.
    /// </summary>
    public ref nint ParentMethodTablePtr => ref GetRef<nint>(ParentMethodTableOffset);

    /// <summary>
    ///     Gets the parent method table handle.
    /// </summary>
    public MethodTableHandle? ParentMethodTable => GetHandle<MethodTableHandle>(ParentMethodTableOffset);
#endregion

#region InterfaceMap
    /// <summary>
    ///     Gets the raw interface map pointer.
    /// </summary>
    public ref nint InterfaceMapPtr => ref GetRef<nint>(InterfaceMapOffset);

    /// <summary>
    ///     Gets the interface map for the type.
    /// </summary>
    public unsafe MethodTableHandle[] InterfaceMap => GetArray<MethodTableHandle>(InterfaceMapPtr, InterfaceCount);
#endregion

    /// <summary>
    ///     Gets whether the type has a component size (i.e., is an array or
    ///     string).
    /// </summary>
    public bool HasComponentSize => (Flags & 0x80000000) != 0;

    /// <summary>
    ///     Gets whether the type contains GC pointers.
    /// </summary>
    public bool ContainsGcPointers => (Flags & 0x01000000) != 0;

    /// <summary>
    ///     Gets whether the type is an interface.
    /// </summary>
    public bool IsInterface => (Flags & 0x000C0000) == 0x000C0000;

    /// <summary>
    ///     Gets whether the type is a value type.
    /// </summary>
    public bool IsValueType => (Flags & 0x000C0000) == 0x00040000;

    /// <summary>
    ///     Gets whether the type is a nullable type.
    /// </summary>
    public bool IsNullable => (Flags & 0x000F0000) == 0x00050000;

    /// <summary>
    ///     Gets the rank of a multidimensional array, or 0 if not an array.
    /// </summary>
    public int MultiDimensionalArrayRank
    {
        get
        {
            if (!HasComponentSize)
            {
                return 0;
            }

            return (int)((BaseSize - (uint)(3 * nint.Size)) / (2 * sizeof(int)));
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="MethodTableHandle"/>s refer to the
    ///     same type.
    /// </summary>
    public static bool AreSameType(MethodTableHandle mt1, MethodTableHandle mt2)
    {
        return mt1.Value == mt2.Value;
    }

#region Factory
    public static MethodTableHandle Create(nint value)
    {
        return new MethodTableHandle(value);
    }

    public static MethodTableHandle? CreateAllowInvalid(nint value)
    {
        return value == nint.Zero ? null : new MethodTableHandle(value);
    }
#endregion
}