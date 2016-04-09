using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Server;
using NDock.Server.Isolation;
using NDock.Server.Isolation.ProcessIsolation;

namespace NDock.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length != 1)
                throw new ArgumentException("Arguments number doesn't match!", "args");

            var name = args[0];

            if (string.IsNullOrEmpty(name))
                throw new Exception("Name cannot be null or empty.");

            name = name.Trim('"');

            var channelPort = string.Format(ProcessAppConst.PortNameTemplate, name, Process.GetCurrentProcess().Id);

            var currentDomain = AppDomain.CurrentDomain;

#pragma warning disable 0618 // Type or member is obsolete
            currentDomain.SetCachePath(Path.Combine(Path.Combine(currentDomain.BaseDirectory, IsolationAppConst.ShadowCopyDir), name));
            currentDomain.SetShadowCopyFiles();
#pragma warning restore 0618 // Type or member is obsolete

            var root = Path.Combine(Path.Combine(currentDomain.BaseDirectory, ProcessAppConst.WorkingDir), name);

            //Hack to change the default AppDomain's root
            if (NDockEnv.IsMono) //for Mono
            {
                var pro = typeof(AppDomain).GetProperty("SetupInformationNoCopy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
                var setupInfo = (AppDomainSetup)pro.GetValue(currentDomain, null);
                setupInfo.ApplicationBase = root;
            }
            else // for .NET
            {
                currentDomain.SetData("APPBASE", root);
            }

            currentDomain.SetData(typeof(IsolationMode).Name, IsolationMode.Process);

            try
            {
                var serverChannel = new IpcServerChannel("IpcAgent", channelPort, new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full });
                var clientChannel = new IpcClientChannel();
                ChannelServices.RegisterChannel(serverChannel, false);
                ChannelServices.RegisterChannel(clientChannel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(ManagedAppWorker), ProcessAppConst.WorkerRemoteName, WellKnownObjectMode.Singleton);
                
                Console.WriteLine("Ok");

                var line = Console.ReadLine();

                while (!"quit".Equals(line, StringComparison.OrdinalIgnoreCase))
                {
                    line = Console.ReadLine();
                }
            }
            catch
            {
                Console.Write("Failed");
            }
        }
    }
}
