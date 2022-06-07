using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public static event Action ServerOnDie;

    [SyncVar]
    private int _currentHealth;

    #region Server
    public override void OnStartServer() {
        _currentHealth = maxHealth;
    }

    public void DealDamage(int damageAmount) {
        if (_currentHealth == 0)
            return;

        _currentHealth = Mathf.Max(_currentHealth - damageAmount, 0);

        if (_currentHealth != 0)
            return;

        ServerOnDie?.Invoke();

        Debug.Log("AMOGUS IS DEAD");
    }
    #endregion

    #region Client
    #endregion
}
