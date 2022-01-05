using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to persist singletons and their gameobjects through scene transitions
// while still letting the object exist if just testing in play mode, and haven't loaded the object previously
// for ex, a Loading scene that prepares some persistent data objects, then moves to the main scene.
// normally a singleton's gameObject would then be created twice, or it would not exist in the main scene.

public class PersistentSingletonHelper : MonoBehaviour
{
    [Sirenix.OdinInspector.Required] public string uniqueKey;

    static List<string> createdObjects = new List<string>();

    private void Awake()
    {
        if (createdObjects.Contains(uniqueKey))
        {
            DestroyImmediate(gameObject);

        } else
        {
            DontDestroyOnLoad(gameObject);
            createdObjects.Add(uniqueKey);
        }
    }
}
