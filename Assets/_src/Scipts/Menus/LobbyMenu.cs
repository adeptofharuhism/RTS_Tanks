using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private PlayerNameSetter[] playerNameSetters = new PlayerNameSetter[4];

    private void OnEnable() {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDisable() {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
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

    private void ClientHandleInfoUpdated() {
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).PlayerList;

        for (int i=0;i< playerNameSetters.Length; i++) {
            if (i < players.Count)
                playerNameSetters[i].SetPlayerName(players[i].DisplayName);
            else playerNameSetters[i].SetPlayerName(string.Empty);
        }

        startGameButton.interactable = (players.Count >= 2);
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
