using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommander : MonoBehaviour {
    [SerializeField] private UnitSelectionHandler unitSelectionHandler;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    
    private Camera mainCamera;

    private void Awake() {
        mainCamera = Camera.main;
    }

    private void Start() {
        GameOverHandler.ClientOnGameOver += DisableUnitCommander;
    }

    private void OnDestroy() {
        GameOverHandler.ClientOnGameOver -= DisableUnitCommander;
    }

    void Update() {
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;

        if (hit.collider.TryGetComponent(out Targetable target) && !target.hasAuthority) {
            TryTarget(target);
        } else {
            TryMove(hit.point);
        }
    }

    private void TryTarget(Targetable target) {
        foreach (var unit in unitSelectionHandler.selectedUnits) {
            unit.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void TryMove(Vector3 point) {
        foreach (var unit in unitSelectionHandler.selectedUnits) {
            unit.GetUnitMovement().CmdMove(point);
        }
    }

    private void DisableUnitCommander(string winnerName) {
        enabled = false;
    }
}
