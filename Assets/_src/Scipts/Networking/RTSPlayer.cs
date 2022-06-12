using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5f;

    public event Action<int> ClientOnResourcesUpdated;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public static event Action ClientOnInfoUpdated;

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int _resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool _isPartyOwner = false;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string _displayName;

    private Color _teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public int Resources => _resources;
    public bool IsPartyOwner => _isPartyOwner;
    public string DisplayName => _displayName;
    public Color TeamColor => _teamColor;
    public List<Unit> MyUnits => myUnits;
    public List<Building> MyBuildings => myBuildings;
    public Transform CameraTransform => cameraTransform;

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point) {
        if (Physics.CheckBox(
            point + buildingCollider.center,
            buildingCollider.size / 2,
            Quaternion.identity,
            buildingBlockLayer))
            return false;

        foreach (var building in myBuildings) {
            if ((point - building.transform.position).sqrMagnitude
                <= buildingRangeLimit * buildingRangeLimit) {
                return true;
            }
        }

        return false;
    }

    #region Server
    public override void OnStartServer() {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer() {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    private void ServerHandleUnitSpawned(Unit unit) {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit) {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building) {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building) {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        myBuildings.Remove(building);
    }

    [Server]
    public void AddResources(int amount) {
        _resources += amount;
    }

    [Server]
    public void RemoveResources(int amount) {
        _resources -= amount;
    }

    [Server]
    public void SetTeamColor(Color color) {
        _teamColor = color;
    }

    [Server]
    public void SetState(bool state) {
        _isPartyOwner = state;
    }

    [Server]
    public void SetDisplayName(string displayName) {
        _displayName = displayName;
    }

    [Command]
    public void CmdStartGame() {
        if (!_isPartyOwner)
            return;

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 placeToBuild) {
        Building buildingToPlace = null;

        foreach (var building in buildings) {
            if (building.Id == buildingId) {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null)
            return;

        if (_resources < buildingToPlace.Price)
            return;

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, placeToBuild))
            return;

        GameObject buildingInstance = 
            Instantiate(buildingToPlace.gameObject, placeToBuild, buildingToPlace.transform.rotation);

        NetworkServer.Spawn(buildingInstance, connectionToClient);

        RemoveResources(buildingToPlace.Price);
    }
    #endregion

    #region Client
    public override void OnStartAuthority() {
        if (NetworkServer.active)
            return;

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient() {
        if (NetworkServer.active)
            return;

        DontDestroyOnLoad(gameObject);

        ((RTSNetworkManager)NetworkManager.singleton).PlayerList.Add(this);
    }

    public override void OnStopClient() {
        ClientOnInfoUpdated?.Invoke();

        if (!isClientOnly)
            return;

        ((RTSNetworkManager)NetworkManager.singleton).PlayerList.Remove(this);

        if (!hasAuthority)
            return;

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void ClientHandleDisplayNameUpdated(string oldName, string newName) {
        ClientOnInfoUpdated?.Invoke();
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState) {
        if (!hasAuthority)
            return;

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleUnitSpawned(Unit unit) {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit) {
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building) {
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building) {
        myBuildings.Remove(building);
    }

    private void ClientHandleResourcesUpdated(int oldValue,int newValue) {
        ClientOnResourcesUpdated?.Invoke(newValue);
    }
    #endregion
}
