using UnityEngine;

public class ObstacleObject : PoolObject {

    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Collider coll;

    public override void OnObjectReuse() {
        base.OnObjectReuse();
    }

    public override void SetVelocity(Vector2 velocity) {
        base.SetVelocity(velocity);
        rb.velocity = velocity;
    }

    public override void OnBecameInvisible() {
        base.OnBecameInvisible();
        // Disable Collsion while we are invisible.
        coll.enabled = false;
    }

    public override void OnBecameVisible() {
        base.OnBecameVisible();
        // Enable Collsion while we are visible.
        coll.enabled = true;
    }
}
