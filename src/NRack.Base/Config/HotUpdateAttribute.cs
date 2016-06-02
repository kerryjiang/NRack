using System;

namespace NRack.Base.Config
{
    /// <summary>
    /// the attribute to mark which property of ServerConfig support hot update
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HotUpdateAttribute : Attribute
    {

    }
}
