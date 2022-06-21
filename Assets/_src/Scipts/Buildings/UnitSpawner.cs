using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;
    [Header("Unit Spawn Queue")]
    [SerializeField] private TMP_Text remainingUnits = null;
    [SerializeField] private Image unitProgressImage = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int _queuedUnits;
    [SyncVar]
    private float _unitTimer;

    private float progressImageVelocity;

    private void Update() {
        if (isServer) {
            ProduceUnits();
        }
        
        if (isClient) {
            UpdateTimerDisplay();
        }
    }

    #region Server
    public override void OnStartServer() {
        health.ServerOnDie += ServerHandleDie;
        base.OnStartServer();
    }

    public override void OnStopServer() {
        health.ServerOnDie -= ServerHandleDie;
        base.OnStopServer();
    }

    [Server]
    private void ServerHandleDie() {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ProduceUnits() {
        if (_queuedUnits == 0)
            return;

        _unitTimer += Time.deltaTime;

        if (_unitTimer < unitSpawnDuration)
            return;

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        _queuedUnits--;
        _unitTimer = 0f;
    }

    [Command]
    private void CmdSpawnUnit() {
        if (_queuedUnits == maxUnitQueue)
            return;

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        if (player.Resources < unitPrefab.Price)
            return;

        _queuedUnits++;

        player.RemoveResources(unitPrefab.Price);
    }
    #endregion

    #region Client
    public void OnPointerClick(PointerEventData pointer) {
        if (pointer.button != PointerEventData.InputButton.Left)
            return;

        if (!hasAuthority)
            return;

        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldValue, int newValue) {
        remainingUnits.text = newValue.ToString();
    }

    private void UpdateTimerDisplay() {
        float newProgress = _unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount) {
            unitProgressImage.fillAmount = newProgress;
        } else {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f);
        }
    }
    #endregion
}
