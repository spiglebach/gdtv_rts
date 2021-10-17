using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour {
    [SerializeField] private GameObject lobbyUi;

    private void OnEnable() {
        RtsNetworkManager.OnClientConnectedToLobby += HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby += LeaveLobby;
    }

    private void OnDisable() {
        RtsNetworkManager.OnClientConnectedToLobby -= HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby -= LeaveLobby;
    }

    private void HandleClientConnected() {
        lobbyUi.SetActive(true);
    }

    public void LeaveLobby() {
        if (NetworkServer.active && NetworkClient.isConnected) {
            NetworkManager.singleton.StopHost();
        } else {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(0);
        }
    }
}
