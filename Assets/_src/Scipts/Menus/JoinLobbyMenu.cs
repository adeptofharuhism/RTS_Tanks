using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject langingPagePanel = null;
    [SerializeField] private TMP_InputField addressInput = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable() {
        RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
    }

    private void OnDisable() {
        RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
    }

    public void Join() {
        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected() {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        langingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected() {
        joinButton.interactable = true;
    }
}
