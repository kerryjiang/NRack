using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NDock.Server.Isolation
{
    /// <summary>
    /// AssemblyImport, used for importing assembly to the current AppDomain
    /// </summary>
    public class AssemblyImport : MarshalByRefObject
    {
        private string m_ImportRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyImport"/> class.
        /// </summary>
        public AssemblyImport(string importRoot)
        {
            m_ImportRoot = importRoot;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        //Process cannot resolved assemblies
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName name = new AssemblyName(args.Name);

            var assemblyFilePath = Path.Combine(m_ImportRoot, name.Name + ".dll");

            if(File.Exists(assemblyFilePath))
                return Assembly.LoadFrom(assemblyFilePath);

            assemblyFilePath = Path.Combine(m_ImportRoot, name.Name + ".exe");

            if (File.Exists(assemblyFilePath))
                return Assembly.LoadFrom(assemblyFilePath);

            assemblyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name.Name + ".dll");

            if (File.Exists(assemblyFilePath))
                return Assembly.LoadFrom(assemblyFilePath);

            assemblyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name.Name + ".exe");

            if (File.Exists(assemblyFilePath))
                return Assembly.LoadFrom(assemblyFilePath);

            return null;
        }

        /// <summary>
        /// Registers the assembply import.
        /// </summary>
        /// <param name="hostAppDomain">The host application domain.</param>
        public static void RegisterAssembplyImport(AppDomain hostAppDomain)
        {
            var assemblyImportType = typeof(AssemblyImport);

            hostAppDomain.CreateInstanceFrom(assemblyImportType.Assembly.CodeBase,
                    assemblyImportType.FullName,
                    true,
                    BindingFlags.CreateInstance,
                    null,
                    new object[] { AppDomain.CurrentDomain.BaseDirectory },
                    null,
                    new object[0]);
        }
    }
}
