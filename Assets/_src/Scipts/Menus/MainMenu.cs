using Mirror;
using Steamworks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private const string HOST_ADDRESS_KEY = "HostAddress";

    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private RTSNetworkManager _networkManager;

    protected Callback<LobbyCreated_t> _lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> _lobbyEntered;

    private void OnEnable() {
        if (!SteamManager.Initialized)
            return;

        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby() {
        if (!SteamManager.Initialized)
            return;

        landingPagePanel.SetActive(false);

        SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypePublic,
            _networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback) {
        if (callback.m_eResult != EResult.k_EResultOK) {
            landingPagePanel.SetActive(true);
            return;
        }

        _networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HOST_ADDRESS_KEY,
            SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback) {
        if (NetworkServer.active)
            return;

        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HOST_ADDRESS_KEY);

        _networkManager.networkAddress = hostAddress;
        _networkManager.StartClient();

        landingPagePanel.SetActive(false);
    }
}
