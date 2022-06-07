using Mirror;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    private bool _isSelected = false;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public UnitMovement UnitMovement => unitMovement;
    public Targeter Targeter => targeter;
    public bool IsSelected => _isSelected;

    #region Server
    public override void OnStartServer() {
        ServerOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopServer() {
        ServerOnUnitDespawned?.Invoke(this);
    }
    #endregion

    #region Client
    [Client]
    public void Select() {
        if (!hasAuthority)
            return;

        _isSelected = true;
        onSelected?.Invoke();
    }

    [Client]
    public void Deselect() {
        if (!hasAuthority)
            return;

        _isSelected = false;
        onDeselected?.Invoke();
    }

    public override void OnStartClient() {
        if (!isClientOnly || !hasAuthority)
            return;

        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient() {
        if (!isClientOnly || !hasAuthority)
            return;

        AuthorityOnUnitDespawned?.Invoke(this);
    }
    #endregion
}
