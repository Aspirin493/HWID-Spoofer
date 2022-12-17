using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

/*
    This is my first spoofer, what i wrote.
    It will help you change the system settings you want and bypass the lockout/tagout in the right places.
*/

namespace spoofer_hwid
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        static bool hwid, guid, mac, install_date, hdd, product_id;

        private void spoof_hwid_CheckedChanged(object sender, EventArgs e)
        {
            hwid = spoof_hwid.Checked;
        }

        public void add_log(string msg)
        {
            listBox1.Items.Add(msg);
        }

        public static void Restart()
        {
            StartShutDown("-f -r -t 5");
        }
        private static void StartShutDown(string param)
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = "cmd";
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Arguments = "/C shutdown " + param;
            Process.Start(proc);
        }

        #region hwid
        public static Regedit regeditOBJ = new Regedit(@"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001");
        public static readonly string Key = "HwProfileGuid";

        public static string GetHwid()
        {
            return regeditOBJ.Read(Key);
        }

        public static bool SetHwid(object value)
        {
            return regeditOBJ.Write(Key, value);
        }

        public bool Spoof_hwid()
        {
            string oldValue = GetHwid();
            bool result = SetHwid("{" + Guid.NewGuid().ToString() + "}");
            if (result)
            {
                add_log("[SUCCES] HWID Changed from " + oldValue + " to " + GetHwid());
            }
            else
            {
                add_log("[ERROR] Hwid error accessing the Registry... Maybe run as admin");
            }
            return result;
        }
        #endregion
        #region guid
        void SpoofGUID()
        {
            add_log("[INFO] Current GUID: " + CurrentGUID());

            string newGUID = Guid.NewGuid().ToString();

            RegistryKey OurKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            OurKey = OurKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", true);
            OurKey.SetValue("MachineGuid", newGUID);

            add_log("[SUCCES] GUID changed to: " + CurrentGUID());
        }

        public static string CurrentGUID()
        {
            string location = @"SOFTWARE\Microsoft\Cryptography";
            string name = "MachineGuid";

            using (RegistryKey localMachineX64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
                {
                    if (rk == null)
                        throw new KeyNotFoundException(string.Format("Key Not Found: {0}", location));

                    object machineGuid = rk.GetValue(name);
                    if (machineGuid == null)
                        throw new IndexOutOfRangeException(string.Format("Index Not Found: {0}", name));

                    return machineGuid.ToString();
                }
            }
        }
        #endregion
        #region MACAddressSpoof

        Random rand = new Random();
        public const string Alphabet = "ABCDEF0123456789";
        public string GenerateString(int size)
        {
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet[rand.Next(Alphabet.Length)];
            }
            return new string(chars);
        }

        void SpoofMacAddress()
        {
            add_log("[INFO] Current MAC Address: " + CurrentMAC());

            try
            {
                string newMACAddress = "00" + GenerateString(10);
                RegistryKey mac = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\0012", true);
                mac.SetValue("NetworkAddress", newMACAddress);
                mac.Close();
                add_log("[SUCCES] MAC address changed to: " + CurrentMAC());
            }
            catch
            {
                add_log("[ERROR] Exception when try to change mac");
            }
        }

        string CurrentMAC()
        {
            RegistryKey mac;
            string MAC;
            mac = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\0012", true);
            MAC = (string)mac.GetValue("NetworkAddress");
            mac.Close();
            return MAC;
        }
        #endregion
        #region InstallTimeSpoof
        Random random = new Random();
        public const string Alphabet1 = "abcdef0123456789";
        public string GenerateDate(int size)
        {
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet1[random.Next(Alphabet1.Length)];
            }
            return new string(chars);
        }

        void SpoofInstallTime()
        {
            add_log("[INFO] Current install time: " + CurrentInstallTime());

            string newInstallTime = GenerateDate(15);

            RegistryKey OurKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            OurKey = OurKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", true);
            OurKey.SetValue("InstallTime", newInstallTime);
            OurKey.Close();
            add_log("[SUCCES] Install time changed to: " + CurrentInstallTime());
        }

        public static string CurrentInstallTime()
        {
            string location = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string name = "InstallTime";

            using (RegistryKey localMachineX64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
                {
                    if (rk == null)
                        throw new KeyNotFoundException(string.Format("Key Not Found: {0}", location));

                    object installtime = rk.GetValue(name);
                    if (installtime == null)
                        throw new IndexOutOfRangeException(string.Format("Index Not Found: {0}", name));

                    return installtime.ToString();
                }
            }
        }
        #endregion
        #region InstallDateSpoof
        void SpoofInstallDate()
        {
            add_log("[INFO] Current install date: " + CurrentInstallDate());

            string newInstallDate = GenerateDate(8);

            RegistryKey OurKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            OurKey = OurKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", true);
            OurKey.SetValue("InstallDate", newInstallDate);
            OurKey.Close();

            add_log("[SUCCES] New Install Date: " + CurrentInstallDate());
        }

        public static string CurrentInstallDate()
        {
            string location = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string name = "InstallDate";

            using (RegistryKey localMachineX64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
                {
                    if (rk == null)
                        throw new KeyNotFoundException(string.Format("Key Not Found: {0}", location));

                    object installdate = rk.GetValue(name);
                    if (installdate == null)
                        throw new IndexOutOfRangeException(string.Format("Index Not Found: {0}", name));

                    return installdate.ToString();
                }
            }
        }
        #endregion
        #region SpoofPCName
        void SpoofPCName()
        {
            add_log("[INFO] Current PC name: " + CurrentPCName());

            try
            {
                RegistryKey OurKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                OurKey = OurKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName", true);
                OurKey.SetValue("ComputerName", "DESKTOP-" + GenerateString(15));
                OurKey.Close();

                add_log("[SUCCES] PC name changed to: " + CurrentPCName());
            }
            catch
            {
                add_log("[ERROR] kernel64.dll , pc_name call exception");
            }
        }

        public static string CurrentPCName()
        {
            string location = @"SYSTEM\CurrentControlSet\Control\ComputerName\ActiveComputerName";
            string name = "ComputerName";

            using (RegistryKey localMachineX64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
                {
                    if (rk == null)
                        throw new KeyNotFoundException(string.Format("Key Not Found: {0}", location));

                    object pcname = rk.GetValue(name);
                    if (pcname == null)
                        throw new IndexOutOfRangeException(string.Format("Index Not Found: {0}", name));

                    return pcname.ToString();
                }
            }
        }
        #endregion
        #region HddSerialSpoof
        void ChangeSerialNumber(char volume, uint newSerial)
        {
            var fsInfo = new[]
            {
        new { Name = "FAT32", NameOffs = 0x52, SerialOffs = 0x43 },
        new { Name = "FAT", NameOffs = 0x36, SerialOffs = 0x27 },
        new { Name = "NTFS", NameOffs = 0x03, SerialOffs = 0x48 }
    };

            using (var disk = new Disk(volume))
            {
                var sector = new byte[512];
                disk.ReadSector(0, sector);

                var fs = fsInfo.FirstOrDefault(
                        f => Strncmp(f.Name, sector, f.NameOffs)
                    );
                if (fs == null) throw new NotSupportedException("This file system is not supported");

                var s = newSerial;
                for (int i = 0; i < 4; ++i, s >>= 8) sector[fs.SerialOffs + i] = (byte)(s & 0xFF);

                disk.WriteSector(0, sector);
            }
        }

        bool Strncmp(string str, byte[] data, int offset)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (data[i + offset] != (byte)str[i]) return false;
            }
            return true;
        }

        class Disk : IDisposable
        {
            private SafeFileHandle handle;

            public Disk(char volume)
            {
                var ptr = CreateFile(
                    String.Format("\\\\.\\{0}:", volume),
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite,
                    IntPtr.Zero,
                    FileMode.Open,
                    0,
                    IntPtr.Zero
                    );

                handle = new SafeFileHandle(ptr, true);

                if (handle.IsInvalid) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            public void ReadSector(uint sector, byte[] buffer)
            {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (SetFilePointer(handle, sector, IntPtr.Zero, EMoveMethod.Begin) == INVALID_SET_FILE_POINTER) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                uint read;
                if (!ReadFile(handle, buffer, buffer.Length, out read, IntPtr.Zero)) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                if (read != buffer.Length) throw new IOException();
            }

            public void WriteSector(uint sector, byte[] buffer)
            {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (SetFilePointer(handle, sector, IntPtr.Zero, EMoveMethod.Begin) == INVALID_SET_FILE_POINTER) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                uint written;
                if (!WriteFile(handle, buffer, buffer.Length, out written, IntPtr.Zero)) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                if (written != buffer.Length) throw new IOException();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (handle != null) handle.Dispose();
                }
            }

            enum EMoveMethod : uint
            {
                Begin = 0,
                Current = 1,
                End = 2
            }

            const uint INVALID_SET_FILE_POINTER = 0xFFFFFFFF;

            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr CreateFile(
                string fileName,
                [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
                [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
                IntPtr securityAttributes,
                [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                int flags,
                IntPtr template);

            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            static extern uint SetFilePointer(
                 [In] SafeFileHandle hFile,
                 [In] uint lDistanceToMove,
                 [In] IntPtr lpDistanceToMoveHigh,
                 [In] EMoveMethod dwMoveMethod);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool ReadFile(SafeFileHandle hFile, [Out] byte[] lpBuffer,
                int nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            [DllImport("kernel32.dll")]
            static extern bool WriteFile(SafeFileHandle hFile, [In] byte[] lpBuffer,
                int nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten,
                [In] IntPtr lpOverlapped);
        }

        void SpoofHWIDserial()
        {
            string newSerial = GenerateString(8);
            uint serial = uint.Parse(newSerial, NumberStyles.HexNumber);
            add_log("HDD serial number changing to: " + newSerial + " - " + serial);
            ChangeSerialNumber('C', serial);
        }
        #endregion
        #region ProductIDD
        public static Regedit regedit_productid = new Regedit(@"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
        public static readonly string Key_productid = "ProductID";
        public static string GetProductId()
        {
            return regedit_productid.Read(Key_productid);
        }

        public static bool SetProductId(object value)
        {
            return regedit_productid.Write(Key_productid, value);
        }

        public bool SpoofProductID()
        {
            string oldValue = GetProductId();
            bool result = SetProductId(Utilities.GenerateString(5) + "-" + Utilities.GenerateString(5) + "-" + Utilities.GenerateString(5) + "-" + Utilities.GenerateString(5));
            if (result)
            {
                add_log("[SUCCES] Computer ProductID Changed from " + oldValue + " to " + GetProductId());
            }
            else
            {
                add_log("[ERROR] Computer ProductID error acces to registry");
            }
            return result;
        }
        #endregion

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            if (hwid)
                Spoof_hwid();
            if (guid)
                SpoofGUID();
            if (mac)
                SpoofMacAddress();
            if (install_date)
            {
                SpoofInstallDate();
                SpoofInstallTime();
            }
            if (hdd)
                SpoofHWIDserial();
            if (product_id)
                SpoofProductID();

            DialogResult dialogResult = MessageBox.Show("For a full-fledged spoof, reboot the system, re-spoof, and do not restart the system.\nDo you want to reboot now?", "spoofer", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Restart();
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }

        private void guna2CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            install_date = guna2CheckBox1.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2CheckBox4_CheckedChanged(object sender, EventArgs e)
        {
            product_id = guna2CheckBox4.Checked;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void guna2CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            hdd = guna2CheckBox3.Checked;
        }

        private void spoof_mac_CheckedChanged(object sender, EventArgs e)
        {
            mac = spoof_mac.Checked;
        }

        private void spoof_guid_CheckedChanged(object sender, EventArgs e)
        {
            guid = spoof_guid.Checked;
        }

        public class Regedit
        {
            private string regeditPath = string.Empty;
            public Regedit(string regeditPath)
            {
                this.regeditPath = regeditPath;
            }

            public string Read(string keyName)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regeditPath))
                    {
                        if (key != null)
                        {
                            return key.GetValue(keyName).ToString();
                        }
                        else
                        {
                            //Console.WriteLine("  [Regedit] SubKey Doesn't founded!");
                            return "ERR";
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("  [Regedit] Error accessing the Registry... Maybe run as admin?\n\n" + ex.ToString());
                    return "ERR";
                }
            }

            public bool Write(string keyName, object value)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regeditPath))
                    {
                        if (key != null)
                        {
                            key.SetValue(keyName, value);
                            return true;
                        }
                        else
                        {
                            //Console.WriteLine("  [Regedit] SubKey Doesn't founded!");
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("  [Regedit] Error accessing the Registry... Maybe run as admin? -" + ex.ToString());
                    return false;
                }
            }
        }
        public static class Utilities
        {
            private static Random rand = new Random();
            public const string Alphabet = "ABCDEF0123456789";
            private static Random random = new Random();
            public const string Alphabet1 = "abcdef0123456789";

            public static string GenerateString(int size)
            {
                char[] array = new char[size];
                for (int i = 0; i < size; i++)
                {
                    array[i] = Alphabet[rand.Next(Alphabet.Length)];
                }
                return new string(array);
            }
        }
    }
}
