using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour {
    [SerializeField] private NavMeshAgent agent;
    private Camera mainCamera;
    
    #region Server

    [Command]
    private void CmdMove(Vector3 position) {
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;
        agent.SetDestination(hit.position);
    }

    #endregion

    #region Client
    
    public override void OnStartAuthority() {
        mainCamera = Camera.main;
    }
    
    private void Update() {
        if (!hasAuthority) return;
        // if (!Input.GetMouseButtonDown(1)) return;
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        // var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return;
        CmdMove(hit.point);
    }

    #endregion
}
