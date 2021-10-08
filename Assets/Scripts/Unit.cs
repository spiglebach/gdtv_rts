using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour {
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    private UnitMovement unitMovement;

    private void Awake() {
        unitMovement = GetComponent<UnitMovement>();
    }

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
        if (!hasAuthority) return;
        onSelected?.Invoke();
    }
    
    [Client]
    public void Deselect() {
        if (!hasAuthority) return;
        onDeselected?.Invoke();
    }

    public override void OnStartClient() {
        if (!isClientOnly || !hasAuthority) return;
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient() {
        if (!isClientOnly || !hasAuthority) return;
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    #endregion

    public UnitMovement GetUnitMovement() {
        return unitMovement;
    }

}
