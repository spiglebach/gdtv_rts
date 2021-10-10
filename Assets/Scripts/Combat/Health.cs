using System;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour {
    [SerializeField] private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    public event Action ServerOnDie;

    #region Server

    public override void OnStartServer() {
        currentHealth = maxHealth;
    }

    [Server]
    public void DealDamage(int amount) {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        if (currentHealth <= 0) {
            ServerOnDie?.Invoke();
            Debug.Log("We died!");
        }
    }

    #endregion

    #region Client

    

    #endregion
}
