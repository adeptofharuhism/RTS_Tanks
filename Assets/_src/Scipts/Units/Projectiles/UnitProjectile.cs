using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] protected Rigidbody rb = null;
    [SerializeField] protected int damageToDeal = 20;
    [SerializeField] protected int _penetration = 0;
    [SerializeField] private float destroyAfterSeconds = 5;
    [SerializeField] protected float launchForce = 10f;

    private void Start() {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer() {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [ServerCallback]
    protected virtual void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out NetworkIdentity networkIdentity)) {
            if (networkIdentity.connectionToClient == connectionToClient)
                return;
        }

        if (other.TryGetComponent(out Health health)) {
            health.DealDamage(damageToDeal, _penetration);
        }

        DestroySelf();
    }

    [Server]
    protected void DestroySelf() {
        NetworkServer.Destroy(gameObject);
    }
}
