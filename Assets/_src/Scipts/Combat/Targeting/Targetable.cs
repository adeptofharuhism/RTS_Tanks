using UnityEngine;
using Mirror;
using System;

public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint = null;

    public static event Action<Targetable> ServerOnTargetSpawned;
    public static event Action<Targetable> ServerOnTargetDespawned;

    public Transform AimAtPoint => aimAtPoint;

    public override void OnStartServer() {
        ServerOnTargetSpawned?.Invoke(this);
    }

    public override void OnStopServer() {
        ServerOnTargetDespawned?.Invoke(this);
    }
}
