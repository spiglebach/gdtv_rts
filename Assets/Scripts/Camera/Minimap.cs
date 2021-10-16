using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler {
    [SerializeField] private RectTransform minimapRect;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6f;

    private Transform playerCameraTransform;

    private void Update() {
        if (playerCameraTransform) return;
        if (!NetworkClient.connection.identity) return;
        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RtsPlayer>().GetCameraTransform();
    }

    private void MoveCamera() {
        var mousePos = Mouse.current.position.ReadValue();
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, mousePos, null, out Vector2 localPoint)) return;

        var lerp = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);
        var newCameraPos = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y));
        playerCameraTransform.position = newCameraPos + new Vector3(0, 0, offset);
    }

    public void OnPointerDown(PointerEventData eventData) {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData) {
        MoveCamera();
    }
}
