using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour {
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    private UnitMovement unitMovement;

    private void Awake() {
        unitMovement = GetComponent<UnitMovement>();
    }

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

    #endregion

    public UnitMovement GetUnitMovement() {
        return unitMovement;
    }

}
