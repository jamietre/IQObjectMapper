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
        if (prop.Length>3 && 
            prop.Substring(0,3)=="min") {
                minValues.Add(prop.Substring(3));
            }
            dict[prop]=((int)dict[prop])+1;
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

#### Options ####

Options can be set globally using the `ObjectMapper.DefaultOptions` object. Different options can be passed to most objects and methods to override the globally-defined behavior.

When you create a new instance of the `MapperOptions` object, it will inherit the global settings automatically. So it's easy to override the settings for any option by 
creating a new instance of `MapperOptions` and changing it:

    // map someInstance to a dictionary, but exclude fields (e.g. only include properties)

    var myDict = ObjectMapper.AsDictionary(someInstance, new MapOptions {
	    IncludeFields: false;
	});

The following options are available. Not all options apply to every method.  The default value is shown with each.

##### Reflection options:

These determine which properties are exposed to any client.

    IncludeProperties = true            // property type members are included
    IncludeFields = true                // field type members are included
    IncludePrivate = false;             // entities protected or private getters are not exposed
    DeclaredOnly = false;               // properties/fields defined in an ancestor are not exposed
    CaseSensitive = false;              // access via most methods is not case sensitive. Be sure you don't have conflicting entities.
    
##### Other options
	
	DynamicObjectType =                 // the type of dynamic object created by methods that return a new dynamic object.
	    typeof(IQDynamicObject);        // Some access control options will only work if you use this type.
    FailOnMismatchedTypes = true;       // when attempting to map data to an incompatible type, throw an error (true) or fail silently
    CanAlterProperties = true;          // For dictionary access to a concrete object, items can be added and removed
    CanAccessMissingProperties = true;  // For dictionary or dynamic access, reading a missing property will return UndefinedValue
    UndefinedValue = Undefined.Value;   // The value returned when accessing a missing property
    
	ParseValues = false;                // When assigning data to an incompatible type, attempt to parse or coerce it. Does stuff like
	                                    // converts "true" to (bool)true.
    UpdateSource = true;                // when using dictionary or dynamic adapters, changes to the data should update the underlying object.
    IsReadOnly = false;                 // when true, no changes are permitted via the adapter.

#### Performance

I've used just about every trick in the book to make this as fast as possible. The slowest methods are the dynamic and dictionary adapters. In a very simple test scenario (see "Perform" in the unit tests) these are approximately 14 times slower than raw access.

That sounds terrible, but remember that it's 14 times slower than pretty much instantaneous. On my laptop, I can read and write two properties (one string, one double) a thousand times in 2 milliseconds.  That's a million complete read + write operations in a second, and that includes overhead of creating unique test values in each loop iteration. 

At the same time, using the easily accessible delegates for reading and writing that are created by this library, you can expect performance that is extremely fast - between 1.5 and 2x native access. it's easy to do this too, see "If you need to optimize performance..." above under "Lower Level Access."

Here are simple figures comparing different types of access. Each tests performs two reads, two writes, and a little bit of math to ensure different values are used.

#### Sample performance data

Each test shows time in milliseconds for 1000 iterations doing two reads, two writes, and a simple calculation to build a unique string & double in each itereation.

Test 1: no optimizing (looking up the delegate from the class type in each loop iteration) versus native property access

    Time 1: 2.153, Time 2: 0.167, Ratio: 12.8922155688623

Test 2: ClassInfo not looked up, but delegates looked up at each loop iteraation (simulates use where property name may be unique for each iteration) vs. native

    Time 1: 0.307, Time 2: 0.172, Ratio: 1.78488372093023

Test 3: Delegates cached outside loop - call is made directly to the property delegate vs. native

	Time 1: 0.238, Time 2: 0.169, Ratio: 1.40828402366864

Test 4: Same as test 3, but with fields instead of properties being tested

    Time 1: 0.296, Time 2: 0.154, Ratio: 1.92207792207792

Test 5: Dictionary property read/write access vs. native

    Time 1: 2.113, Time 2: 0.152, Ratio: 13.9013157894737

Test 6: Dictionary property read/write vs. native Dictionary<string,object>

    Time 1: 2.107, Time 2: 0.385, Ratio: 5.47272727272727

Test 7: Native dynamic object vs. native (for reference)

	Time 1: 0.373, Time 2: 0.172, Ratio: 2.16860465116279

Test 8: DynamicAdapter vs. native
    
     Time 1: 2.43, Time 2: 0.172, Ratio: 14.1279069767442

Test 9: DynamicAdapter vs. native dynamic object

     Time 1: 2.434, Time 2: 0.355, Ratio: 6.85633802816901

