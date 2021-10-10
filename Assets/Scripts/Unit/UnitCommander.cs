using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommander : MonoBehaviour {
    [SerializeField] private UnitSelectionHandler unitSelectionHandler;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    
    private Camera mainCamera;

    private void Awake() {
        mainCamera = Camera.main;
    }

    void Update() {
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;

        TryMove(hit.point);
    }

    private void TryMove(Vector3 point) {
        foreach (var unit in unitSelectionHandler.selectedUnits) {
            unit.GetUnitMovement().CmdMove(point);
        }
    }
}
