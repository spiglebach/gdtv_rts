using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RtsNetworkManager : NetworkManager {
    [SerializeField] private GameObject unitSpawnerPrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;

    public static event Action OnClientConnectedToLobby;
    public static event Action OnClientDisconnectedFromLobby;

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        OnClientConnectedToLobby?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        OnClientDisconnectedFromLobby?.Invoke();
    }

    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        var player = conn.identity.GetComponent<RtsPlayer>();
        player.SetTeamColor(Random.ColorHSV());
        
        /*var playerTransform = conn.identity.transform;
        var unitSpawnerInstance = Instantiate(unitSpawnerPrefab, playerTransform.position, playerTransform.rotation);
        NetworkServer.Spawn(unitSpawnerInstance, conn);*/
    }

    public override void OnServerSceneChanged(string sceneName) {
        if (SceneManager.GetActiveScene().name.ToLower().Contains("map")) {
            var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }
    }
}
