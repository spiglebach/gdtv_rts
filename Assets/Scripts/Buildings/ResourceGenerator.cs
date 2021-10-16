using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour {
    [SerializeField] private Health health;
    [SerializeField] private int resourcesPerInterval = 10;
    [SerializeField] private float interval = 2f;

    private float timer;
    private RtsPlayer player;

    public override void OnStartServer() {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RtsPlayer>();

        health.ServerOnDie += SelfDestruct;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer() {
        health.ServerOnDie -= SelfDestruct;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    private void SelfDestruct() {
        NetworkServer.Destroy(gameObject);
    }

    private void ServerHandleGameOver() {
        enabled = false;
    }

    [ServerCallback]
    private void Update() {
        timer -= Time.deltaTime;
        if (!(timer <= 0)) return;
        
        timer += interval;
        player.IncreaseResources(resourcesPerInterval);
    }
}
