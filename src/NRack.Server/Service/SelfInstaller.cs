using System;
using System.Configuration.Install;
using System.Reflection;

namespace NDock.Server.Service
{
    public static class SelfInstaller
    {
        private static readonly string _exePath = Assembly.GetEntryAssembly().Location;

        public static bool InstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { _exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UninstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", _exePath });
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
