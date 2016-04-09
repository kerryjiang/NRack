using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32.SafeHandles;
using NDock.Base;

namespace NDock.Server
{
    class ProcessLocker
    {
        private string m_LockFilePath;

        public ProcessLocker(string workDir, string lockFileName)
        {
            m_LockFilePath = Path.Combine(workDir, lockFileName);
        }

        public Process GetLockedProcess()
        {
            if (!File.Exists(m_LockFilePath))
                return null;

            int processId;

            var lockFileText = File.ReadAllText(m_LockFilePath);

            var lockFileInfoArray = lockFileText.Split(',');

            if (!int.TryParse(lockFileInfoArray[0], out processId))
            {
                File.Delete(m_LockFilePath);
                return null;
            }

            try
            {
                var process = Process.GetProcessById(processId);

                var safeInputHandle = new SafeFileHandle(new IntPtr(long.Parse(lockFileInfoArray[1])), true);

                var standardInput = new StreamWriter(new FileStream(safeInputHandle, FileAccess.Write, 4096, false), Encoding.UTF8, 4096);
                standardInput.AutoFlush = true;

                var standInputFieldName = NDockEnv.IsMono ? "input_stream" : "standardInput";

                var standInputField = process.GetType().GetField(standInputFieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.GetField);

                standInputField.SetValue(process, standardInput);

                return process;
            }
            catch
            {
                File.Delete(m_LockFilePath);
                return null;
            }
        }

        public void SaveLock(Process process)
        {
            var inputHandle = (process.StandardInput.BaseStream as FileStream)
                .SafeFileHandle
                .DangerousGetHandle().ToInt64();

            File.WriteAllText(m_LockFilePath, string.Format("{0},{1}", process.Id, inputHandle));
        }

        public void CleanLock()
        {
            if (File.Exists(m_LockFilePath))
                File.Delete(m_LockFilePath);
        }

        ~ProcessLocker()
        {
            CleanLock();
        }
    }
}

