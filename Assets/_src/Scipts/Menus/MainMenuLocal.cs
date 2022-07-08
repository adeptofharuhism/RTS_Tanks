using Mirror;
using Steamworks;
using UnityEngine;

public class MainMenuLocal : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private RTSNetworkManager _networkManager;

    public void HostLobby() {
        landingPagePanel.SetActive(false);

        _networkManager.StartHost();
    }
}
