# ManualReserialization

Update any `MonoBehaviour`, `ScriptableObject`, `SerializedClass` easily and quickly.

```csharp
public class Something : MonoBehaviour
{
    //We want to deprecate this
    public int someField;

    //and move the int to a boolean
    public bool newField;

    [MenuItem("Tools/Reserialize Something")]
    public static void Reserialize()
    {
        Reserializer.Reserialize<Something>(x => x.newField = x.someField == 1, new [] {"someField"});
    }
}
```


On this example you can see that you can quickly refactor the value of all the assets in your asset database that use the `Something` type.
Just call the `Reserializer.Reserialize` method with the generic type desired, provide the reserializing functionality and refer to the path (dot separated if nested) inside the type where you will change data.

The reserializing action is self explanatory, it will run and do the change you want.
The path is a bit trickyer. When the element you are reserializing is a prefab things get complicated. It might have prefab variants, might be a scene instance, etc. The path allows to just call the reserializing action to the places where the prefab modifications are actually changing those fields. For example if for a prefab X there is 2 variants Y and Z but only Z changes the field we want to reserialize, the reserializer will only call the action on Z and ignore Y.

See the tests for more in depth examples.


## License

[MIT](LICENSE.md) © [Pere Viader](https://github.com/PereViader)
