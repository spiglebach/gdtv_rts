using Mirror;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour {
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Targeter targeter;
    
    #region Server

    [ServerCallback]
    private void Update() {
        if (!agent.hasPath) return;
        if (agent.remainingDistance <= agent.stoppingDistance) agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position) {
        targeter.ClearTarget();
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;
        agent.SetDestination(hit.position);
    }

    #endregion

    #region Client
/*
    public override void OnStartAuthority() {
        mainCamera = Camera.main;
    }
    
    [ClientCallback]
    private void Update() {
        if (!hasAuthority) return;
        // if (!Input.GetMouseButtonDown(1)) return;
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        // var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return;
        CmdMove(hit.point);
    }
*/
    #endregion
}
