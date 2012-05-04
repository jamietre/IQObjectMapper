using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{

    public class DelegateAdapter : IEnumerable<IDelegateInfo>
    {
        public DelegateAdapter(Type type, IMapOptions options = null)
        {
            Type = type;
            Options = MapOptions.From(options);
        }

        protected Type Type;
        public MapOptions Options { get; set; }

        public IEnumerator<IDelegateInfo> GetEnumerator()
        {
            return ObjectMapper.MapperCache.GetClassInfo(Type,Options)
                .Data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }


}
