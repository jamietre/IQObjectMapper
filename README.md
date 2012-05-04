### IQObjectMapper ###

A C# reflection library providing adapters to access typed objects as `IDictionary<string,object>` and other common interfaces 

#### In A Nutshell ####

This tool makes common reflection tasks easier by exposing object's properties using familiar interfaces.

    var instance = new ConcreteClass {
    	StringProp = "String value",
    	DoubleProp = 1.23
    };

Create a dictionary adapter for `instance`:

    IDictionary<string,object> dict = ObjectMapper.AsDictionary(instance);

You now have read/write access to properties & fields via the dictionary. Case senstitivity, what's included (e.g. fields, properties, visibility, etc.) are configurable.
   
    Assert.AreEqual("String value",dict["stringprop"]);

    dict["doubleprop"] = 3.14;
    Assert.AreEqual(3.14,instance.DoubleProp);


#### What can I do with it? ####

If you use Javascript, you are familiar with the concept of "objects as collections." There are many situations where it's useful to be able to quickly look through the properties of an object. 

    
    minValueNames=[];

    foreach (var propName in options) { 
        if (propName.length > 3 && 
            propName.substring(0,3)=='min') {
            minValueName.push(propName.substring(3));
            options[propName]+=1;  // add one to the minimum value
        }
     }

In C#, we don't do things like this because it's hard. We generally use other data structures like dictionaries and lists. But there are many times when you want the benefits and security of a strongly-typed object for some situations, but the flexibility to handle the same object using another more flexible data structure in others. And it's not a coincidence that I bring Javascript into this. One of the more common "other" reasons why you might want to do this is to simplify passing data back and forth to Javascript. It's common for Javascript libraries to accept and return objects with dynamic structures. It can be difficult to facilitate this data exchange using strongly typed objects in C#. 

Many are used to dealing with by using dynamic types, but then the advantage of strong typing is lost. Alternatively, you may end up writing lots of code to map less-structured data from a Javascript client to C# strongly-type structures and vice versa. 

Beyon that, there are plenty of other situations where an easy mapping from basic data types to strongly typed object would simplify code. The goal of this project is to provide tools to perform this common, yet difficult task using standardized interfaces and objects that are familiar to most C# programmers. 

Using IQObjectMapper, our Javascript example above becomes:

    var minValues = new List<string>();
    var dict = ObjectMapper.AsDictionary(myObject);

    foreach (var prop in dict) {
        if (prop.Key.Length>3 && 
            prop.Key.Substring(0,3)=="min") {
                minValues.Add(prop.Key.Substring(3));
            }
            dict[prop.Key]=0;
        }
    }

Or alternatively, using LINQ (with the same setup of `minValues` and `dict`):

    var minValues = dict
        .Where(item => item.Key.Substring(0, 3) == "min")
        .Select(item =>
        {
            dict[item.Key] = ((int)item.Value) + 1;
            return item.Key.Substring(3);
        });

OK, the LINQ version isn't really any shorter, but it looks cooler. 

In both cases, we're enumerating the properties of a "plain old CLR object" or POCO, accessing their values, adding them to a list conditionally, and updating data.
     

#### Map concrete classes to flexible data structures.    ####

    var instance = new SomeClass {
    	StringProp = "String value",
    	DoubleProp = 1.23
    };

    IDictionary<string,object> dict= ObjectMapper.AsDictionary(instance);
    dynamic dyn = ObjectMapper.AsDynamic(instance);
    
Read and write to the object's properties, or add new "properties" as permitted by the mapped data structure. (New properties won't actually be added to the underlying class, of course, but as long as you are using the adapter they will be treated the same as real properties).

    dict["StringProp"] == "String value"            // true
    dict.Count == 2                                 // true

Access is not cases sensitive by default, but can be changed with `MapOptions.CaseSensitive` at a global or instance level.

    dict["stringprop"] = "New string value";        // updates instance.StringProp. 

The dictionary is writable unless `CanAlterProperties=false`. Of course new properties have no effect on the POCO that is bound.

    dict["newprop"] = 12345;                        // adds to the dictionary
    dict.Count == 3                                 // true
    
Same thing if you're using a dynamic object.

    dyn.NewProp = 12345;                            // creates a new property    

#### Map dictionary-like structures back to an object ####

If you started with key/value pairs, you can map it back to an object.

    var kvps = new List<KeyValuePair<string,object>> {
        new KeyValuePair<string,object>("StringProp","some new data"),
	    new KeyValuePair<string,object>("DoubleProp",2)
    };


The data structure `IEnumerable<KeyValuePair<string,object>>` is accepted for most mapping operations that target objects. This structure is used because it's exposed by dictionaries and ExpandoObjects. It's also the default object type for most JSON deserializers.

    var newClass = ObjectMapper.ToNew<SomeClass>(kvps);

You can update an existing instance.

    ObjectMapper.ToExisting(kvps,existingClass);


#### Lower level access ###

It's easy to use the internal structures to provide easy reflections access and take advantage of IQObjectMappers caching and data translation capabilities. The IClassInfo object provides a useful subset of information that the .NET reflection objects provide, and provides consistent access to fields, properties, and anonymous types.

Each class is mapped the first time it's accessed and the metadata cached in memory. Fast, typed access methods are used for fields and properties. Performance is about 2x the cost of a direct property access, which should be plenty fast for just about anything. When using wrapper data structures this may be worse (e.g. the Dictionary wrapper is about 4x). Bear in mind that even a 4x as long as a direct access, you can do hundreds of millions of reads/writes in a second. This should not in any way impact an application's performance and is far better than using conventional methods of reflection.


    IClassInfo info = GetClassInfo(newClass);
      
    info["stringprop"].IsPrivate == false               // true
    info["doubleprop"].IsNullable == false              // true
      
If you need to optimize performance for a long loop, you can access the delegates directly.

    IDelegateInfo dblPropInfo = info["doubleprop"];  
    dblProp.SetValue(newClass,21.22);
    Console.Write(dblProp.GetValue(newClass));

You can easily change what is reflected with a number of options.

    IClassInfo info = GetClassInfo(newClass, new MapOptions( { IncludeFields: false, IncludePrivate: true });


#### DataReaders ####

Adapters specific to DataReaders are provided. This maps the current row to a dictionary:

    IDictionary<string,object> dict = ObjectMapper.ToDictionary(reader);
    


### API ###

The library contains a number of objects that can be instantiated directly, but most common uses are available through static methods of the `ObjectMapper` singleton.

Generally: 

Each method has an optional parameter `IMapOptions` which will override the global settings.
The data structure `IEnumerable<KeyValuePair<string,object>>` is what I mean when I say "dictionary-like" because it's hard to keep typing (and reading) the full name. This type can be assigned to by `IDictionary<string,object>` and dynamic objects (which use this structure internally). It's a good data structure for representing a set of properties, and is very common, so it's useful as the basis for interchange. 

##### Methods that return a new object #####

Returns a dynamic (expando) object given a concrete class instance or a dictionary-like object

    dynamic ToDynamic(object source);

Return a new dynamic object of type T given a concrete class instance or a dictionary-like object

	T ToDynamic<T>(object source, bool deep = false) where T : IDynamicMetaObjectProvider

Return a new dictionary populated from the properties of the concrete object

	IDictionary<string, object> ToDictionary(object source, bool deep = false)

Return a new concrete object of a specific type based on the values of the dictionary-like source

    object ToNew(IEnumerable<KeyValuePair<string, object>> source, Type type)
    T ToNew<T>(IEnumerable<KeyValuePair<string, object>> source)

Given a sequence of dictionary-like objects, return a sequence of strongly-typed instances

    IEnumerable<T> ToTypedSequence<T>(IEnumerable<IEnumerable<KeyValuePair<string,object>>> source)


##### Methods that update existing objects #####

Populate an existing concrete obejct from the dictionary-like source

	void ToExisting(IEnumerable<KeyValuePair<string, object>> source, object target)
	void ToExisting<T>(IEnumerable<KeyValuePair<string, object>> source, T target)

##### Methods that return two-way adapters (e.g. changes to the returned object are reflected in the source) #####
	
	IDynamicMetaObjectProvider AsDynamic(object source)
	IDynamicMetaObjectProvider AsDynamic<T>(object source) where T: IDynamicMetaObjectProvider
    IDictionary<string, object> AsDictionary(object source)
	
##### Other Methods #####

Parse value-like data into a value type. Value-like data is a value type, a string that can be parsed
into a value type, or an array or list.

	T ParseValue<T>(object source) 
	object ParseValue(object source, Type type)

Methods for mapping DataReaders to useful formats.

	IEnumerable<IDictionary<string, object>> ToDictionarySequence(IDataReader reader)
    IDictionary<string, object> ToDictionary(IDataRecord reader)

