using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [Header("Camera look parameters")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private int minHeight = 5;
    [SerializeField] private int maxHeight = 25;
    [SerializeField] private float lookPointOffset = 8f;
    [Header("Borders")]
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 _previousInput;
    private int _currentHeight;

    private Controls _controls;

    public override void OnStartAuthority() {
        playerCameraTransform.gameObject.SetActive(true);

        _currentHeight = minHeight;
        UpdateCameraAngle();

        _controls = new Controls();

        _controls.Player.MoveCamera.performed += SetPreviousInput;
        _controls.Player.MoveCamera.canceled += SetPreviousInput;
        _controls.Player.CameraDown.performed += HandleOnCameraDown;
        _controls.Player.CameraUp.performed += HandleOnCameraUp;
        _controls.Player.CameraMouseChangeHeight.performed += HandleOnMouseScroll;

        _controls.Enable();
    }

    public override void OnStopAuthority() {
        _controls.Disable();

        _controls.Player.MoveCamera.performed -= SetPreviousInput;
        _controls.Player.MoveCamera.canceled -= SetPreviousInput;
        _controls.Player.CameraDown.performed -= HandleOnCameraDown;
        _controls.Player.CameraUp.performed -= HandleOnCameraUp;
        _controls.Player.CameraMouseChangeHeight.performed -= HandleOnMouseScroll;
    }

    [ClientCallback]
    private void Update() {
        if (!hasAuthority || !Application.isFocused)
            return;

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition() {
        Vector3 newPosition = playerCameraTransform.position;

        if (_previousInput == Vector2.zero) {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if (cursorPosition.y >= Screen.height - screenBorderThickness) {
                cursorMovement.z += 1;
            }else if (cursorPosition.y <= screenBorderThickness) {
                cursorMovement.z -= 1;
            }

            if (cursorPosition.x >= Screen.width - screenBorderThickness) {
                cursorMovement.x += 1;
            } else if (cursorPosition.x <= screenBorderThickness) {
                cursorMovement.x -= 1;
            }

            newPosition += cursorMovement.normalized * speed * Time.deltaTime;
        } else {
            newPosition += new Vector3(_previousInput.x, 0f, _previousInput.y).normalized * speed * Time.deltaTime;
        }

        newPosition.x = Mathf.Clamp(newPosition.x, screenXLimits.x, screenXLimits.y);
        newPosition.z = Mathf.Clamp(newPosition.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = newPosition;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx) {
        _previousInput = ctx.ReadValue<Vector2>();
    }

    private void HandleOnMouseScroll(InputAction.CallbackContext ctx) {
        if (ctx.ReadValue<Vector2>().y > 0)
            HandleOnCameraDown(ctx);
        else HandleOnCameraUp(ctx);
    }

    private void HandleOnCameraUp(InputAction.CallbackContext ctx) {
        ChangeCurrentHeight(1);
    }

    private void HandleOnCameraDown(InputAction.CallbackContext ctx) {
        ChangeCurrentHeight(-1);
    }

    private void ChangeCurrentHeight(int value) {
        int newHeight = _currentHeight + value;

        if (!(minHeight <= newHeight && maxHeight >= newHeight))
            return;

        _currentHeight = newHeight;
        UpdateCameraAngle();
    }

    private void UpdateCameraAngle() {
        playerCameraTransform.position = new Vector3(
            playerCameraTransform.position.x,
            _currentHeight,
            playerCameraTransform.position.z);

        Vector3 onGroundCameraPosition = new Vector3(
            playerCameraTransform.position.x,
            0,
            playerCameraTransform.position.z);

        Vector3 lookAtPosition = onGroundCameraPosition + Vector3.forward * lookPointOffset;

        playerCameraTransform.LookAt(lookAtPosition);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(screenXLimits.x, 0, screenZLimits.x), 
            new Vector3(screenXLimits.x, 0, screenZLimits.y));
        Gizmos.DrawLine(
            new Vector3(screenXLimits.x, 0, screenZLimits.y),
            new Vector3(screenXLimits.y, 0, screenZLimits.y));
        Gizmos.DrawLine(
            new Vector3(screenXLimits.y, 0, screenZLimits.y),
            new Vector3(screenXLimits.y, 0, screenZLimits.x));
        Gizmos.DrawLine(
            new Vector3(screenXLimits.y, 0, screenZLimits.x),
            new Vector3(screenXLimits.x, 0, screenZLimits.x));
    }
}
