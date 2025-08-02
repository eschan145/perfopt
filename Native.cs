namespace perfopt.Native
{
    using System.Runtime.InteropServices;

    public static class native
    {
        [DllImport("libperfopt.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void clearMemory();
    }
}
