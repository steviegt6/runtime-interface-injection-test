using System;

namespace Outerface.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class TypeDefAttribute(string srcPath, string typeDef) : Attribute;