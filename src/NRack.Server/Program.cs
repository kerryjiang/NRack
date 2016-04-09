using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.ServiceProcess;
using System.Text;
using NRack.Base;
using NRack.Base.Config;
using NRack.Server.Isolation;
using NRack.Server.Service;


namespace NRack.Server
{
    public static partial class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            var isMono = NRackEnv.IsMono;

            //If this application run in Mono/Linux, change the control script to be executable
            if (isMono && Path.DirectorySeparatorChar == '/')
                ChangeScriptExecutable();

            if ((!isMono && !Environment.UserInteractive)//Windows Service
                || (isMono && !AppDomain.CurrentDomain.FriendlyName.Equals(Path.GetFileName(Assembly.GetEntryAssembly().CodeBase))))//MonoService
            {
                RunAsService();
                return;
            }

            string exeArg = string.Empty;

            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Welcome to NRack.Server!");

                Console.WriteLine("Please press a key to continue...");
                Console.WriteLine("-[r]: Run this application as a console application;");
                Console.WriteLine("-[i]: Install this application as a Windows Service;");
                Console.WriteLine("-[u]: Uninstall this Windows Service application;");

                while (true)
                {
                    exeArg = Console.ReadKey().KeyChar.ToString();
                    Console.WriteLine();

                    if (Run(exeArg, null))
                        break;
                }
            }
            else
            {
                exeArg = args[0];

                if (!string.IsNullOrEmpty(exeArg))
                    exeArg = exeArg.TrimStart('-');

                Run(exeArg, args);
            }
        }

        static void ChangeScriptExecutable()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NRack.sh");

            try
            {
                if (!File.Exists(filePath))
                    return;

                File.SetAttributes(filePath, (FileAttributes)((uint)File.GetAttributes(filePath) | 0x80000000));
            }
            catch
            {

            }
        }

        private static bool Run(string exeArg, string[] startArgs)
        {
            switch (exeArg.ToLower())
            {
                case ("i"):
                    SelfInstaller.InstallMe();
                    return true;

                case ("u"):
                    SelfInstaller.UninstallMe();
                    return true;

                case ("r"):
                    RunAsConsole();
                    return true;

                case ("c"):
                    RunAsController(startArgs);
                    return true;

                default:
                    Console.WriteLine("Invalid argument!");
                    return false;
            }
        }

        private static bool setConsoleColor;

        static void CheckCanSetConsoleColor()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.ResetColor();
                setConsoleColor = true;
            }
            catch
            {
                setConsoleColor = false;
            }
        }

        private static void SetConsoleColor(ConsoleColor color)
        {
            if (setConsoleColor)
                Console.ForegroundColor = color;
        }

        private static Dictionary<string, ControlCommand> m_CommandHandlers = new Dictionary<string, ControlCommand>(StringComparer.OrdinalIgnoreCase);

        private static void AddCommand(string name, string description, Func<IBootstrap, string[], bool> handler)
        {
            var command = new ControlCommand
            {
                Name = name,
                Description = description,
                Handler = handler
            };

            m_CommandHandlers.Add(command.Name, command);
        }

        static void RunAsConsole()
        {
            Console.WriteLine("Welcome to NRack.Server!");

            CheckCanSetConsoleColor();

            Console.WriteLine("Initializing...");

            IBootstrap bootstrap = BootstrapFactory.CreateBootstrap();

            if (!bootstrap.Initialize())
            {
                SetConsoleColor(ConsoleColor.Red);

                Console.WriteLine("Failed to initialize NRack.Server! Please check error log for more information!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Starting...");

            bootstrap.Start();

            Console.WriteLine("-------------------------------------------------------------------");

            foreach (var server in bootstrap.AppServers)
            {
                if (server.State == ServerState.Running)
                {
                    SetConsoleColor(ConsoleColor.Green);
                    Console.WriteLine("- {0} has been started", server.Name);
                }
                else
                {
                    SetConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("- {0} failed to start", server.Name);
                }
            }

            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------------");

            Console.ResetColor();
            Console.WriteLine("Enter key 'quit' to stop the NRack.Server.");

            RegisterCommands();

            ReadConsoleCommand(bootstrap);

            bootstrap.Stop();

            Console.WriteLine("The NRack.Server has been stopped!");
        }

        private static void RegisterCommands()
        {
            AddCommand("List", "List all server instances", ListCommand);
            AddCommand("Start", "Start a server instance: Start {ServerName}", StartCommand);
            AddCommand("Stop", "Stop a server instance: Stop {ServerName}", StopCommand);
        }

        private static void RunAsController(string[] arguments)
        {
            if (arguments == null || arguments.Length < 2)
            {
                Console.WriteLine("Invalid arguments!");
                return;
            }

            var config = ConfigurationManager.GetSection("NRack") as IConfigSource;

            if (config == null)
            {
                Console.WriteLine("NRack configuration is required!");
                return;
            }

            var clientChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(clientChannel, false);

            IBootstrap bootstrap = null;

            try
            {
                var remoteBootstrapUri = string.Format("ipc://NRack.Bootstrap[{0}]/Bootstrap.rem", Math.Abs(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar).GetHashCode()));
                bootstrap = (IBootstrap)Activator.GetObject(typeof(IBootstrap), remoteBootstrapUri);
            }
            catch (RemotingException)
            {
                if (config.Isolation != IsolationMode.Process)
                {
                    Console.WriteLine("Error: the NRack.Server has not been started!");
                    return;
                }
            }

            RegisterCommands();

            var cmdName = arguments[1];

            ControlCommand cmd;

            if (!m_CommandHandlers.TryGetValue(cmdName, out cmd))
            {
                Console.WriteLine("Unknown command");
                return;
            }

            try
            {
                if (cmd.Handler(bootstrap, arguments.Skip(1).ToArray()))
                    Console.WriteLine("Ok");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed. " + e.Message);
            }
        }

        static bool ListCommand(IBootstrap bootstrap, string[] arguments)
        {
            foreach (var s in bootstrap.AppServers)
            {
                Console.WriteLine("{0} - {1}", s.Name, s.State);
            }

            return false;
        }

        static bool StopCommand(IBootstrap bootstrap, string[] arguments)
        {
            var name = string.Empty;

            if(arguments.Length > 1)
            {
                name = arguments[1];
            }

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Server name is required!");
                return false;
            }

            var server = bootstrap.AppServers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (server == null)
            {
                Console.WriteLine("The server was not found!");
                return false;
            }

            server.Stop();

            return true;
        }

        static bool StartCommand(IBootstrap bootstrap, string[] arguments)
        {
            var name = string.Empty;

            if (arguments.Length > 1)
            {
                name = arguments[1];
            }

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Server name is required!");
                return false;
            }

            var server = bootstrap.AppServers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (server == null)
            {
                Console.WriteLine("The server was not found!");
                return false;
            }

            server.Start();

            return true;
        }

        static void ReadConsoleCommand(IBootstrap bootstrap)
        {
            var line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                ReadConsoleCommand(bootstrap);
                return;
            }

            if ("quit".Equals(line, StringComparison.OrdinalIgnoreCase))
                return;

            var cmdArray = line.Split(' ');

            ControlCommand cmd;

            if (!m_CommandHandlers.TryGetValue(cmdArray[0], out cmd))
            {
                Console.WriteLine("Unknown command");
                ReadConsoleCommand(bootstrap);
                return;
            }

            try
            {
                if(cmd.Handler(bootstrap, cmdArray))
                    Console.WriteLine("Ok");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed. " + e.Message + Environment.NewLine + e.StackTrace);
            }

            ReadConsoleCommand(bootstrap);
        }

        static void RunAsService()
        {
            var currentDomain = AppDomain.CurrentDomain;

#pragma warning disable 0618 // Type or member is obsolete
            currentDomain.SetCachePath(Path.Combine(Path.Combine(currentDomain.BaseDirectory, IsolationAppConst.ShadowCopyDir), "Bootstrap"));
            currentDomain.SetShadowCopyFiles();
#pragma warning restore 0618 // Type or member is obsolete

            ServiceBase[] servicesToRun;
            servicesToRun = new ServiceBase[] { new NRackService() };
            ServiceBase.Run(servicesToRun);
        }
    }
}