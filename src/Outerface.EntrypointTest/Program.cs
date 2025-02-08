// See https://aka.ms/new-console-template for more information

using System;
using System.Diagnostics;

using Outerface;

HookTest.Hook();
InterfaceInjector.InjectInterfaces(typeof(TheClass), InterfaceIdentity.CreateEmpty(typeof(IInterface)));
Debugger.Break();
((IInterface)new TheClass()).Print();
Console.WriteLine("hi");

class TheClass;

interface IInterface
{
    void Print();
}