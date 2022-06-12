using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [Header("RTS Preferences")]
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool isGameInProgress = false;

    private List<RTSPlayer> _playerList = new List<RTSPlayer>();

    public List<RTSPlayer> PlayerList => _playerList;

    #region Server
    public override void OnServerConnect(NetworkConnection conn) {
        if (!isGameInProgress)
            return;

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        PlayerList.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer() {
        PlayerList.Clear();

        isGameInProgress = false;
    }

    public void StartGame() {
        if (PlayerList.Count < 2)
            return;

        isGameInProgress = true;

        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        _playerList.Add(player);

        player.SetDisplayName($"Gay {PlayerList.Count}");

        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)));

        player.SetState(PlayerList.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName) {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map")) {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(var player in PlayerList) {
                GameObject baseInstance =
                    Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);

                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }
    #endregion

    #region Client
    [Obsolete]
    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    [Obsolete]
    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient() {
        PlayerList.Clear();
    }
    #endregion
}
