using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;

using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Expose the properties of an object as IDictionary-string,object-. Changes to the dictionary
    /// (if allowed) will affect the underlying bound object. Not much going on here - this is really
    /// a composite if IQDynamicObject and PropertyDictionaryAdapter.
    /// 
    /// </summary>
    public class DynamicAdapter : IQDynamicObject,
        IDictionary<string, object>
    {
       
        public DynamicAdapter(object obj, IMapOptions options=null):
            base()
        {
            // do not call base- we want to create the dictionary ourselves, so we also need to configure opts.
            Options = MapOptions.From(options);
            InnerAdapter = new PropertyDictionaryAdapter(obj, Options);
            InnerDict = InnerAdapter;
        }

        protected PropertyDictionaryAdapter InnerAdapter;

    }


}
