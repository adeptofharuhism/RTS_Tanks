using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;

    private void OnEnable() {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDisable() {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    public void LeaveGame() {
        if (NetworkServer.active && NetworkClient.isConnected) {
            NetworkManager.singleton.StopHost();
        } else {
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner) {
        winnerNameText.text = $"{winner} Has Won!";

        gameOverDisplayParent.SetActive(true);
    }
}
