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
        Reserializer.Reserialize<SomeClass>(x => x.newField = x.someField == 1);
    }
}
```

When implementing the delegate, make sure that it is [idempotent](https://en.wikipedia.org/wiki/Idempotence). This is necessary because if the reserialization is applied to a root prefab and also to prefab variants, it should only produce overrides when actually necessary.

This is an example of non idempotent implementation

```csharp
//Before running
//RootPrefab: someField == 1
//PrefabVariant: without override thus value == 1

Reserializer.Reserialize<SomeClass>(x => x.someField++);

//After running
//RootPrefab: someField == 2
//PrefabVariant override with someField == 3
```

## License

[MIT](LICENSE.md) © [Pere Viader](https://github.com/PereViader)
