using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitBuyButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Unit _unit;
    [SerializeField] private Image _iconImage = null;
    [SerializeField] private TMP_Text _priceText = null;

    private RTSPlayer _player;

    private void Start() {
        _priceText.text = _unit.Price.ToString();
        
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        print(_player);
        _iconImage.sprite = _unit.Icon;
        print(_player.TeamColor);
        _iconImage.color = _player.TeamColor;
    }

    private void OnEnable() {
        RTSPlayer.ClientOnPlayerLost += OnPlayerLost;
    }

    private void OnDisable() {
        RTSPlayer.ClientOnPlayerLost -= OnPlayerLost;
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (_player.Resources < _unit.Price)
            return;

        _player.CmdTrySpawnUnit(_unit.Id);
    }

    public void OnPointerDown(PointerEventData _) {

    }

    private void OnPlayerLost(RTSPlayer player) {
        if (player == _player) {
            Destroy(gameObject);
        }
    }
}