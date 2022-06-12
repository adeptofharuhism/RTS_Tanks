using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton = null;

    private void OnEnable() {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
    }

    private void OnDisable() {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
    }

    private void HandleClientConnected() {
        lobbyUI.SetActive(true);
    }

    private void HandleClientDisconnected() {
        SceneManager.LoadScene(0);
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state) {
        startGameButton.gameObject.SetActive(state);
    }

    public void LeaveLobby() {
        if (NetworkServer.active && NetworkClient.isConnected) {
            NetworkManager.singleton.StopHost();
        } else {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }

    public void StartGame() {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }
}
