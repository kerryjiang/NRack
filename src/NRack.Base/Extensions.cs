using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AnyLog;

namespace NRack.Base
{
    public static class Extensions
    {

        private const string CurrentAppDomainExportProviderKey = "CurrentAppDomainExportProvider";

        /// <summary>
        /// Gets the current application domain's export provider, if it doesn't exist, create one.
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        /// <returns></returns>
        public static CompositionContainer GetCurrentAppDomainExportProvider(this AppDomain appDomain)
        {
            var exportProvider = appDomain.GetData(CurrentAppDomainExportProviderKey) as CompositionContainer;

            if (exportProvider != null)
                return exportProvider;

            var isolation = IsolationMode.None;
            var isolationValue = appDomain.GetData(typeof(IsolationMode).Name);

            if (isolationValue != null)
                isolation = (IsolationMode)isolationValue;

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(IAppServer).Assembly));

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (Directory.Exists(baseDirectory))
                catalog.Catalogs.Add(new DirectoryCatalog(baseDirectory, "*.*"));


            if (isolation != IsolationMode.None)
            {
                catalog.Catalogs.Add(new DirectoryCatalog(Directory.GetParent(baseDirectory).Parent.FullName, "*.*"));
            }

            exportProvider = new CompositionContainer(catalog);

            appDomain.SetData(CurrentAppDomainExportProviderKey, exportProvider);
            return exportProvider;
        }

        public static IBootstrap GetBootstrap(this AppDomain appDomain)
        {
            return appDomain.GetData("Bootstrap") as IBootstrap;
        }

        /// <summary>
        /// Copies the properties of one object to another object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static T CopyPropertiesTo<T>(this T source, T target)
        {
            return source.CopyPropertiesTo(p => true, target);
        }

        /// <summary>
        /// Copies the properties of one object to another object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predict">The properties predict.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static T CopyPropertiesTo<T>(this T source, Predicate<PropertyInfo> predict, T target)
        {
            PropertyInfo[] properties = source.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            Dictionary<string, PropertyInfo> sourcePropertiesDict = properties.ToDictionary(p => p.Name);

            PropertyInfo[] targetProperties = target.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
                .Where(p => predict(p)).ToArray();

            for (int i = 0; i < targetProperties.Length; i++)
            {
                var p = targetProperties[i];
                PropertyInfo sourceProperty;

                if (sourcePropertiesDict.TryGetValue(p.Name, out sourceProperty))
                {
                    if (sourceProperty.PropertyType != p.PropertyType)
                        continue;

                    if (!sourceProperty.PropertyType.IsSerializable)
                        continue;

                    p.SetValue(target, sourceProperty.GetValue(source, null), null);
                }
            }

            return target;
        }

        /// <summary>
        /// Logs the aggregate exception.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void LogAggregateException(this ILog log, string message, AggregateException exception)
        {
            var sb = new StringBuilder();

            sb.AppendLine(message);

            foreach (var e in exception.InnerExceptions)
            {
                sb.AppendLine("#1: " + e.Message);
                sb.AppendLine(e.StackTrace);
            }

            log.Error(sb.ToString());
        }
    }
}
