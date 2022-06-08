using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    public static event Action<int> ServerOnPlayerDie;
    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;

    #region Server
    public override void OnStartServer() {
        health.ServerOnDie += ServerHandleDeath;

        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer() {
        health.ServerOnDie += ServerHandleDeath;

        ServerOnBaseDespawned?.Invoke(this);
    }

    [Server]
    private void ServerHandleDeath() {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client
    #endregion
}
