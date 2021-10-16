using Mirror;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour {
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Targeter targeter;
    [SerializeField] private float chaseRange = 4f;
    
    #region Server

    public override void OnStartServer() {
        GameOverHandler.ServerOnGameOver += ResetPath;
    }

    public override void OnStopServer() {
        GameOverHandler.ServerOnGameOver -= ResetPath;
    }

    [ServerCallback]
    private void Update() {
        var target = targeter.GetTarget();
        if (target) {
            var targetPosition = target.transform.position;
            var squareDistance = (targetPosition - transform.position).sqrMagnitude;
            if (squareDistance > chaseRange * chaseRange) {
                agent.SetDestination(targetPosition);
            } else if (agent.hasPath) {
                ResetPath();
            }
            return;
        }
        
        if (!agent.hasPath) return;
        if (agent.remainingDistance <= agent.stoppingDistance) agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position) {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position) {
        targeter.ClearTarget();
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;
        agent.SetDestination(hit.position);
    }

    [Server]
    private void ResetPath() {
        agent.ResetPath();
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
