using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRack.Base.Config;

namespace NRack.Server.Config
{
    public class RecycleTriggerConfig : ConfigurationElementBase
    {
        public RecycleTriggerConfig()
            : base(true)
        {

        }

        [ConfigurationProperty("type", IsRequired = false)]
        public string Type
        {
            get { return this["type"] as string; }
        }
    }

    [ConfigurationCollection(typeof(RecycleTriggerConfig), AddItemName = "trigger")]
    class RecycleTriggerConfigCollection : GenericConfigurationElementCollectionBase<RecycleTriggerConfig>
    {

    }
}
