using UnityEngine;
using Mirror;
using System;

public class Targeter : NetworkBehaviour
{
    private Targetable _target = null;

    public Targetable Target => _target;

    #region Server
    public override void OnStartServer() {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer() {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject) {
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable target))
            return;

        _target = target;
    }

    [Server]
    public void ClearTarget() {
        _target = null;
    }

    [Server]
    private void ServerHandleGameOver() {
        ClearTarget();
    }
    #endregion

    #region Client
    #endregion
}
