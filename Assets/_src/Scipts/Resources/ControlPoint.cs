using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPoint : NetworkBehaviour
{
    [SerializeField] private int _unitsOnConquered = 1;
    [SerializeField] private int _resourceGeneration = 10;
    [SerializeField] private Image _conqueringCircle = null;
    [SerializeField] private GameObject _conqueringCircleParent = null;
    [SerializeField] private float _conqueringTime = 15f;

    public static event Action<int, RTSPlayer> ServerOnControlPointCaptured;
    public static event Action<int, RTSPlayer> ServerOnControlPointLost;

    public event Action<RTSPlayer> ServerOnPointColorsChanged;

    [SyncVar(hook = nameof(ClientHandleConqueringCircleStateChange))]
    private bool _conqueringCircleState = false;
    [SyncVar(hook = nameof(ClientHandleConqueringCircleColorChange))]
    private Color _conqueringCircleColor;
    [SyncVar]
    private float _timeInConquest = 0;
    [SyncVar]
    private bool _hasOwner = false;

    private float _progressImageVelocity;

    private RTSPlayer _currentConqueror = null;
    private RTSPlayer _currentOwner = null;
    private RTSPlayer _lastConqueror = null;
    private bool _timeInConquestCanBeChanged = false;
    private Dictionary<RTSPlayer, List<UnitControlPointConqueror>> _playersInConqueringZone =
        new Dictionary<RTSPlayer, List<UnitControlPointConqueror>>();

    private void Update() {
        if (isServer) {
            UpdateUnitsInZone();
            UpdateCurrentConqueror();
            UpdatePointConquering();
        }

        if (isClient) {
            UpdateConqueringCircle();
        }
    }

    #region Server
    private void UpdateUnitsInZone() {
        foreach(var pair in _playersInConqueringZone) {
            for (int i = 0; i < pair.Value.Count; i++) {
                if (pair.Value[i] == null)
                    pair.Value.RemoveAt(i--);
            }
        }
    }

    private void UpdateCurrentConqueror() {
        _currentConqueror = null;
        _timeInConquestCanBeChanged = true;

        foreach (var pair in _playersInConqueringZone) {
            if (pair.Value.Count > 0) {
                if (_currentConqueror == null)
                    _currentConqueror = pair.Key;
                else {
                    _currentConqueror = null;
                    _timeInConquestCanBeChanged = false;
                    return;
                }
            }
        }
    }

    private void UpdatePointConquering() {
        if (!_timeInConquestCanBeChanged)
            return;

        if (_currentConqueror == null) {
            if (_currentOwner == null) {
                DropNeutral();
            } else {
                DropSomeone();
            }
        } else {
            if (_currentConqueror == _currentOwner)
                return;

            if (_currentOwner == null) {
                ConquerNeutral();
            } else {
                ConquerSomeone();
            }
        }
    }

    private void ConquerNeutral() {
        if (_lastConqueror == null) {
            _lastConqueror = _currentConqueror;

            _conqueringCircleState = true;
            _conqueringCircleColor = _currentConqueror.TeamColor;
        }else if (_lastConqueror != _currentConqueror) {
            DropNeutral();
            return;
        }

        _timeInConquest += Time.deltaTime;

        if (_timeInConquest > _conqueringTime) {
            _timeInConquest = 0;

            _currentOwner = _currentConqueror;
            _currentOwner.AddMaxUnits(_unitsOnConquered);
            _hasOwner = true;

            _conqueringCircleState = false;

            ServerOnPointColorsChanged?.Invoke(_currentOwner);
            ServerOnControlPointCaptured?.Invoke(_resourceGeneration, _currentOwner);
        }
    }

    private void ConquerSomeone() {
        if (!_conqueringCircleState)
            _conqueringCircleState = true;

        _timeInConquest += Time.deltaTime;

        if (_timeInConquest > _conqueringTime) {
            _timeInConquest = 0;

            ServerOnPointColorsChanged?.Invoke(null);
            ServerOnControlPointLost?.Invoke(_resourceGeneration, _currentOwner);

            _currentOwner.RemoveMaxUnits(_unitsOnConquered);
            _currentOwner = null;
            _hasOwner = false;

            _conqueringCircleState = false;
        }
    }

    private void DropNeutral() {
        _timeInConquest -= Time.deltaTime;

        if (_timeInConquest < 0) {
            _timeInConquest = 0;

            _lastConqueror = null;

            _conqueringCircleState = false;
        }
    }

    private void DropSomeone() {
        _timeInConquest -= Time.deltaTime;

        if (_timeInConquest < 0) {
            _timeInConquest = 0;

            _lastConqueror = null;

            _conqueringCircleState = false;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out UnitControlPointConqueror unitControlPointConqueror)) {
            AddPlayerToConquerorsList(unitControlPointConqueror);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out UnitControlPointConqueror unitControlPointConqueror)) {
            RemovePlayerFromConquerorsList(unitControlPointConqueror);
        }
    }

    private void AddPlayerToConquerorsList(UnitControlPointConqueror conqueror) {
        if (!_playersInConqueringZone.ContainsKey(conqueror.Player))
            _playersInConqueringZone[conqueror.Player] = new List<UnitControlPointConqueror>();

        _playersInConqueringZone[conqueror.Player].Add(conqueror);
    }

    private void RemovePlayerFromConquerorsList(UnitControlPointConqueror conqueror) {
        _playersInConqueringZone[conqueror.Player].Remove(conqueror);
    }
    #endregion

    #region Client
    private void UpdateConqueringCircle() {
        float newProgress;
        if (_hasOwner)
            newProgress = 1 - (_timeInConquest / _conqueringTime);
        else newProgress = _timeInConquest / _conqueringTime;

        if (newProgress < _conqueringCircle.fillAmount) {
            _conqueringCircle.fillAmount = newProgress;
        } else {
            _conqueringCircle.fillAmount = Mathf.SmoothDamp(
                _conqueringCircle.fillAmount,
                newProgress,
                ref _progressImageVelocity,
                0.1f);
        }
    }

    private void ClientHandleConqueringCircleStateChange(bool oldValue, bool newValue) {
        _conqueringCircleParent.SetActive(newValue);
    }

    private void ClientHandleConqueringCircleColorChange(Color oldValue, Color newValue) {
        _conqueringCircle.color = new Color(newValue.r, newValue.g, newValue.b, 0.5f);
    }
    #endregion
}