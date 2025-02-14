using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using MinHook;

using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;

namespace Outerface;

public static partial class HookTest
{
    private static partial class SignatureScanner
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public static byte[] ReadModuleMemory(ProcessModule module)
        {
            var handle = Process.GetCurrentProcess().Handle;
            var buffer = new byte[module.ModuleMemorySize];
            ReadProcessMemory(handle, module.BaseAddress, buffer, buffer.Length, out _);
            return buffer;
        }

        public static int FindPatterns(byte[] data, params string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                var offset = FindPattern(data, pattern);
                if (offset != -1)
                {
                    return offset;
                }
            }

            return -1;
        }

        public static int FindPattern(byte[] data, string pattern)
        {
            var patternBytes = pattern.Split(' ')
                                      .Select(x => x == "??" ? default(byte?) : Convert.ToByte(x, 16))
                                      .ToArray();

            for (var i = 0; i <= data.Length - patternBytes.Length; i++)
            {
                if (!IsMatch(data, i, patternBytes))
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        private static bool IsMatch(byte[] data, int start, byte?[] pattern)
        {
            for (var i = 0; i < pattern.Length; i++)
            {
                var patternByte = pattern[i];
                if (patternByte.HasValue && data[start + i] != patternByte.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DispatchToken
    {
        public nint TokenPtr;
    }

    private static HookEngine engine = new();

    public static unsafe void Hook()
    {
        Console.WriteLine(Environment.Version);
        var signatures = (string[])
        [
            string.Join(
                ' ',
                "48 89 5C 24 10",          // mov     [rsp-8+arg_8], rbx
                "55",                      // push    rbp
                "56",                      // push    rsi
                "57",                      // push    rdi
                "41 54",                   // push    r12
                "41 55",                   // push    r13
                "41 56",                   // push    r14
                "41 57",                   // push    r15
                "48 8D AC 24 ?? ?? ?? ??", // lea     rbp, [rsp-0A0h]     60 FF FF FF
                "48 81 EC ?? ?? ?? ??"     // sub     rsp, 1A0h           A0 01 00 00
            ),
            "4C 89 4C 24 20 4C 89 44 24 18 48 89 54 24 10 48 89 4C 24 08 57 48 81 EC 50 08 00 00 48 8D 7C 24 40",
        ];
        ;

        var proc          = Process.GetCurrentProcess();
        var coreclr       = proc.Modules.Cast<ProcessModule>().First(x => x.ModuleName == "coreclr.dll");
        var coreclrMemory = SignatureScanner.ReadModuleMemory(coreclr);
        var fuckYouOffset = SignatureScanner.FindPatterns(coreclrMemory, "48 83 EC 38 E8 3F 1C F0 FF 48 83 64 24 28 00", "40 57 48 83 EC 30 E8 25 7F 50 00 48 C7 44");

        // var getThreadOffset = SignatureScanner.FindPatterns(coreclrMemory, "8B 0D FA B2 32 00 65 48 8B 04 25 58 00 00 00 BA 30 01 00 00", "40 57 33 C0 85 C0 74 02");

        var offset = SignatureScanner.FindPatterns(coreclrMemory, signatures);

        if (offset == -1)
        {
            throw new Exception("offset not found");
        }

        Console.WriteLine($"Got initial offset: {offset}");
        
        // write ret instruction to the start of the ReversePInvokeBadTransition function (fuckYouOffset)
        {
            VirtualProtect(coreclr.BaseAddress + fuckYouOffset, 1, 0x40, out _);
            var ret = new byte[] { 0xC3 };
            Marshal.Copy(ret, 0, coreclr.BaseAddress + fuckYouOffset, ret.Length);
        }
        
        Console.WriteLine("yay");

        // engine.EnableHook(engine.CreateHook(coreclr.BaseAddress + offset, new ReversePInvokeBadTransition(DisableBadTransition)));
        orig = engine.CreateHook(coreclr.BaseAddress + offset, new VirtualCallStubManager__Resolver(ResolverImpl));
        engine.EnableHook(orig);

        // Console.WriteLine(fuckYouOffset);

        //origFunction = ReloadedHooks.Instance.CreateHook<VirtualCallStubManager__Resolver>((delegate* unmanaged[Cdecl]<nint, DispatchToken, nint, nint, int, int>)&ResolverImpl, coreclr.BaseAddress + offset).Activate();
        // fuckYou      = ReloadedHooks.Instance.CreateHook<ReversePInvokeBadTransition>(DisableBadTransition, coreclr.BaseAddress + fuckYouOffset).Activate();
        // origFunction = ReloadedHooks.Instance.CreateHook<VirtualCallStubManager__Resolver>(ResolverImpl, coreclr.BaseAddress    + offset).Activate();

        /*var type = origFunction.GetType();
        Console.WriteLine(origFunction.GetType());
        Console.WriteLine(origFunction.GetType().GetMethod("Invoke", BindingFlags.NonPublic | BindingFlags.Instance));*/
    }

    // private static IHook<VirtualCallStubManager__Resolver> origFunction;
    // private static IHook<ReversePInvokeBadTransition>      fuckYou;

    // [Function(CallingConventions.Cdecl)]
    private delegate int VirtualCallStubManager__Resolver(
        nint          pMt,
        DispatchToken token,
        nint          protectedObj,
        nint          ppTarget,
        int           throwOnConflict
    );

    private static VirtualCallStubManager__Resolver orig;

    //[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int ResolverImpl(
        nint          pMt,
        DispatchToken token,
        nint          protectedObj,
        nint          ppTarget,
        int           throwOnConflict
    )
    {
        Console.WriteLine("Resolver: " + pMt);
        return orig(pMt, token, protectedObj, ppTarget, throwOnConflict);
    }

    // [Function(CallingConventions.Cdecl)]
    private delegate void ReversePInvokeBadTransition();

    private static void DisableBadTransition() { }
    
    // define virtualprotect
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(nint lpAddress, nint dwSize, uint flNewProtect, out uint lpflOldProtect);
}