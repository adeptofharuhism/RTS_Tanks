using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-50)]
public class GlobalTargetCache : NetworkBehaviour
{
    public static event Action<Targetable> ServerOnGlobalTargetAppeared;
    public static event Action<Targetable> ServerOnGlobalTargetDisappeared;

    private static List<Targetable> _allTargets = new List<Targetable>();

    public static List<Targetable> AllTargets => _allTargets;

    public override void OnStartServer() {
        Targetable.ServerOnTargetSpawned += ServerHandleOnTargetSpawned;
        Targetable.ServerOnTargetDespawned += ServerHandleOnTargetDespawned;
    }

    public override void OnStopServer() {
        Targetable.ServerOnTargetSpawned -= ServerHandleOnTargetSpawned;
        Targetable.ServerOnTargetDespawned -= ServerHandleOnTargetDespawned;
    }

    private void ServerHandleOnTargetSpawned(Targetable target) {
        print("New target appear");

        _allTargets.Add(target);

        ServerOnGlobalTargetAppeared?.Invoke(target);
    }

    private void ServerHandleOnTargetDespawned(Targetable target) {
        _allTargets.Remove(target);

        ServerOnGlobalTargetDisappeared?.Invoke(target);
    }
}