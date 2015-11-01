using System.Runtime.InteropServices;

namespace Managed.Memory
{
    public static class SDRAMInterop
    {
        [DllImport("C")]
        internal static extern byte BSP_SDRAM_Init();
    }
}
