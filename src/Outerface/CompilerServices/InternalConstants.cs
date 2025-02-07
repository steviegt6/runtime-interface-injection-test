using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Outerface.CompilerServices;

internal static class InternalConstants
{
    public static int DebugClassNamePtr => debug_class_name_ptr.Value;

    private static readonly Lazy<int> debug_class_name_ptr = new(
        () =>
        {
            /*var methodTableType = typeof(RuntimeHelpers).Assembly.GetType("System.Runtime.CompilerServices.MethodTable");
            {
                Debug.Assert(methodTableType is not null, "Could not find the MethodTable type.");
            }

            var debugClassNamePtrConstField = methodTableType.GetField("DebugClassNamePtr", BindingFlags.NonPublic | BindingFlags.Static);
            {
                Debug.Assert(debugClassNamePtrConstField is not null, "Could not find the DebugClassNamePtr field.");
            }

            var value = debugClassNamePtrConstField.GetValue(null) as int?;
            {
                Debug.Assert(value is not null, "Could not get the value of the DebugClassNamePtr field.");
            }

            return value.Value;*/

            var isDebuggable = typeof(RuntimeHelpers).Assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled ?? false;

            return isDebuggable ? nint.Size == 8 ? 8 : 4 : 0;
        }
    );
}