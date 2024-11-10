[![Publish Release](https://github.com/PereViader/ManualReserialization/actions/workflows/PublishRelease.yml/badge.svg)](https://github.com/PereViader/ManualReserialization/actions/workflows/PublishRelease.yml) ![Unity version 2021.3](https://img.shields.io/badge/Unity-2021.3-57b9d3.svg?style=flat&logo=unity)

# ManualReserialization

This library allows you to programatically update the all assets in your project when the structure of your serialized fields change.

Regardless of where those assets are found within your asset database* the reserialization will be applied to them allowing you to programatically update the shape of your objects.

*`SerializeReference` is not compatible but could be implemented

```csharp
public class SomeClass : MonoBehaviour
{
    [Obsolete] public int someField;
    public bool newField;

    [MenuItem("Tools/Reserialize the class")] //You can add a menu item to be able to run this snippet of code from the UnityEditor
    public static void Reserialize()
    {
        Reserializer.Reserialize<SomeClass>((x, metadata) => x.newField = x.someField == 1);
    }
}
```

The metadata provided can be inspected to see if the instance of the type comes from either a prefab, scriptable object or a scene

When implementing the delegate, make sure that it is [idempotent](https://en.wikipedia.org/wiki/Idempotence). This is necessary because if the reserialization is applied to a root prefab and also to prefab variants, it should only produce overrides when actually necessary.

This is an example of non idempotent implementation

```csharp
//Before running
//RootPrefab: someField == 1
//PrefabVariant: without override thus value == 1

Reserializer.Reserialize<SomeClass>((x, metadata) => x.someField++);

//After running
//RootPrefab: someField == 2
//PrefabVariant override with someField == 3
```

The variant ends up with an override of value 3 because the delegate will run after the original prefab has had the delegate run on it. Thus the root prefab is updated from 1 to 2 and then the variant is updated from 2 to 3.

