#nullable disable

using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.UI.Xaml.Documents;

using perfopt.Native;

namespace perfopt.Views
{
    public partial class MainPage : Page
    {
        private int counter = 0;
        private PerformanceCounter cpuCounter;
        static DispatcherTimer dispatcherTimer;

        private const uint PROCESS_SET_QUOTA = 0x0100;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;

        [DllImport("kernel32.dll")]
        private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS sps);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("psapi.dll")]
        static extern bool EmptyWorkingSet(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public void Init()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();

            this.Update();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            this.Update();
        }

        public float GetBatteryCharge()
        {
            if (GetSystemPowerStatus(out SYSTEM_POWER_STATUS sps))
            {
                memoryButton.Content = sps.BatteryLifePercent.ToString();
                return sps.BatteryLifePercent;
            }
            throw new Exception("Failed to get battery status.");
        }

        public bool IsBatteryCharging()
        {
            if (GetSystemPowerStatus(out SYSTEM_POWER_STATUS sps))
            {
                return sps.ACLineStatus != 0;
            }
            throw new Exception("Failed to get power status.");
        }

        public double GetRamUsage()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            memStatus.Init();

            if (GlobalMemoryStatusEx(ref memStatus))
            {
                return (double)(memStatus.ullTotalPhys - memStatus.ullAvailPhys);
            }
            throw new Exception("Failed to get RAM info.");
        }

        public double GetRamUsagePercentage()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            memStatus.Init();

            if (GlobalMemoryStatusEx(ref memStatus))
            {
                double usedBytes = GetRamUsage();
                double totalBytes = memStatus.ullTotalPhys;
                return usedBytes / totalBytes;
            }
            throw new Exception("Failed to get RAM info.");
        }

        private unsafe void ClearMemory(object sender, RoutedEventArgs e)
        {
            //int* ptr = (int*)123;
            //*ptr += 1;
            native.clearMemory();
        }

        public void Update()
        {
            string command;

            float batteryCharge = GetBatteryCharge();

            if (IsBatteryCharging())
            {
                if (batteryCharge > 25)
                {
                    command = "/setactive scheme_min";
                }
                else
                {
                    command = "/setactive scheme_balanced";
                }
            }
            else
            {
                if (batteryCharge > 70)
                {
                    command = "/setactive scheme_min";
                }
                else if (batteryCharge > 30)
                {
                    command = "/setactive scheme_balanced";
                }
                else
                {
                    command = "/setactive scheme_max";
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            double ramUsage = GetRamUsagePercentage();
            int frequency;

            if (ramUsage > 0.8)
            {
                frequency = 5;
            }
            else if (ramUsage > 0.5)
            {
                frequency = 30;
            }
            else if (ramUsage > 0.25)
            {
                frequency = 60;
            }
            else
            {
                frequency = 120;
            }

            if (counter % frequency == 0)
            {
                native.clearMemory();
            }

            ramDisplay.Inlines.Clear();

            var ramNumberRun = new Run
            {
                Text = (ramUsage * 100).ToString("F0"),
                FontSize = 18
            };

            var ramPercentRun = new Run
            {
                Text = "%",
                FontSize = 14
            };

            ramDisplay.Inlines.Add(ramNumberRun);
            ramDisplay.Inlines.Add(ramPercentRun);

            double cpuUsage = native.getCpuUtilization();

            cpuDisplay.Inlines.Clear();

            var numberRun = new Run
            {
                Text = cpuUsage.ToString("F0"),
                FontSize = 18
            };

            var percentRun = new Run
            {
                Text = "%",
                FontSize = 14
            };

            cpuDisplay.Inlines.Add(numberRun);
            cpuDisplay.Inlines.Add(percentRun);
            counter++;
        }
    }
}
