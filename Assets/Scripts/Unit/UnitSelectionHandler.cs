using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour {
    
    [SerializeField] private RectTransform unitSelectionArea;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private RtsPlayer player;
    private Vector2 selectionStartPosition;
    private Camera mainCamera;
    public readonly List<Unit> selectedUnits = new List<Unit>();

    private void Awake() {
        mainCamera = Camera.main;
        Unit.AuthorityOnUnitDespawned += DeselectUnit;
        GameOverHandler.ClientOnGameOver += DisableUnitSelection;
    }

    private void OnDestroy() {
        Unit.AuthorityOnUnitDespawned -= DeselectUnit;
        GameOverHandler.ClientOnGameOver -= DisableUnitSelection;
    }

    void Update() {
        if (!player) {
            player = NetworkClient.connection.identity.GetComponent<RtsPlayer>();
            if (!player) return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            StartSelectionArea();
        } else if (Mouse.current.leftButton.wasReleasedThisFrame) {
            ProcessSelectionArea();
        } else if (Mouse.current.leftButton.isPressed) {
            UpdateSelectionArea();
        }
    }

    private void DeselectAll() {
        foreach (var selectedUnit in selectedUnits) {
            selectedUnit.Deselect();
        }
        selectedUnits.Clear();
    }

    private void ProcessSelectionArea() {
        unitSelectionArea.gameObject.SetActive(false);
        if (unitSelectionArea.sizeDelta.magnitude < Mathf.Epsilon) {
            SelectSingleUnit();
        } else {
            SelectMultipleUnits();
        }
        IndicateSelectUnits();
    }

    private void SelectSingleUnit() {
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
        if (!hit.collider.TryGetComponent(out Unit unit)) return;
        if (!unit.hasAuthority) return;
        if (!selectedUnits.Contains(unit))
            selectedUnits.Add(unit);
    }
    
    private void SelectMultipleUnits() {
        var min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        var max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (var unit in player.GetUnits()) {
            if (selectedUnits.Contains(unit)) continue;
            var unitScreenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if (unitScreenPosition.x > min.x && unitScreenPosition.x < max.x
                                             && unitScreenPosition.y > min.y && unitScreenPosition.y < max.y) {
                selectedUnits.Add(unit);
            }
        }
    }

    private void IndicateSelectUnits() {
        foreach (var selectedUnit in selectedUnits) {
            selectedUnit.Select();
        }
    }
    
    private void StartSelectionArea() {
        if (!Keyboard.current.leftShiftKey.isPressed)
            DeselectAll();
        unitSelectionArea.gameObject.SetActive(true);
        selectionStartPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }

    private void UpdateSelectionArea() {
        var mousePosition = Mouse.current.position.ReadValue();
        var areaWidth = mousePosition.x - selectionStartPosition.x;
        var areaHeight = mousePosition.y - selectionStartPosition.y;
        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = selectionStartPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void DeselectUnit(Unit unit) {
        selectedUnits.Remove(unit);
    }
    
    private void DisableUnitSelection(string winnerName) {
        enabled = false;
    }
}
