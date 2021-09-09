using UnityEngine;

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