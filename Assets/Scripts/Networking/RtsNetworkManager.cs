using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RtsNetworkManager : NetworkManager {
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab;

    public static event Action OnClientConnectedToLobby;
    public static event Action OnClientDisconnectedFromLobby;

    private bool isGameInProgress = false;

    public List<RtsPlayer> Players { get; } = new List<RtsPlayer>();

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        OnClientConnectedToLobby?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        OnClientDisconnectedFromLobby?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn) {
        if (!isGameInProgress) return;
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        var player = conn.identity.GetComponent<RtsPlayer>();
        Players.Remove(player);
    }

    public override void OnStopServer() {
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame() {
        if (Players.Count < 2) return;
        isGameInProgress = true;
        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        var player = conn.identity.GetComponent<RtsPlayer>();
        Players.Add(player);
        player.SetDisplayName($"Player {Players.Count}");
        player.SetTeamColor(Random.ColorHSV());
        player.SetLobbyOwner(Players.Count < 2);
    }

    public override void OnServerSceneChanged(string sceneName) {
        if (!SceneManager.GetActiveScene().name.ToLower().Contains("map")) return;
        var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
        NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        foreach (var player in Players) {
            var baseInstance = Instantiate(basePrefab, GetStartPosition().position, Quaternion.identity);
            NetworkServer.Spawn(baseInstance, player.connectionToClient);
        }
    }

    public override void OnStopClient() {
        Players.Clear();
    }
}
