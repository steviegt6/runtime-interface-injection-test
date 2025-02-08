// See https://aka.ms/new-console-template for more information

using System;

using Outerface;

InterfaceInjector.InjectInterfaces(typeof(TheClass), InterfaceIdentity.CreateEmpty(typeof(IInterface)));
((IInterface)new TheClass()).Print();
Console.WriteLine("hi");

class TheClass;

interface IInterface
{
    void Print();
}