using System;
using System.Runtime.CompilerServices;

namespace Outerface.CompilerServices.Handles;

/// <summary>
///     An abstraction for allowing handles to provide constructors used in
///     utility methods.
/// </summary>
/// <typeparam name="THandle">The handle type.</typeparam>
public interface IHandleFactory<out THandle> where THandle : AbstractHandle
{
    /// <summary>
    ///     Creates a new handle from a pointer that is not allowed to be
    ///     <see langword="null"/>.
    /// </summary>
    /// <param name="value">The pointer.</param>
    /// <returns>A new handle wrapping the pointer.</returns>
    static abstract THandle Create(nint value);

    /// <summary>
    ///     Creates a new handle from a pointer that is allowed to be
    ///     <see langword="null"/>.
    /// </summary>
    /// <param name="value">The pointer.</param>
    /// <returns>
    ///     A new handle wrapping the pointer, or <see langword="null"/> if the
    ///     pointer is invalid (zero).
    /// </returns>
    static abstract THandle? CreateAllowInvalid(nint value);
}

/// <summary>
///     Wraps a pointer and provides a more descriptive, dynamic view into its
///     data.
///     <br />
///     Designed specifically to account for differences in architecture and
///     memory layout.
/// </summary>
public abstract class AbstractHandle
{
    protected nint Value { get; set; }

    protected AbstractHandle() { }

    protected AbstractHandle(nint value)
    {
        if (value == nint.Zero)
        {
            throw new ArgumentException("Handle cannot be zero.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    ///     Gets the pointer value.
    /// </summary>
    public nint DangerousGetHandle()
    {
        return Value;
    }

    /// <summary>
    ///     Gets a reference to a value at a given offset.
    /// </summary>
    protected unsafe ref T GetRef<T>(int offset) where T : unmanaged
    {
        return ref Unsafe.AsRef<T>((void*)(Value + offset));
    }

    /// <summary>
    ///     Gets a value at a given offset.
    /// </summary>
    protected unsafe T GetValue<T>(int offset) where T : unmanaged
    {
        return Unsafe.Read<T>((void*)(Value + offset));
    }

    /// <summary>
    ///     Gets a handle at a given offset.
    /// </summary>
    protected T? GetHandle<T>(int offset) where T : AbstractHandle, IHandleFactory<T>
    {
        var ptr = GetValue<nint>(offset);
        return ptr == nint.Zero ? null : T.Create(ptr);
    }

    /// <summary>
    ///     Creates a managed array of handles from a pointer and a length,
    ///     assuming the pointer points to a contiguous block of memory of
    ///     <paramref name="length"/> items.
    /// </summary>
    /// <param name="ptr">The pointer to the array in memory.</param>
    /// <param name="length">The amount of items in memory.</param>
    /// <typeparam name="TItem">The type of handle that should wrap.</typeparam>
    /// <returns>
    ///     An array of handles wrapping the memory at <paramref name="ptr"/>.
    /// </returns>
    public static TItem[] GetArray<TItem>(nint ptr, int length) where TItem : AbstractHandle, IHandleFactory<TItem>
    {
        if (ptr == nint.Zero || length == 0)
        {
            return [];
        }

        var array = new TItem[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = TItem.Create(ptr + i * nint.Size);
        }

        return array;
    }
}