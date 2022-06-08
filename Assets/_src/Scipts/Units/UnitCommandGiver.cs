using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera _mainCamera;

    private void Start() {
        _mainCamera = Camera.main;
    }

    private void OnEnable() {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDisable() {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update() {
        if (!Mouse.current.rightButton.wasPressedThisFrame)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            return;

        if (hit.collider.TryGetComponent<Targetable>(out Targetable target)) {
            if (target.hasAuthority) {
                TryMove(hit.point);
            } else {
                TryTarget(target);
            }
            return;
        }

        TryMove(hit.point);
    }

    private void TryMove(Vector3 point) {
        foreach(Unit unit in unitSelectionHandler.SelectedUnits) {
            unit.UnitMovement.CmdMove(point);
        }
    }

    private void TryTarget(Targetable target) {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits) {
            unit.Targeter.CmdSetTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winner) {
        enabled = false;
    }
}
