using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] private Building building;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text costText;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private RtsPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingPreviewRenderer;

    private void Start() {
        mainCamera = Camera.main;
        iconImage.sprite = building.GetIcon();
        costText.text = building.GetPrice().ToString();
    }

    private void Update() {
        if (!player) {
            player = NetworkClient.connection.identity.GetComponent<RtsPlayer>();
            if (!player) return;
        }

        if (!buildingPreviewInstance) return;
        
        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingPreviewRenderer = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!buildingPreviewInstance) return;

        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }
        
        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview() {
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return;
        buildingPreviewInstance.transform.position = hit.point;
        buildingPreviewRenderer.material.color =
            player.CanPlaceBuildingAtPosition(building, hit.point) ? Color.green : Color.red;
        if (!buildingPreviewInstance.activeSelf) {
            buildingPreviewInstance.SetActive(true);
        }
    }
}
