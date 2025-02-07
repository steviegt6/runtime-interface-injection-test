using System;
using System.Runtime.InteropServices;

using Outerface.CompilerServices;

namespace Outerface;

public readonly record struct InterfaceIdentity(
    Type InterfaceType,
    Type InterfaceImplementationType
);

public static class InterfaceInjector
{
    public static void InjectInterface(Type origType, InterfaceIdentity interfaceIdentity)
    {
        InjectInterfaces(origType, interfaceIdentity);
    }

    public static void InjectInterfaces(Type origType, params InterfaceIdentity[] interfaceIdentities)
    {
        InjectInterfacesIntoClass(origType, interfaceIdentities);
    }

    public static MethodTableHandle GetMethodTable(Type type)
    {
        return new MethodTableHandle(type.TypeHandle.Value);
    }

    private static void InjectInterfacesIntoClass(
        Type                target,
        InterfaceIdentity[] interfaces
    )
    {
        // RuntimeHelpers.PrepareConstrainedRegions();
        foreach (var interfaceIdentity in interfaces)
        {
            InjectInterfaceIntoClass(target, interfaceIdentity);
        }
    }

    private static unsafe void InjectInterfaceIntoClass(
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
        var interfaceEntries = (MethodTableHandle.InterfaceInfo*)newInterfaceMap;
        interfaceEntries[targetMt.InterfaceCount].MethodTablePtr = interfaceMt.Value;
        
        // Update target's MethodTable
        targetMt.InterfaceMapPtr = newInterfaceMap;
        targetMt.InterfaceCount++;
        
        // TODO: modify vtable
    }
}