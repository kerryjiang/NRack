using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using NRack.Base;

namespace NRack.Server.Service
{
    partial class NRackService : ServiceBase
    {
        private IBootstrap m_Bootstrap;

        public NRackService()
        {
            InitializeComponent();
            ServiceName = ConfigurationManager.AppSettings["ServiceName"];
            m_Bootstrap = BootstrapFactory.CreateBootstrap();
        }

        protected override void OnStart(string[] args)
        {
            if (!m_Bootstrap.Initialize())
                return;

            m_Bootstrap.Start();
        }

        protected override void OnStop()
        {
            m_Bootstrap.Stop();
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            m_Bootstrap.Stop();
            base.OnShutdown();
        }
    }
}
