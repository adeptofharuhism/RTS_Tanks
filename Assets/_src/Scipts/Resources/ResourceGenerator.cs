using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private int resourcePerIntervalAtStart = 10;
    [SerializeField] private float interval = 2f;

    private int _currentResourcePerInterval;
    private float _timer;
    private RTSPlayer _player;

    public override void OnStartServer() {
        _currentResourcePerInterval = resourcePerIntervalAtStart;

        _timer = interval;
        _player = connectionToClient.identity.GetComponent<RTSPlayer>();

        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        ControlPoint.ServerOnControlPointCaptured += ServerHandleControlPointCaptured;
        ControlPoint.ServerOnControlPointLost += ServerHandleControlPointLost;
    }

    public override void OnStopServer() {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
        ControlPoint.ServerOnControlPointCaptured -= ServerHandleControlPointCaptured;
        ControlPoint.ServerOnControlPointLost -= ServerHandleControlPointLost;
    }

    [ServerCallback]
    private void Update() {
        _timer -= Time.deltaTime;

        if (_timer < 0) {
            _player.AddResources(_currentResourcePerInterval);
            _timer += interval;
        }
    }

    private void ServerHandleGameOver() {
        enabled = false;
    }

    [Server]
    private void ServerHandleControlPointCaptured(int resourcePerInterval, RTSPlayer pointOwner) {
        if (pointOwner == _player)
            _currentResourcePerInterval += resourcePerInterval;
    }

    private void ServerHandleControlPointLost(int resourcePerInterval, RTSPlayer pointOwner) {
        if (pointOwner == _player)
            _currentResourcePerInterval -= resourcePerInterval;
    }
}
