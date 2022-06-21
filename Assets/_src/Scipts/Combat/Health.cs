using Mirror;
using System;
using UnityEngine;

public class Health : NetworkBehaviour
{
    private const float DAMAGE_MODIFIER_PER_ARMOR_UNIT = 0.05f;

    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private int _armor = 0;

    public event Action ServerOnDie;

    public event Action<float, float> ClientOnHealthUpdated;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private float _currentHealth;

    #region Server
    public override void OnStartServer() {
        _currentHealth = _maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer() {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    public void DealDamage(float damageAmount, int penetration, bool useArmor = false) {
        if (_currentHealth == 0)
            return;

        float damageMultiplier = 1;
        if (useArmor) {
            damageMultiplier = damageMultiplier + (penetration - _armor) * DAMAGE_MODIFIER_PER_ARMOR_UNIT;
            damageMultiplier = Mathf.Clamp(damageMultiplier, 0, Mathf.Infinity);
        }

        _currentHealth = Mathf.Max(_currentHealth - damageAmount * damageMultiplier, 0);

        if (_currentHealth != 0)
            return;

        ServerOnDie?.Invoke();
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId) {
        if (connectionToClient.connectionId != connectionId)
            return;

        DealDamage(_currentHealth * 2, 0, false);
    }
    #endregion

    #region Client
    private void HandleHealthUpdated(float oldHealth, float newHealth) {
        ClientOnHealthUpdated?.Invoke(newHealth, _maxHealth);
    }
    #endregion
}
