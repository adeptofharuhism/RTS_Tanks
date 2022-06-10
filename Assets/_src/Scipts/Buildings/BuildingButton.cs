using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera _mainCamera;
    private RTSPlayer _player;
    private GameObject _buildingPreviewInstance;
    private Renderer _buildingRendererInstance;
    private BoxCollider _buildingCollider;

    private void Start() {
        _mainCamera = Camera.main;
        iconImage.sprite = building.Icon;
        priceText.text = building.Price.ToString();
        _buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update() {
        if (_player == null) {
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (_buildingPreviewInstance == null)
            return;

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (_player.Resources < building.Price)
            return;

        _buildingPreviewInstance = Instantiate(building.BuildingPreview);
        _buildingPreviewInstance.SetActive(false);
        _buildingRendererInstance = _buildingPreviewInstance.GetComponentInChildren<Renderer>();
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (_buildingPreviewInstance == null)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {
            _player.CmdTryPlaceBuilding(building.Id, hit.point);
        }

        Destroy(_buildingPreviewInstance);
    }

    private void UpdateBuildingPreview() {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            return;

        _buildingPreviewInstance.transform.position = hit.point;

        if (!_buildingPreviewInstance.activeSelf)
            _buildingPreviewInstance.SetActive(true);

        Color color =
            _player.CanPlaceBuilding(_buildingCollider, hit.point)
            ? Color.green
            : Color.red;

        _buildingRendererInstance.material.SetColor("_Color", color);
    }
}
