using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RtsPlayer : NetworkBehaviour {
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private List<Unit> myUnits = new List<Unit>();
    [SerializeField] private List<Building> myBuildings = new List<Building>();
    [SerializeField] private Building[] buildableBuildings = new Building[0];
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private float buildingRangeLimit = 5f;

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))] private int resources = 500;
    public event Action<int> ClientOnResourcesUpdated; 
    public static event Action<bool> AuthorityOnLobbyOwnerChanged; 
    public static event Action OnClientOnInfoUpdated; 
    
    private Color teamColor;
    [SyncVar(hook = nameof(AuthorityHandleLobbyOwnerStateUpdated))]private bool isLobbyOwner = false;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))] private string displayName;

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

    public bool IsLobbyOwner() {
        return isLobbyOwner;
    }

    public string GetDisplayName() {
        return displayName;
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

    [Command]
    public void CmdStartGame() {
        if (!isLobbyOwner) return;
        ((RtsNetworkManager) NetworkManager.singleton).StartGame();
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
    public void SetLobbyOwner(bool state) {
        isLobbyOwner = state;
    }

    [Server]
    public void SetTeamColor(Color teamColor) {
        this.teamColor = teamColor;
    }
    
    [Server]
    public void SetDisplayName(string newName) {
        displayName = newName;
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

    public override void OnStartClient() {
        if (NetworkServer.active) return;
        ((RtsNetworkManager) NetworkManager.singleton).Players.Add(this);
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopClient() {
        OnClientOnInfoUpdated?.Invoke();
        if (!isClientOnly) return;
        ((RtsNetworkManager) NetworkManager.singleton).Players.Remove(this);
        if (!hasAuthority) return;
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void AuthorityHandleLobbyOwnerStateUpdated(bool oldState, bool newState) {
        if (!hasAuthority) return;
        AuthorityOnLobbyOwnerChanged?.Invoke(newState);
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

    private void ClientHandleDisplayNameUpdated(string oldName, string newName) {
        OnClientOnInfoUpdated?.Invoke();
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
