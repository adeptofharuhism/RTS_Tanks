using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Turret : NetworkBehaviour
{
    [Header("Aiming")]
    [SerializeField] private float _aimingRange = 0f;
    [Header("Rotating turret")]
    [SerializeField] private Transform _turretTransform = null;
    [SerializeField] private float _turretRotationSpeed = 0f;

    private List<Targetable> _availableTargets = new List<Targetable>();
    private Targetable _currentTarget = null;

    #region Server
    public override void OnStartServer() {
        GlobalTargetCache.ServerOnGlobalTargetAppeared += ServerHandleGlobalTargetAppeared;
        GlobalTargetCache.ServerOnGlobalTargetDisappeared += ServerHandleGlobalTargetDisappeared;

        ProcessAllTargets();
    }

    public override void OnStopServer() {
        GlobalTargetCache.ServerOnGlobalTargetAppeared -= ServerHandleGlobalTargetAppeared;
        GlobalTargetCache.ServerOnGlobalTargetDisappeared -= ServerHandleGlobalTargetDisappeared;
    }

    [ServerCallback]
    private void Update() {
        if (!CanAimAtCurrentTarget())
            FindNewTarget();

        if (_currentTarget == null)
            return;

        AimTurretAtTarget();
    }

    [Server]
    private bool CanAimAtCurrentTarget() {
        return _currentTarget != null && InRangeWithTarget(_currentTarget);
    }

    [Server]
    private bool InRangeWithTarget(Targetable target) =>
        (target.transform.position - _turretTransform.position).sqrMagnitude < (_aimingRange * _aimingRange);

    [Server]
    private void FindNewTarget() {
        foreach(Targetable target in _availableTargets) {
            if (InRangeWithTarget(target)) {
                _currentTarget = target;
                break;
            }
        }
    }

    [Server]
    private void AimTurretAtTarget() {
        Quaternion targetRotation =
            Quaternion.LookRotation(_currentTarget.transform.position - _turretTransform.position);

        _turretTransform.rotation = 
            Quaternion.RotateTowards(_turretTransform.rotation, targetRotation, _turretRotationSpeed * Time.deltaTime);
    }

    [Server]
    private void ProcessAllTargets() {
        foreach(Targetable target in GlobalTargetCache.AllTargets) {
            if (target.connectionToClient != connectionToClient)
                _availableTargets.Add(target);
        }
    }

    [Server]
    private void ServerHandleGlobalTargetAppeared(Targetable target) {
        if (target.connectionToClient != connectionToClient)
            _availableTargets.Add(target);
    }

    [Server]
    private void ServerHandleGlobalTargetDisappeared(Targetable target) {
        _availableTargets.Remove(target);
    }
    #endregion
}
