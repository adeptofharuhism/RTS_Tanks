using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private int resourcePerInterval = 10;
    [SerializeField] private float interval = 2f;

    private float _timer;
    private RTSPlayer _player;

    public override void OnStartServer() {
        _timer = interval;
        _player = connectionToClient.identity.GetComponent<RTSPlayer>();

        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer() {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update() {
        _timer -= Time.deltaTime;

        if (_timer < 0) {
            _player.AddResources(resourcePerInterval);
            _timer += interval;
        }
    }

    private void ServerHandleGameOver() {
        enabled = false;
    }
}
