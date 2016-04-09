using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.ServiceProcess;

namespace NDock.Server.Service
{
    [RunInstaller(true)]
    public partial class NDockServiceInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public NDockServiceInstaller()
        {
            InitializeComponent();

            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = ConfigurationManager.AppSettings["ServiceName"];

            var serviceDescription = ConfigurationManager.AppSettings["ServiceDescription"];
            if (!string.IsNullOrEmpty(serviceDescription))
                serviceInstaller.Description = serviceDescription;

            var servicesDependedOn = new List<string> { "tcpip" };
            var servicesDependedOnConfig = ConfigurationManager.AppSettings["ServicesDependedOn"];

            if (!string.IsNullOrEmpty(servicesDependedOnConfig))
                servicesDependedOn.AddRange(servicesDependedOnConfig.Split(new char[] { ',', ';' }));

            serviceInstaller.ServicesDependedOn = servicesDependedOn.ToArray();

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
}