using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PoolManager : MonoBehaviour {

    #region Singelton
    public static PoolManager instance;

    private void Awake() {
        // Check if instance is already defined and if this gameObject is not the current instance.
        if (instance != null) {
            Debug.LogWarning("Multiple Instances of PoolManager found. Current instance was destroyed.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private Dictionary<int, Pool> poolDictionary = new Dictionary<int, Pool>();
    
    /// <summary>
    /// Instantiates and disables the given amount of prefabs and adds them into the poolDictionary.
    /// </summary>
    /// <param name="prefab">Prefab we want to save into our pool.</param>
    /// <param name="poolSize">Amount of instances of the prefab we want to save into our pool.</param>
    /// <param name="dynamicPooling">Defines if the poolSize should increase dynamically, if there are only visible instances of the given prefab to use.</param>
    public void CreatePool(GameObject prefab, int poolSize, bool dynamicPooling = false) {
        // Get InstanceID of the given gameObject.
        int poolKey = prefab.GetInstanceID();

        // Check if the poolDictionary doesn't already contain our prefab pool.
        if (!poolDictionary.ContainsKey(poolKey)) {
            // Add the InstanceID to the poolDictionary.
            poolDictionary.Add(poolKey, new Pool(dynamicPooling, new Queue<ObjectInstance>()));

            // Enqueue disabled gameObjects into the Queue equal to the poolSize.
            for (; poolSize > 0; --poolSize) {
                ObjectInstance newObject = new ObjectInstance(Instantiate(prefab) as GameObject);
                poolDictionary[poolKey].ObjectQueue.Enqueue(newObject);
            }
        }
    }

    /// <summary>
    /// Changes the parent of the pool with the given prefab.
    /// </summary>
    /// <param name="prefab">Pool we want to change the default parent at.</param>
    /// <param name="parent">Transform we want to parent our pool too.</param>
    public void ParentPool(GameObject prefab, Transform parent) {
        // Get InstanceID of the given gameObject.
        int poolKey = prefab.GetInstanceID();

        // Create new gameObject to store the pooled gameObjects.
        GameObject poolHolder = new GameObject(prefab.name + " Pool");
        poolHolder.transform.SetParent(parent);

        // Try to get the ObjectInstance queue from the given poolKey.
        if (poolDictionary.TryGetValue(poolKey, out Pool pool)) {
            // Loop through each objectInstance in the objectQueue.
            foreach (var objectInstance in pool.ObjectQueue) {
                // Set its parent equal to the newly created poolHolder.
                objectInstance.Transform.SetParent(poolHolder.transform);
            }
        }
    }
    
    /// <summary>
    /// Instantiates and disables the given amount of prefabs and adds them into the poolDictionary into the already existing pool.
    /// </summary>
    /// <param name="prefab">Prefab we want to save into our pool.</param>
    /// <param name="difference">Additional amount of instances of the prefab we want to save into our pool.</param>
    public void IncreasePoolSize(GameObject prefab, int difference) {
        // Get InstanceID of the given gameObject.
        int poolKey = prefab.GetInstanceID();

        // Check if the poolDictionary already contains our prefab pool.
        if (poolDictionary.ContainsKey(poolKey)) {
            // Enqueue disabled gameObjects into the Queue equal to the poolSize.
            for (; difference > 0; --difference) {
                ObjectInstance newObject = new ObjectInstance(Instantiate(prefab) as GameObject);
                poolDictionary[poolKey].ObjectQueue.Enqueue(newObject);

                // Get the parent we parented the rest of the pool too and parent the new instance to that as well.
                Transform currentParent = poolDictionary[poolKey].ObjectQueue.FirstOrDefault().Transform.parent;
                newObject.Transform.SetParent(currentParent);
            }
        }
    }
    
    /// <summary>
    /// En -or disables dynamic pooling on the pool with the instances of the given prefab.
    /// </summary>
    /// <param name="prefab">Prefab we want to en -or disable dynamic pooling on the pool off.</param>
    /// <param name="dynamicPooling">Defines if the poolSize should increase dynamically, if there are only visible instances of the given prefab to use.</param>
    public void EnableDynamicPooling(GameObject prefab, bool dynamicPooling = true) {
        // Get InstanceID of the given gameObject.
        int poolKey = prefab.GetInstanceID();

        // Check if the poolDictionary already contains our prefab pool.
        if (poolDictionary.ContainsKey(poolKey)) {
            // Adjust the value of to the expandablePoolDictionar at the given InstanceID.
            poolDictionary[poolKey].DynamicPooling = dynamicPooling;
        }
    }

    /// <summary>
    /// Takes the given prefab and gets the first deactivated instance to reuse.
    /// </summary>
    /// <param name="prefab">Prefab we want to reuse.</param>
    /// <param name="position">Position we want to set for the reused object.</param>
    /// <param name="rotation">Rotation we want to set for the reused object.</param>
    /// <param name="velocity">Velocity we want to set for the reused object.</param>
    public void ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector2 velocity) {
        // Get InstanceID of the given gameObject.
        int poolKey = prefab.GetInstanceID();

        // Check if the poolDictionary already contains our Prefab.
        if (poolDictionary.ContainsKey(poolKey)) {
            // First we try to reuse objects that are invisble.
            var objectToReuse = poolDictionary[poolKey].ObjectQueue.Where(x => !x.IsVisible()).FirstOrDefault();
            
            if (objectToReuse == null) {
                objectToReuse = UseDynamicPooledObject(prefab);
            }
            poolDictionary[poolKey].ObjectQueue.Enqueue(objectToReuse);
            objectToReuse.Reuse(position, rotation, velocity);
        }
    }
    
    /// <summary>
    /// Takes the given prefab and returns the first deactivated instance or creates a new instance to reuse.
    /// </summary>
    /// <param name="prefab">Prefab we want to reuse.</param>
    /// <param name="prefab">Prefab we want to reuse.</param>
    /// <returns>Newly created instance in our pool of the given prefab or our last used instance in case dymaicPooling is disabled.</returns>
    private ObjectInstance UseDynamicPooledObject(GameObject prefab) {
        ObjectInstance dynamicObject = null;
        
        // Get InstanceID of the given gameObject.
        int poolKey = prefab.GetInstanceID();
        
        // Check if the poolDictionary already contains our expandable bool
        // and copy its value into bool dynamicPooling.
        if (poolDictionary.TryGetValue(poolKey, out Pool pool)) {
            if (pool.DynamicPooling) {
                // Increase poolSize by the needed amount of new instances.
                IncreasePoolSize(prefab, 1);
                // Return the newly create instance.
                dynamicObject = poolDictionary[poolKey].ObjectQueue.Where(x => !x.IsVisible()).FirstOrDefault();
            }
        }
        
        // Check if we got a newly instantiated object from our prefab pool.
        if (dynamicObject == null) {
            // If not return the last used instance from the prefab pool
            dynamicObject = poolDictionary[poolKey].ObjectQueue.Dequeue();
        }

        return dynamicObject;
    }
}
