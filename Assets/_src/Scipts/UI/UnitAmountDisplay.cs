using UnityEngine;
using TMPro;
using Mirror;

public class UnitAmountDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _unitAmountText = null;

    private RTSPlayer _player;

    private int _currentUnitAmount = 0;
    private int _currentMaxUnitAmount = RTSPlayer.MAX_UNITS_DEFAULT;

    private void Awake() {
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    private void Start() {
        UpdateText();
    }

    private void OnEnable() {
        _player.ClientOnUnitAmountUpdated += ClientHandleUnitAmountUpdated;
        _player.ClientOnUnitMaxAmountUpdated += ClientHandleMaxUnitAmountUpdated;
    }

    private void OnDisable() {
        _player.ClientOnUnitAmountUpdated -= ClientHandleUnitAmountUpdated;
        _player.ClientOnUnitMaxAmountUpdated -= ClientHandleMaxUnitAmountUpdated;
    }

    private void UpdateText() {
        _unitAmountText.text = $"Units: {_currentUnitAmount}/{_currentMaxUnitAmount}";
    }

    private void ClientHandleUnitAmountUpdated(int amount) {
        _currentUnitAmount = amount;

        UpdateText();
    }

    private void ClientHandleMaxUnitAmountUpdated(int amount) {
        _currentMaxUnitAmount = amount;

        UpdateText();
    }
}