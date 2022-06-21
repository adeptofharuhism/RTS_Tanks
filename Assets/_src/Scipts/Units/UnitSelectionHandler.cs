using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private RTSPlayer _player;
    private Camera _mainCamera;
    private Vector2 _startPosition;

    private int _unitSelectorBlockedTimes = 0;
    private bool _startedSelection = false;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start() {
        _mainCamera = Camera.main;
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void OnEnable() {
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        UnitSelectionBlocker.OnUnitSelectionBlocked += ClientHandleOnUnitSelectionBlocked;
        UnitSelectionBlocker.OnUnitSelectionAllowed += ClientHandleOnUnitSelectionAllowed;
    }

    private void OnDisable() {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
        UnitSelectionBlocker.OnUnitSelectionBlocked -= ClientHandleOnUnitSelectionBlocked;
        UnitSelectionBlocker.OnUnitSelectionAllowed -= ClientHandleOnUnitSelectionAllowed;
    }

    private void Update() {
        if ((_unitSelectorBlockedTimes == 0) && Mouse.current.leftButton.wasPressedThisFrame) {
            StartSelectionArea();
        } else if (_startedSelection && Mouse.current.leftButton.isPressed) {
            UpdateSelectionArea();
        } else if (_startedSelection && Mouse.current.leftButton.wasReleasedThisFrame) {
            ClearSelectionArea();
        }
    }

    private void StartSelectionArea() {
        _startedSelection = true;

        if (!Keyboard.current.leftShiftKey.isPressed) {
            foreach (Unit selectedUnit in SelectedUnits) {
                selectedUnit.Deselect();
            }

            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);
        _startPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }

    private void UpdateSelectionArea() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - _startPosition.x;
        float areaHeight = mousePosition.y - _startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition =
            _startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea() {
        unitSelectionArea.gameObject.SetActive(false);

        _startedSelection = false;

        if (unitSelectionArea.sizeDelta.magnitude == 0) {
            SingleUnitSelection();
        } else {
            MultipleUnitSelection();
        }
    }

    private void SingleUnitSelection() {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            return;

        if (!hit.collider.TryGetComponent(out Unit unit))
            return;

        if (!unit.hasAuthority)
            return;

        SelectedUnits.Add(unit);
        unit.Select();
    }

    private void MultipleUnitSelection() {
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in _player.MyUnits) {
            if (unit.IsSelected)
                continue;

            Vector3 screenPosition = _mainCamera.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x &&
                screenPosition.x < max.x &&
                screenPosition.y > min.y &&
                screenPosition.y < max.y) {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawned(Unit despawnedUnit) {
        SelectedUnits.Remove(despawnedUnit);
    }

    private void ClientHandleGameOver(string winnerName) {
        enabled = false;
    }

    private void ClientHandleOnUnitSelectionBlocked() {
        _unitSelectorBlockedTimes++;
    }

    private void ClientHandleOnUnitSelectionAllowed() {
        _unitSelectorBlockedTimes--;
    }
}