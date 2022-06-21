using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class UnitBuyButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Unit _unit;
    [SerializeField] private Image _iconImage = null;
    [SerializeField] private TMP_Text _priceText = null;

    public static event Action<Unit> ServerOnUnitOrdered;

    private RTSPlayer _player;

    private void Start() {
        _priceText.text = _unit.Price.ToString();
        _iconImage.sprite = _unit.Icon;
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    public void OnPointerUp(PointerEventData eventData) {
        print("Button clicked");
        if (_player.Resources < _unit.Price)
            return;

        print("Sending command");
        _player.CmdTrySpawnUnit(_unit.Id);
    }

    public void OnPointerDown(PointerEventData _) {

    }
}