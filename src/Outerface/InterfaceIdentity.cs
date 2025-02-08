using System;
using System.Collections.Generic;
using System.Reflection;

namespace Outerface;

public sealed class InterfaceIdentity
{
    public Type InterfaceType { get; }

    public Dictionary<MethodInfo, MethodInfo> InterfaceImplementationMap { get; }

    private InterfaceIdentity(Type interfaceType, Dictionary<MethodInfo, MethodInfo> interfaceImplementationMap)
    {
        InterfaceType              = interfaceType;
        InterfaceImplementationMap = interfaceImplementationMap;
    }

    /// <summary>
    ///     Creates an implementation-less interface identity.
    /// </summary>
    /// <param name="interfaceType">The interface type.</param>
    /// <returns>
    ///     A new <see cref="InterfaceIdentity"/> with no interface
    ///     implementations, assuming the interface has no methods to implement.
    /// </returns>
    public static InterfaceIdentity CreateEmpty(Type interfaceType)
    {
        // if (interfaceType.GetMethods().Length > 0)
        // {
        //     throw new ArgumentException("Interface must have no methods to be empty.", nameof(interfaceType));
        // }

        return new InterfaceIdentity(interfaceType, []);
    }
}