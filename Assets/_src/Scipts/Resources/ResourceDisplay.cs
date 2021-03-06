using Mirror;
using TMPro;
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayer _player;

    private void Start() {
        _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        ClientHandleResourcesUpdated(_player.Resources);

        _player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
    }

    private void OnDestroy() {
        _player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources) {
        resourcesText.text = $"Resources: {resources}";
    }
}
