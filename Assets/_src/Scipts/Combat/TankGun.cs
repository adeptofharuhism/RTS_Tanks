using Mirror;
using UnityEngine;

public class TankGun : NetworkBehaviour
{
    [Header("Firing")]
    [SerializeField] private float _cooldownTimeSeconds = 1f;
    [SerializeField] protected GameObject _projectilePrefab = null;
    [Header("Aiming")]
    [SerializeField] protected Transform _gunTransform = null;
    [SerializeField] protected Turret _turret = null;
    [SerializeField] private float _maxAngle = 10f;
    [SerializeField] private float _minAngle = 10f;
    [SerializeField] private LayerMask _layerMask = new LayerMask();

    private bool _readyToFire = false;
    private float _cooldownTimeRemaining;

    #region Server
    public override void OnStartServer() {
        _cooldownTimeRemaining = _cooldownTimeSeconds;
    }

    [ServerCallback]
    private void Update() {
        if (!_readyToFire) {
            CooldownProcess();
            return;
        }

        if (_turret.CurrentTarget == null) {
            _gunTransform.localRotation = Quaternion.identity;
            return;
        }

        AimAtTarget();

        if (Physics.Raycast(
            _gunTransform.position,
            _gunTransform.forward, 
            out RaycastHit hit, 
            _turret.AimingRange,
            _layerMask,
            QueryTriggerInteraction.Ignore)) {

            if (hit.collider.TryGetComponent(out NetworkIdentity identity)) {

                if (identity.connectionToClient != connectionToClient) {
                    Fire();
                    GoOnCooldown();
                }
            }
        }
    }

    [Server]
    private void CooldownProcess() {
        _cooldownTimeRemaining -= Time.deltaTime;
        if (_cooldownTimeRemaining < 0)
            _readyToFire = true;
    }

    [Server]
    private void AimAtTarget() {
        Vector3 aimHypotenuse = 
            _turret.CurrentTarget.AimAtPoint.position - _gunTransform.position;
        Vector3 aimLeg = _turret.CurrentTarget.AimAtPoint.position;
        aimLeg.y = _gunTransform.position.y;
        aimLeg = aimLeg - _gunTransform.position;

        float aimXAngle = Vector3.Angle(aimLeg, aimHypotenuse);
        if (_turret.CurrentTarget.AimAtPoint.position.y > _gunTransform.position.y)
            aimXAngle = -aimXAngle;

        aimXAngle = Mathf.Clamp(aimXAngle, _maxAngle, _minAngle);

        _gunTransform.localRotation = Quaternion.Euler(aimXAngle, 0, 0);
    }

    [Server]
    protected virtual void Fire() {
        GameObject projectileInstance = 
            Instantiate(_projectilePrefab, _gunTransform.position, _gunTransform.rotation);

        NetworkServer.Spawn(projectileInstance, connectionToClient);
    }

    [Server]
    private void GoOnCooldown() {
        _readyToFire = false;
        _cooldownTimeRemaining += _cooldownTimeSeconds;
    }
    #endregion
}
