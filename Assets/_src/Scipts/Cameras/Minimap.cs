using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Mirror;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6;

    private Transform _playerCameraTransform;

    private void Update() {
        if (_playerCameraTransform != null)
            return;

        if (NetworkClient.connection.identity == null)
            return;

        _playerCameraTransform = 
            NetworkClient.connection.identity.GetComponent<RTSPlayer>().CameraTransform;
    }

    private void MoveCamera() {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            mousePosition,
            null,
            out Vector2 localPoint))
            return;

        Vector2 lerp = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        Vector3 newCameraPos = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            _playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y));

        _playerCameraTransform.position = newCameraPos + new Vector3(0, 0, offset);
    }

    public void OnPointerDown(PointerEventData eventData) {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData) {
        MoveCamera();
    }
}
