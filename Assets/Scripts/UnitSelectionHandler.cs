using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour {
    [SerializeField] private LayerMask LayerMask = new LayerMask();
    
    private Camera mainCamera;
    private List<Unit> selectedUnits = new List<Unit>();

    private void Awake() {
        mainCamera = Camera.main;
    }

    void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            DeselectAll();
            // TODO Start selection area
        } else if (Mouse.current.leftButton.wasReleasedThisFrame) {
            // TODO select all units in frame
            ProcessSelectionArea();
        }
    }

    private void DeselectAll() {
        foreach (var selectedUnit in selectedUnits) {
            selectedUnit.Deselect();
        }
        selectedUnits.Clear();
    }

    private void ProcessSelectionArea() {
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask)) return;
        if (!hit.collider.TryGetComponent(out Unit unit)) return;
        if (!unit.hasAuthority) return;
        selectedUnits.Add(unit);

        foreach (var selectedUnit in selectedUnits) {
            selectedUnit.Select();
        }
    }
}
