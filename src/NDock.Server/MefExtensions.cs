using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NDock.Server
{
    public static class MefExtensions
    {
        public static Type GetExportType<TExport, TMetadata>(this Lazy<TExport, TMetadata> lazyFactory)
        {
            var valueFactoryField = typeof(Lazy<TExport>)
                .GetField("m_valueFactory", BindingFlags.Instance | BindingFlags.NonPublic);

            var valueFactory = (Func<TExport>)valueFactoryField.GetValue(lazyFactory);
            var exportField = valueFactory.Target.GetType().GetField("export");
            var export = exportField.GetValue(valueFactory.Target) as Export;
            var memberInfo = (LazyMemberInfo)export.Definition.GetType().GetProperty("ExportingLazyMember").GetValue(export.Definition, null);
            return memberInfo.GetAccessors()[0] as Type;
        }
    }
}
