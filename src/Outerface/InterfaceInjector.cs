using System;
using System.Runtime.InteropServices;

using Outerface.CompilerServices.Handles;

namespace Outerface;

public static class InterfaceInjector
{
    public static void InjectInterfaces(
        Type                       target,
        params InterfaceIdentity[] interfaces
    )
    {
        foreach (var interfaceIdentity in interfaces)
        {
            InjectInterface(target, interfaceIdentity);
        }
    }
    
    public static MethodTableHandle GetMethodTable(Type type)
    {
        var handle = MethodTableHandle.Create(type.TypeHandle.Value);
        if (handle is null)
        {
            throw new ArgumentException("Type must be a valid reference type.", nameof(type));
        }

        return handle;
    }
    
    private static unsafe void InjectInterface(
        Type              target,
        InterfaceIdentity interfaceIdentity
    )
    {
        var targetMt    = GetMethodTable(target);
        var interfaceMt = GetMethodTable(interfaceIdentity.InterfaceType);

        // Allocate new interface map with one extra entry
        var newSize         = (targetMt.InterfaceCount + 1) * sizeof(nint);
        var newInterfaceMap = Marshal.AllocHGlobal(newSize);
        Buffer.MemoryCopy((void*)targetMt.InterfaceMapPtr, (void*)newInterfaceMap, newSize, newSize - sizeof(nint));

        // Add new interface entry
        var interfaceEntries = AbstractHandle.GetArray<InterfaceInfoHandle>(newInterfaceMap, newSize);
        interfaceEntries[targetMt.InterfaceCount].MethodTablePtr = interfaceMt.DangerousGetHandle();

        // Update target's MethodTable
        targetMt.InterfaceMapPtr = newInterfaceMap;
        targetMt.InterfaceCount++;

        // TODO: modify vtable
    }
}