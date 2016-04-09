using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDock.Server.Isolation
{
    class AppAssemblyUpdateState
    {
        public DateTime CurrentAssemblyTime { get; private set; }

        public DateTime LastUpdatedTime { get; private set; }

        public DateTime LastCheckTime { get; set; }

        private string m_AppAssemblyDir;

        public AppAssemblyUpdateState(string dir)
        {
            m_AppAssemblyDir = dir;
            CurrentAssemblyTime = LastUpdatedTime = GetLastUpdateTime();
        }

        private DateTime GetLastUpdateTime()
        {
            var lastUpdatedTime = DateTime.MinValue;

            foreach(var file in Directory.GetFiles(m_AppAssemblyDir, "*.dll").Union(Directory.GetFiles(m_AppAssemblyDir, "*.exe")))
            {
                var fileWriteTime = File.GetLastWriteTime(file);

                if (fileWriteTime > lastUpdatedTime)
                    lastUpdatedTime = fileWriteTime;
            }

            return lastUpdatedTime;
        }

        public bool TryCheckUpdate()
        {
            var lastUpdatedTime = GetLastUpdateTime();

            if (lastUpdatedTime <= LastUpdatedTime)
                return false;

            LastUpdatedTime = lastUpdatedTime;
            LastCheckTime = DateTime.Now;
            return true;
        }
    }
}
