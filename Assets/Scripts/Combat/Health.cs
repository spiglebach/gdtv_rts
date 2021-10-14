using System;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour {
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer() {
        UnitBase.ServerOnPlayerDefeated += SelfDestruct;
        currentHealth = maxHealth;
    }

    public override void OnStopServer() {
        UnitBase.ServerOnPlayerDefeated -= SelfDestruct;
    }

    [Server]
    public void DealDamage(int amount) {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        if (currentHealth <= 0) {
            ServerOnDie?.Invoke();
        }
    }

    [Server]
    private void SelfDestruct(int defeatedPlayerConnectionId) {
        if (connectionToClient.connectionId != defeatedPlayerConnectionId) return;
        DealDamage(currentHealth);
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth) {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
