using UnityEngine;
using Mirror;

public class UnitProjectileATGM : UnitProjectile
{
    [Header("ATGM settings")]
    [SerializeField] private float _turnSpeed = 10f;
    [SerializeField] private Health _health = null;

    private Targetable _target = null;

    public override void OnStartServer() {
        base.OnStartServer();

        _health.ServerOnDie += DestroySelf;
    }

    public override void OnStopServer() {
        _health.ServerOnDie -= DestroySelf;
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.TryGetComponent(out NetworkIdentity networkIdentity)) {
            if (networkIdentity.connectionToClient == connectionToClient)
                return;
        }

        if (collision.gameObject.TryGetComponent(out Health health)) {
            health.DealDamage(damageToDeal, _penetration);
        }

        DestroySelf();
    }

    [ServerCallback]
    protected override void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out ControlPoint controlPoint)) {
            return;
        }

        base.OnTriggerEnter(other);
    }

    [Server]
    public void SetTarget(Targetable target) {
        _target = target;
    }

    private void Update() {
        if (_target == null)
            return;

        AimAtTarget();
    }

    private void AimAtTarget() {
        transform.LookAt(_target.AimAtPoint.position);

        rb.velocity = Vector3.RotateTowards(
            rb.velocity, 
            _target.AimAtPoint.position - transform.position, 
            _turnSpeed * Time.deltaTime,
            0);
    }
}