using Mirror;
using TMPro;
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayer _player;

    private void Update() {
        if (_player == null) {
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

            if (_player != null) {
                ClientHandleResourcesUpdated(_player.Resources);

                _player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
            }
        }
    }

    private void OnDestroy() {
        _player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources) {
        resourcesText.text = $"Resources: {resources}";
    }
}
