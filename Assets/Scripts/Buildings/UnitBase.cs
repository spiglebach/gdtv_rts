using System;
using Mirror;

public class UnitBase : NetworkBehaviour {
    private Health health;

    public static event Action<int> ServerOnPlayerDefeated; 
    public static event Action<UnitBase> ServerOnBaseSpawned; 
    public static event Action<UnitBase> ServerOnBaseDespawned; 

    private void Awake() {
        health = GetComponent<Health>();
    }

    #region Server

    public override void OnStartServer() {
        health.ServerOnDie += DestroyBuilding;
        
        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer() {
        health.ServerOnDie -= DestroyBuilding;
        
        ServerOnBaseDespawned?.Invoke(this);
    }

    [Server]
    private void DestroyBuilding() {
        ServerOnPlayerDefeated?.Invoke(connectionToClient.connectionId);
        
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    

    #endregion
}
