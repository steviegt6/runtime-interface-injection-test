using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Outerface.CompilerServices;

public sealed class MethodTableHandle
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InterfaceInfo
    {
        public nint MethodTablePtr;
    }

    public static int ParentMethodTableOffset => 0x10 + InternalConstants.DebugClassNamePtr;

    public static int InterfaceMapOffset => nint.Size == 8 ? 0x38 + InternalConstants.DebugClassNamePtr : 0x24 + InternalConstants.DebugClassNamePtr;

    public nint Value { get; }

    public MethodTableHandle(nint value)
    {
        if (value == nint.Zero)
        {
            throw new ArgumentException("Value cannot be zero.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    ///     The low WORD of the first field is the component size for array and
    ///     string types.
    /// </summary>
    public ref ushort ComponentSize => ref GetRef<ushort>(0);

    /// <summary>
    ///     The flags for the current method table (only for not array or string
    ///     types).
    /// </summary>
    public ref uint Flags => ref GetRef<uint>(0);

    /// <summary>
    ///     Gets the base size of the type (used when allocating an instance on
    ///     the heap).
    /// </summary>
    public ref uint BaseSize => ref GetRef<uint>(4);

    // See additional native members in methodtable.h, not needed here yet.
    // 0x8: m_dwFlags2 (additional flags and token in upper 24 bits)
    // 0xC: m_wNumVirtuals

    /// <summary>
    ///     Gets the number of interfaces implemented by this type.
    /// </summary>
    public ref ushort InterfaceCount => ref GetRef<ushort>(0x0E);

    // For DEBUG builds, there is a conditional field here (see methodtable.h again).
    // 0x10: debug_m_szClassName (display name of the class, for the debugger)

    /// <summary>
    /// Gets the parent method table handle.
    /// </summary>
    public MethodTableHandle? ParentMethodTable
    {
        get
        {
            var parentPtr = GetValue<nint>(ParentMethodTableOffset);
            return parentPtr == nint.Zero ? null : new MethodTableHandle(parentPtr);
        }
    }

    /// <summary>
    ///     Gets the raw interface map pointer.
    /// </summary>
    public ref nint InterfaceMapPtr => ref GetRef<nint>(InterfaceMapOffset);

    /// <summary>
    ///     Gets the interface map for the type.
    /// </summary>
    public unsafe MethodTableHandle[] InterfaceMap
    {
        get
        {
            var interfaceCount = InterfaceCount;

            var interfaceMapPtr = InterfaceMapPtr;
            if (interfaceMapPtr == nint.Zero || interfaceCount == 0)
            {
                return [];
            }

            var interfaceMap = new MethodTableHandle[interfaceCount];
            for (var i = 0; i < interfaceCount; i++)
            {
                interfaceMap[i] = new MethodTableHandle(Unsafe.Read<nint>((void*)(interfaceMapPtr + i * nint.Size)));
            }

            return interfaceMap;
        }
    }

    /*


    /// <summary>
    /// Gets whether the type has a component size (i.e., is an array or string).
    /// </summary>
    public bool HasComponentSize => (Flags & 0x80000000) != 0;

    /// <summary>
    /// Gets whether the type contains GC pointers.
    /// </summary>
    public bool ContainsGCPointers => (Flags & 0x01000000) != 0;

    /// <summary>
    /// Gets whether the type is an interface.
    /// </summary>
    public bool IsInterface => (Flags & 0x000C0000) == 0x000C0000;

    /// <summary>
    /// Gets whether the type is a value type.
    /// </summary>
    public bool IsValueType => (Flags & 0x000C0000) == 0x00040000;

    /// <summary>
    /// Gets whether the type is a nullable type.
    /// </summary>
    public bool IsNullable => (Flags & 0x000F0000) == 0x00050000;

    /// <summary>
    /// Gets the rank of a multi-dimensional array, or 0 if not an array.
    /// </summary>
    public int MultiDimensionalArrayRank
    {
        get
        {
            if (!HasComponentSize) return 0;
            return (int)((BaseSize - (uint)(3 * IntPtr.Size)) / (uint)(2 * sizeof(int)));
        }
    }

    /// <summary>
    /// Determines whether two MethodTableHandles refer to the same type.
    /// </summary>
    public static bool AreSameType(MethodTableHandle mt1, MethodTableHandle mt2) => mt1._methodTablePtr == mt2._methodTablePtr;


     */

    /// <summary>
    ///     Gets a reference to a value at a given offset.
    /// </summary>
    private unsafe ref T GetRef<T>(int offset) where T : unmanaged
    {
        return ref Unsafe.AsRef<T>((void*)(Value + offset));
    }

    /// <summary>
    ///     Gets a value at a given offset.
    /// </summary>
    private unsafe T GetValue<T>(int offset) where T : unmanaged
    {
        return Unsafe.Read<T>((void*)(Value + offset));
    }
}