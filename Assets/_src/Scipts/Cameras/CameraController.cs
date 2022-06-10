using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 _previousInput;

    private Controls _controls;

    public override void OnStartAuthority() {
        playerCameraTransform.gameObject.SetActive(true);

        _controls = new Controls();

        _controls.Player.MoveCamera.performed += SetPreviousInput;
        _controls.Player.MoveCamera.canceled += SetPreviousInput;

        _controls.Enable();
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
