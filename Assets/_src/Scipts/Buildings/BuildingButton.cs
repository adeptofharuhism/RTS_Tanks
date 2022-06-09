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
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;

    private void Start() {
        _mainCamera = Camera.main;
        iconImage.sprite = building.Icon;
        priceText.text = building.Price.ToString();
    }

    private void Update() {
        if (_player == null) {
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (buildingPreviewInstance == null)
            return;

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        buildingPreviewInstance = Instantiate(building.BuildingPreview);
        buildingPreviewInstance.SetActive(false);
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (buildingPreviewInstance == null)
            return;

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {
            _player.CmdTryPlaceBuilding(building.Id, hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview() {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            return;

        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
            buildingPreviewInstance.SetActive(true);
    }
}
