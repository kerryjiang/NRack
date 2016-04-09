using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Server
{
    class ManagedAppGroup : IManagedApp
    {
        public string Name { get; private set; }

        public IManagedApp[] Items { get; private set; }

        public ManagedAppGroup(string name, IEnumerable<IManagedApp> groupItems)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (groupItems == null || !groupItems.Any())
                throw new ArgumentNullException("name");

            Name = name;
            Items = groupItems.ToArray();
        }

        public bool Setup(IBootstrap bootstrap, IServerConfig config)
        {
            Config = config;

            foreach (var item in Items)
            {
                if (!item.Setup(bootstrap, config))
                    return false;
            }

            return true;
        }

        public bool Start()
        {
            foreach (var item in Items)
            {
                if (!item.Start())
                    return false;
            }

            return true;
        }

        public void Stop()
        {
            foreach (var item in Items)
            {
                item.Stop();
            }
        }


        public IServerConfig Config { get; private set; }

        public AppServerMetadata GetMetadata()
        {
            throw new NotImplementedException();
        }


        public ServerState State { get; private set; }


        public bool CanBeRecycled()
        {
            return true;
        }

        public StatusInfoCollection CollectStatus()
        {
            throw new NotImplementedException();
        }

        public void ReportPotentialConfigChange(IServerConfig config)
        {
            foreach (var item in Items)
            {
                item.ReportPotentialConfigChange(config);
            }
        }
    }
}
