using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;

namespace NDock.Server
{
    class RemoteAppGroup : IRemoteApp
    {
        public string Name { get; private set; }

        public IRemoteApp[] Items { get; private set; }

        public RemoteAppGroup(string name, IEnumerable<IRemoteApp> groupItems)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (groupItems == null || !groupItems.Any())
                throw new ArgumentNullException("name");

            Name = name;
            Items = groupItems.ToArray();
        }

        public bool Setup(IServerConfig config, IServiceProvider serviceProvider)
        {
            Config = config;

            foreach (var item in Items)
            {
                if (!item.Setup(config, serviceProvider))
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
    }
}
