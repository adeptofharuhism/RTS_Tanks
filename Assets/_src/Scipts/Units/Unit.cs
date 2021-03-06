using Mirror;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [Header("Buy info")]
    [SerializeField] private int _price = 10;
    [SerializeField] private int _unitId = -1;
    [SerializeField] private Sprite _unitIcon;
    [Header("Unit parameters")]
    [SerializeField] private Health health = null;
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
    public int Price => _price;
    public int Id => _unitId;
    public Sprite Icon => _unitIcon;

    #region Server
    public override void OnStartServer() {
        health.ServerOnDie += ServerDieHandle;
        ServerOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopServer() {
        health.ServerOnDie -= ServerDieHandle;
        ServerOnUnitDespawned?.Invoke(this);
    }

    [Server]
    private void ServerDieHandle() {
        NetworkServer.Destroy(gameObject);
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

    public override void OnStartAuthority() {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient() {
        if (!hasAuthority)
            return;

        AuthorityOnUnitDespawned?.Invoke(this);
    }
    #endregion
}
