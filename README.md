# ManualReserialization

This library allows you to programatically update the all assets in your project when the structure of your serialized fields change.

Regardless of where those assets are found within your asset database the reserialization will be applied to them allowing you to update programatically update the shape of your objects.

```csharp
public class SomeClass : MonoBehaviour
{
    [Obsolete] public int someField;
    public bool newField;

    [MenuItem("Tools/Reserialize the class")] //You can add a menu item to be able to run this snippet of code from the UnityEditor
    public static void Reserialize()
    {
        Reserializer.Reserialize<SomeClass>(x => x.newField = x.someField == 1, "someField");
    }
}
```


On the example above, we want to update an obsolete `int` field to a `bool` one.
This is quickly accomplished with the library. Calling the `Reserializer.Reserialize` method with the type we want to update.
The first parameter provided is the delegate that will apply the changes necessary on the instance.
The second parameter is the dot separated path of the serialized properties that are used on the delegate.
This second parameter is necessary to only apply the delegate on the necessary prefab variants. Only prefab variants that have property overrides applied to the properties you want to update should have the delegate applied to them. Otherwise unnecesary modifications would be done on prefab variants.

In the example above the path is simple. We just need to provide the name of the property without any dots.

However in the case below

```csharp

[Serializable]
public class Nested
{
    [Obsolete] public int someField;
    public bool newField;
}

public class Parent
{
    public Nested nested;
}
```

if we wanted to apply the same and we were doing it on Parent like this 

```
Reserializer.Reserialize<Parent>(x => x.nested.newField = x.nested.someField == 1, "nested.someField");
```

Notice that the path now has the full path from the class defined on the Reserialize method until the nested property we are getting the data from.

However unless you only want to apply changes on instances of `Parent` this should really be implemented as

```
Reserializer.Reserialize<Nested>(x => x.newField = x.someField == 1, "someField");
```


## License

[MIT](LICENSE.md) © [Pere Viader](https://github.com/PereViader)
