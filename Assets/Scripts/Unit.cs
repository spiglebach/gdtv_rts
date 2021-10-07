using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour {
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    #region Client

    [Client]
    public void Select() {
        if (!hasAuthority) return;
        Debug.Log("I got selected!");
        onSelected?.Invoke();
    }
    
    [Client]
    public void Deselect() {
        if (!hasAuthority) return;
        onDeselected?.Invoke();
    }

    #endregion
    

}
