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

    private Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>>();

    /// <summary>
    /// Instantiates and disables the given amount of Prefabs and adds them into the poolDictionary.
    /// </summary>
    /// <param name="prefab">Prefab we want to save into our pool.</param>
    /// <param name="poolSize">Amount of instances of the prefab we want to save into our pool.</param>
    public void CreatePool(GameObject prefab, int poolSize) {
        // Get InstanceID of the given GameObject.
        int poolKey = prefab.GetInstanceID();

        // Check if the poolDictionary doesn't already contains our Prefab.
        if (!poolDictionary.ContainsKey(poolKey)) {
            // Add the InstanceID to the poolDictionary.
            poolDictionary.Add(poolKey, new Queue<ObjectInstance>());

            // Enqueue disabled GameObjects into the Queue equal to the poolSize.
            for (int i = 0; i < poolSize; ++i) {
                ObjectInstance newObject = new ObjectInstance(Instantiate(prefab) as GameObject);
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }

    /// <summary>
    /// Changes the parent of the pool with the given prefab.
    /// </summary>
    /// <param name="prefab">Pool we want to change the default parent at.</param>
    /// <param name="parent">Transform we want to parent our pool too.</param>
    public void ParentPool(GameObject prefab, Transform parent) {
        // Get InstanceID of the given GameObject.
        int poolKey = prefab.GetInstanceID();

        // Create new GameObject to store the pooled GameObjects.
        GameObject poolHolder = new GameObject(prefab.name + " Pool");
        poolHolder.transform.SetParent(parent);

        // Try to get the ObjectInstance queue from the given poolKey.
        if (poolDictionary.TryGetValue(poolKey, out Queue<ObjectInstance> objectQueue)) {
            // Loop through each objectInstance in the objectQueue.
            foreach (var objectInstance in objectQueue) {
                // Set its parent equal to the newly created poolHolder.
                objectInstance.Transform.SetParent(poolHolder.transform);
            }
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
        // Get InstanceID of the given GameObject.
        int poolKey = prefab.GetInstanceID();

        // Check if the poolDictionary already contains our Prefab.
        if (poolDictionary.ContainsKey(poolKey)) {
            // First we try to reuse objects that are invisble.
            var objectToReuse = poolDictionary[poolKey].Where(x => !x.IsVisible()).FirstOrDefault();
            if (objectToReuse == null) {
                // If there are none we just use the first one from our queue.
                objectToReuse = poolDictionary[poolKey].Dequeue();
            }
            poolDictionary[poolKey].Enqueue(objectToReuse);
            objectToReuse.Reuse(position, rotation, velocity);
        }
    }

    /// <summary>
    /// Handles components needed for managing of the pooled gameobjects.
    /// </summary>
    public class ObjectInstance {
        private readonly GameObject _gameObject;
        private readonly bool _hasPoolObjectComponent;
        private readonly PoolObject _poolObjectScript;

        private Transform _transform;

        public GameObject GameObject {
            get { return _gameObject; }
        }

        public Transform Transform {
            get { return _transform; }
            set { _transform = value; }
        }

        public ObjectInstance(GameObject objectInstance) {
            _gameObject = objectInstance;
            _transform = _gameObject.transform;
            _gameObject.SetActive(false);

            // Get the PoolObject Script from the given objectInstance.
            var script = _gameObject.GetComponent<PoolObject>();
            if (script != null) {
                _hasPoolObjectComponent = true;
                _poolObjectScript = script;
            }
        }

        public void Reuse(Vector3 position, Quaternion rotation, Vector2 velocity) {
            _gameObject.SetActive(true);
            _transform.position = position;
            _transform.rotation = rotation;

            if (_hasPoolObjectComponent) {
                _poolObjectScript.SetVelocity(velocity);
                _poolObjectScript.OnObjectReuse();
            }
        }

        public bool IsVisible() {
            if (_hasPoolObjectComponent) {
                return _poolObjectScript.IsVisible;
            }
            return true;
        }
    }
}
