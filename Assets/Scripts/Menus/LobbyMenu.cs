using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour {
    [SerializeField] private GameObject lobbyUi;
    [SerializeField] private Button startGameButton;

    private void OnEnable() {
        RtsNetworkManager.OnClientConnectedToLobby += HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby += LeaveLobby;
        RtsPlayer.AuthorityOnLobbyOwnerChanged += AuthorityHandleLobbyOwnerChanged;
    }

    private void OnDisable() {
        RtsNetworkManager.OnClientConnectedToLobby -= HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby -= LeaveLobby;
        RtsPlayer.AuthorityOnLobbyOwnerChanged -= AuthorityHandleLobbyOwnerChanged;
    }

    public void StartGame() {
        NetworkClient.connection.identity.GetComponent<RtsPlayer>().CmdStartGame();
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

    private void AuthorityHandleLobbyOwnerChanged(bool isLobbyOwner) {
        startGameButton.gameObject.SetActive(isLobbyOwner);
    }
}
