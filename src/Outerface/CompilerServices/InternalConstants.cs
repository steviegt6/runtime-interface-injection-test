using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Outerface.CompilerServices;

internal static class InternalConstants
{
    public static readonly bool IS_DEBUGGABLE = typeof(RuntimeHelpers).Assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled ?? false;

    public static readonly bool IS_64_BIT = nint.Size == 8;

    public static readonly int DEBUG_CLASS_NAME_PTR = IS_DEBUGGABLE
        ? IS_64_BIT ? 8 : 4
        : 0;
}