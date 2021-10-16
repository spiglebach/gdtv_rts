using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour {
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;
    [SerializeField] private int resourceCost = 10;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    private UnitMovement unitMovement;
    private Targeter targeter;
    private Health health;

    private void Awake() {
        unitMovement = GetComponent<UnitMovement>();
        targeter = GetComponent<Targeter>();
        health = GetComponent<Health>();
    }

    #region Server

    public override void OnStartServer() {
        health.ServerOnDie += ServerHandleDeath;
        ServerOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopServer() {
        health.ServerOnDie -= ServerHandleDeath;
        ServerOnUnitDespawned?.Invoke(this);
    }
    
    [Server]
    private void ServerHandleDeath() {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    [Client]
    public void Select() {
        if (!hasAuthority) return;
        onSelected?.Invoke();
    }
    
    [Client]
    public void Deselect() {
        if (!hasAuthority) return;
        onDeselected?.Invoke();
    }

    public override void OnStartAuthority() {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient() {
        if (!hasAuthority) return;
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    #endregion

    public UnitMovement GetUnitMovement() {
        return unitMovement;
    }

    public Targeter GetTargeter() {
        return targeter;
    }

    public int GetResourceCost() {
        return resourceCost;
    }

}
