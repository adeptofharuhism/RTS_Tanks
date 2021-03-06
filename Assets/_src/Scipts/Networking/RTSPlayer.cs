using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    private const int DEFAULT_RESOURCES = 200;
    public const int MAX_UNITS_DEFAULT = 8;

    [Header("Common")]
    [SerializeField] private Transform _cameraTransform = null;
    [Header("Units spawn")]
    [SerializeField] private float _spawnOffset = 3f;
    [SerializeField] private Unit[] _units = new Unit[0];
    [Header("Buildings spawn")]
    [SerializeField] private LayerMask _buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] _buildings = new Building[0];
    [SerializeField] private float _buildingRangeLimit = 5f;

    public event Action<int> ClientOnResourcesUpdated;
    public event Action<int> ClientOnUnitAmountUpdated;
    public event Action<int> ClientOnUnitMaxAmountUpdated;

    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;
    public static event Action ClientOnInfoUpdated;
    public static event Action<RTSPlayer> ServerOnPlayerLost;
    public static event Action<RTSPlayer> ClientOnPlayerLost;

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int _resources = DEFAULT_RESOURCES;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool _isPartyOwner = false;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string _displayName;

    [SyncVar(hook = nameof(ClientHandleUnitMaxAmountUpdated))]
    private int _currentMaxUnits = MAX_UNITS_DEFAULT;
    [SyncVar(hook = nameof(ClientHandleUnitAmountUpdated))]
    private int _currentUnits = 0;

    private Vector3 _unitSpawnPoint = Vector3.zero;
    [SyncVar]
    private Color _teamColor = new Color();
    private List<Unit> _myUnits = new List<Unit>();
    private List<Building> _myBuildings = new List<Building>();

    public int Resources => _resources;
    public bool IsPartyOwner => _isPartyOwner;
    public string DisplayName => _displayName;
    public Color TeamColor => _teamColor;
    public List<Unit> MyUnits => _myUnits;
    public List<Building> MyBuildings => _myBuildings;
    public Transform CameraTransform => _cameraTransform;

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point) {
        if (Physics.CheckBox(
            point + buildingCollider.center,
            buildingCollider.size / 2,
            Quaternion.identity,
            _buildingBlockLayer))
            return false;

        foreach (var building in _myBuildings) {
            if ((point - building.transform.position).sqrMagnitude
                <= _buildingRangeLimit * _buildingRangeLimit) {
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
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer() {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    private void ServerHandleUnitSpawned(Unit unit) {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        _currentUnits++;
        _myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit) {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        _currentUnits--;
        _myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building) {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        _myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building) {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId)
            return;

        _myBuildings.Remove(building);
    }

    private void ServerHandleBaseSpawned(UnitBase unitBase) {
        if (unitBase.connectionToClient != connectionToClient)
            return;

        Vector3 unitBasePosition = unitBase.transform.position;
        Vector3 spawnDirection = Vector3.zero - unitBasePosition;
        spawnDirection.y = 0;

        _unitSpawnPoint = unitBasePosition + (spawnDirection.normalized * _spawnOffset);
    }

    private void ServerHandleBaseDespawned(UnitBase unitBase) {
        if (unitBase.connectionToClient == connectionToClient) {
            ServerOnPlayerLost?.Invoke(this);
            RpcPlayerLost(this);
        }
    }

    [Server]
    public void AddMaxUnits(int amount) {
        _currentMaxUnits += amount;
    }

    [Server]
    public void RemoveMaxUnits(int amount) {
        _currentMaxUnits -= amount;
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

        foreach (var building in _buildings) {
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

    [Command]
    public void CmdTrySpawnUnit(int unitId) {
        if (_currentMaxUnits <= _currentUnits)
            return;

        Unit unitToSpawn = null;

        foreach (Unit unit in _units) {
            if (unit.Id == unitId) {
                unitToSpawn = unit;
                break;
            }
        }

        if (unitToSpawn == null)
            return;

        if (_resources < unitToSpawn.Price)
            return;

        GameObject unitInstance =
            Instantiate(unitToSpawn.gameObject, _unitSpawnPoint, Quaternion.identity);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        RemoveResources(unitToSpawn.Price);
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
        _myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit) {
        _myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building) {
        _myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building) {
        _myBuildings.Remove(building);
    }

    private void ClientHandleResourcesUpdated(int oldValue,int newValue) {
        ClientOnResourcesUpdated?.Invoke(newValue);
    }

    private void ClientHandleUnitAmountUpdated(int oldValue, int newValue) {
        ClientOnUnitAmountUpdated?.Invoke(newValue);
    }

    private void ClientHandleUnitMaxAmountUpdated(int oldValue, int newValue) {
        ClientOnUnitMaxAmountUpdated?.Invoke(newValue);
    }

    [ClientRpc]
    private void RpcPlayerLost(RTSPlayer player) {
        ClientOnPlayerLost?.Invoke(player);
    }
    #endregion
}
