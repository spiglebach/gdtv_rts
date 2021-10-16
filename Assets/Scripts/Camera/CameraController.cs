using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour {
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;
    
    private Controls controls;

    public override void OnStartAuthority() {
        playerCameraTransform.gameObject.SetActive(true);
        controls = new Controls();
        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;
        
        controls.Enable();
    }

    [ClientCallback]
    private void Update() {
        if (!hasAuthority || !Application.isFocused) return;
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition() {
        var cameraPosition = playerCameraTransform.position;
        if (previousInput == Vector2.zero) {
            var cursorMovement = Vector3.zero;
            var cursorPosition = Mouse.current.position.ReadValue();
            if (cursorPosition.y >= Screen.height - screenBorderThickness) {
                cursorMovement.z += 1;
            } else if (cursorPosition.y <= screenBorderThickness) {
                cursorMovement.z -= 1;
            }
            if (cursorPosition.x >= Screen.width - screenBorderThickness) {
                cursorMovement.x += 1;
            } else if (cursorPosition.x <= screenBorderThickness) {
                cursorMovement.x -= 1;
            }
            cameraPosition += cursorMovement.normalized * (speed * Time.deltaTime);
        } else {
            cameraPosition += new Vector3(previousInput.x, 0f, previousInput.y).normalized * (speed * Time.deltaTime);
        }

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, screenXLimits.x, screenXLimits.y);
        cameraPosition.z = Mathf.Clamp(cameraPosition.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = cameraPosition;
    }

    private void SetPreviousInput(InputAction.CallbackContext context) {
        previousInput = context.ReadValue<Vector2>();
    }
}
