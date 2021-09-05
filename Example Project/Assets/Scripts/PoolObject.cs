using UnityEngine;

public class PoolObject : MonoBehaviour
{
    private bool _isVisible = false;

    public bool IsVisible {
        get { return _isVisible; }
    }

    public virtual void OnBecameInvisible() {
        _isVisible = false;
    }

    public virtual void OnBecameVisible() {
        _isVisible = true;
    }

    public virtual void OnObjectReuse() {
    }

    public virtual void SetVelocity(Vector2 velocity) {
    }

    protected void Destroy() {
        gameObject.SetActive(false);
    }
}
