using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Metadata;

namespace NDock.Server.Utils
{
    class ProcessPerformanceCounter
    {
        private PerformanceCounterInfo[] m_CounterDefinitions;
        private PerformanceCounter[] m_Counters;
        private Process m_Process;
        private bool m_CollectThreadPoolInfo;

        public ProcessPerformanceCounter(Process process, PerformanceCounterInfo[] counters)
            : this(process, counters, true)
        {

        }

        public ProcessPerformanceCounter(Process process, PerformanceCounterInfo[] counters, bool collectThreadPoolInfo)
        {
            m_Process = process;
            m_CounterDefinitions = counters;
            m_CollectThreadPoolInfo = collectThreadPoolInfo;

            //Windows .Net, to avoid same name process issue
            if (!NDockEnv.IsMono)
                RegisterSameNameProcesses(process);

            SetupPerformanceCounters();
        }

        private void RegisterSameNameProcesses(Process process)
        {
            foreach (var p in Process.GetProcessesByName(process.ProcessName).Where(x => x.Id != process.Id))
            {
                p.EnableRaisingEvents = true;
                p.Exited += new EventHandler(SameNameProcess_Exited);
            }
        }

        //When find a same name process exit, re-initialize the performance counters
        //because the performance counters' instance names could have been changed
        void SameNameProcess_Exited(object sender, EventArgs e)
        {
            SetupPerformanceCounters();
        }

        private void SetupPerformanceCounters()
        {
            var isUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
            var instanceName = (isUnix || NDockEnv.IsMono) ? string.Format("{0}/{1}", m_Process.Id, m_Process.ProcessName) : GetPerformanceCounterInstanceName(m_Process);

            if (string.IsNullOrEmpty(instanceName))
                return;

            SetupPerformanceCounters(instanceName);
        }

        private void SetupPerformanceCounters(string instanceName)
        {
            m_Counters = new PerformanceCounter[m_CounterDefinitions.Length];

            for (var i = 0; i < m_CounterDefinitions.Length; i++)
            {
                var counterInfo = m_CounterDefinitions[i];
                m_Counters[i] = new PerformanceCounter(counterInfo.Category, counterInfo.Name, instanceName);
            }
        }

        //Tt is only used in windows
        private static string GetPerformanceCounterInstanceName(Process process)
        {
            var processId = process.Id;
            var processCategory = new PerformanceCounterCategory("Process");
            var runnedInstances = processCategory.GetInstanceNames();

            foreach (string runnedInstance in runnedInstances)
            {
                if (!runnedInstance.StartsWith(process.ProcessName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (process.HasExited)
                    return string.Empty;

                using (var performanceCounter = new PerformanceCounter("Process", "ID Process", runnedInstance, true))
                {
                    if ((int)performanceCounter.RawValue == processId)
                    {
                        return runnedInstance;
                    }
                }
            }

            return process.ProcessName;
        }

        public void Collect(StatusInfoCollection statusCollection)
        {
            if(m_CollectThreadPoolInfo)
            {
                int availableWorkingThreads, availableCompletionPortThreads;
                ThreadPool.GetAvailableThreads(out availableWorkingThreads, out availableCompletionPortThreads);

                int maxWorkingThreads;
                int maxCompletionPortThreads;
                ThreadPool.GetMaxThreads(out maxWorkingThreads, out maxCompletionPortThreads);

                statusCollection[StatusInfoKeys.AvailableWorkingThreads] = availableWorkingThreads;
                statusCollection[StatusInfoKeys.AvailableCompletionPortThreads] = availableCompletionPortThreads;
                statusCollection[StatusInfoKeys.MaxCompletionPortThreads] = maxCompletionPortThreads;
                statusCollection[StatusInfoKeys.MaxWorkingThreads] = maxWorkingThreads;
            }

            var retry = false;

            while (true)
            {
                try
                {
                    for (var i = 0; i < m_CounterDefinitions.Length; i++)
                    {
                        var counterInfo = m_CounterDefinitions[i];
                        var counter = m_Counters[i];
                        statusCollection[counterInfo.StatusInfoKey] = counterInfo.Read(counter.NextValue());
                    }

                    break;
                }
                catch (InvalidOperationException e)
                {
                    //Only re-get performance counter one time
                    if (retry)
                        throw e;

                    //Only re-get performance counter for .NET/Windows
                    if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX || NDockEnv.IsMono)
                        throw e;

                    //If a same name process exited, this process's performance counters instance name could be changed,
                    //so if the old performance counter cannot be access, get the performance counter's name again
                    var newInstanceName = GetPerformanceCounterInstanceName(m_Process);

                    if (string.IsNullOrEmpty(newInstanceName))
                        break;

                    SetupPerformanceCounters(newInstanceName);
                    retry = true;
                }
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < m_Counters.Length; i++)
            {
                var counter = m_Counters[i];

                if (counter != null)
                {
                    counter.Dispose();
                    m_Counters[i] = null;
                }
            }

            m_Counters = null;
        }
    }
}
