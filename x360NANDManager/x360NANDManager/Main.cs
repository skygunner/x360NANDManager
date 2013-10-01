﻿namespace x360NANDManager {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using x360NANDManager.MMC;
    using x360NANDManager.SPI;
    using x360NANDManager.XSVF;

    public static class Main {
        [DllImport("shell32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsUserAnAdmin();

        private const string BaseName = "x360NANDManager v{0}.{1} (Build: {2}) {3}";
        private static readonly Version Ver = Assembly.GetExecutingAssembly().GetName().Version;

        public static string Version {
            get {
#if DEBUG
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, "DEBUG BUILD");
#elif ALPHA
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, Ver.Revision > 0 ? string.Format("ALPHA{0}", Ver.Revision) : "ALPHA");
#elif BETA
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, Ver.Revision > 0 ? string.Format("BETA{0}", Ver.Revision) : "BETA");
#else
                return string.Format(BaseName, Ver.Major, Ver.Minor, Ver.Build, "");
#endif
            }
        }

        public static ISPIFlasher GetSPIFlasher() {
            if(NativeWin32.IsDeviceConnected(0xFFFF, 0x4))
                return new ARMFlasher(0xFFFF, 0x4);
            if(NativeWin32.IsDeviceConnected(0x11D4, 0x8338))
                return new ARMFlasher(0x11D4, 0x8338);
            throw new x360NANDManagerException(x360NANDManagerException.ErrorLevels.NoDeviceFound);
        }

        public static ISPIFlasher GetSPIFlasher(int vendorID, int productID) {
            if(!NativeWin32.IsDeviceConnected(vendorID, productID))
                throw new x360NANDManagerException(x360NANDManagerException.ErrorLevels.NoDeviceFound);
            return new ARMFlasher(vendorID, productID);
        }

        public static IXSVFFlasher GetXSVFFlasher() {
            if(NativeWin32.IsDeviceConnected(0xFFFF, 0x4)) {
                var flasher = new ARMXSVFFlasher(0xFFFF, 0x4);
                if(!flasher.IsCompatibleVersion())
                    throw new x360NANDManagerException(x360NANDManagerException.ErrorLevels.IncompatibleDevice);
                return flasher;
            }
            if(NativeWin32.IsDeviceConnected(0x11D4, 0x8338))
                return new JRPXSVFFlasher(0x11D4, 0x8338);
            throw new x360NANDManagerException(x360NANDManagerException.ErrorLevels.NoDeviceFound);
        }

        public static IXSVFFlasher GetXSVFFlasher(int vendorID, int productID) {
            if(NativeWin32.IsDeviceConnected(vendorID, productID))
                return new ARMXSVFFlasher(vendorID, productID);
            throw new x360NANDManagerException(x360NANDManagerException.ErrorLevels.NoDeviceFound);
        }

        public static IMMCFlasher GetMMCFlasher(MMCDevice device) {
            if(!IsUserAnAdmin())
                throw new Exception("You must be admin to use this function...");
            return new MMCFlasher(device);
        }

        public static IList<MMCDevice> GetMMCDeviceList(bool onlyRemoveable = true) {
            if (!IsUserAnAdmin())
                throw new Exception("You must be admin to use this function...");
            return MMCFlasher.GetDevices(onlyRemoveable);
        }

        public static event EventHandler<EventArg<string>> Debug;

        [Conditional("DEBUG")] internal static void SendDebug(string message) {
            var dbg = Debug;
            if(dbg != null && message != null)
                dbg(null, new EventArg<string>(message));
        }
    }

    public struct Presets {
        #region MMCPresets enum

        public enum MMCPresets {
            SystemOnly,
            SystemOnlyEX,
            MUOnly,
            MUOnlyEX,
            XeLL,
            XeLLEX,
            Full
        }

        #endregion

        #region SPIPresets enum

        public enum SPIPresets {
            Auto,
            BigBlockSystemOnly,
            BigBlockMemoryUnit
        }

        #endregion

        public readonly long Start;
        public readonly long End;

        private Presets(MMCPresets mmc) {
            switch(mmc) {
                case MMCPresets.SystemOnly:
                    Start = 0;
                    End = 0x18000;
                    break;
                case MMCPresets.SystemOnlyEX:
                    Start = 0;
                    End = 0x3000000;
                    break;
                case MMCPresets.MUOnly:
                    Start = 0x18000;
                    End = 0;
                    break;
                case MMCPresets.MUOnlyEX:
                    Start = 0x3000000;
                    End = 0;
                    break;
                case MMCPresets.Full:
                    Start = 0;
                    End = 0;
                    break;
                case MMCPresets.XeLL:
                    Start = 0;
                    End = 0xA00;
                    break;
                case MMCPresets.XeLLEX:
                    Start = 0;
                    End = 0x140000;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mmc");
            }
        }

        private Presets(SPIPresets spi) {
            switch(spi) {
                case SPIPresets.Auto:
                    Start = 0;
                    End = 0;
                    break;
                case SPIPresets.BigBlockSystemOnly:
                    Start = 0;
                    End = 0;
                    break;
                case SPIPresets.BigBlockMemoryUnit:
                    Start = 0;
                    End = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("spi");
            }
        }
    }
}