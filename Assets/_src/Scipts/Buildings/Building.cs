using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Building : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPreview;
    [SerializeField] private Sprite icon = null;
    [SerializeField] private int price = 100;
    [SerializeField] private int id = -1;

    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;

    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    public GameObject BuildingPreview => buildingPreview;
    public Sprite Icon => icon;
    public int Price => price;
    public int Id => id;

    #region Server
    public override void OnStartServer() {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer() {
        ServerOnBuildingDespawned?.Invoke(this);
    }
    #endregion

    #region Client
    public override void OnStartAuthority() {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient() {
        if (!hasAuthority)
            return;

        AuthorityOnBuildingDespawned?.Invoke(this);
    }
    #endregion
}
