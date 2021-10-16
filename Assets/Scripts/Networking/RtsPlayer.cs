using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RtsPlayer : NetworkBehaviour {
    private Color teamColor;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private List<Unit> myUnits = new List<Unit>();
    [SerializeField] private List<Building> myBuildings = new List<Building>();
    [SerializeField] private Building[] buildableBuildings = new Building[0];
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private float buildingRangeLimit = 5f;

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))] private int resources = 500;
    public event Action<int> ClientOnResourcesUpdated; 

    public List<Unit> GetUnits() {
        return myUnits;
    }
    
    public List<Building> GetBuildings() {
        return myBuildings;
    }

    public int GetResources() {
        return resources;
    }

    public Color GetTeamColor() {
        return teamColor;
    }

    public Transform GetCameraTransform() {
        return cameraTransform;
    }

    #region Server

    public override void OnStartServer() {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
    }

    public override void OnStopServer() {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 position) {
        var buildingToPlace = GetBuildingById(buildingId);
        if (!buildingToPlace) return;
        if (!CanPlaceBuildingAtPosition(buildingToPlace, position)) return;
        
        DecreaseResources(buildingToPlace.GetPrice());
        var buildingInstance = Instantiate(buildingToPlace.gameObject, position, Quaternion.identity);
        NetworkServer.Spawn(buildingInstance, connectionToClient);
    }

    private Building GetBuildingById(int buildingId) {
        foreach (var building in buildableBuildings) {
            if (building.GetId() == buildingId) return building;
        }
        return null;
    }

    private void ServerHandleUnitSpawned(Unit unit) {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Add(unit);
    }
    
    private void ServerHandleUnitDespawned(Unit unit) {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Remove(unit);
    }
    
    private void ServerHandleBuildingSpawned(Building building) {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Add(building);
    }
    
    private void ServerHandleBuildingDespawned(Building building) {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Remove(building);
    }
    
    [Server]
    public void IncreaseResources(int amount) {
        resources += amount;
    }

    [Server]
    public void DecreaseResources(int amount) {
        resources -= amount;
    }

    [Server]
    public void SetTeamColor(Color teamColor) {
        this.teamColor = teamColor;
    }

    #endregion

    #region Client

    public override void OnStartAuthority() {
        if (NetworkServer.active) return;
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStopClient() {
        if (!isClientOnly || !hasAuthority) return;
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
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

    private void ClientHandleResourcesUpdated(int oldResources, int newResources) {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    #endregion

    public bool CanPlaceBuildingAtPosition(Building building, Vector3 position) {
        return CanAffordResourceCost(building.GetPrice())
               && !IsBuildingBlockedBySomethingAtPosition(building, position)
               && IsPositionInRangeOfAnyOwnedBuilding(position);
    }
    
    private bool CanAffordResourceCost(int cost) {
        return resources >= cost;
    }

    private bool IsBuildingBlockedBySomethingAtPosition(Building building, Vector3 position) {
        var buildingCollider = building.GetComponent<BoxCollider>();
        return Physics.CheckBox(position + buildingCollider.center,
            buildingCollider.size / 2,
            Quaternion.identity,
            buildingBlockLayer);
    }

    private bool IsPositionInRangeOfAnyOwnedBuilding(Vector3 position) {
        foreach (var building in myBuildings) {
            if ((position - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit) {
                return true;
            }
        }
        return false;
    }
    
}
