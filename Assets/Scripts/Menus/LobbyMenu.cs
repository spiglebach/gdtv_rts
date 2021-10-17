using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour {
    [SerializeField] private GameObject lobbyUi;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private string emptyPlayerSlotText = "Waiting for players...";

    private void OnEnable() {
        RtsNetworkManager.OnClientConnectedToLobby += HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby += LeaveLobby;
        RtsPlayer.AuthorityOnLobbyOwnerChanged += AuthorityHandleLobbyOwnerChanged;
        RtsPlayer.OnClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDisable() {
        RtsNetworkManager.OnClientConnectedToLobby -= HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby -= LeaveLobby;
        RtsPlayer.AuthorityOnLobbyOwnerChanged -= AuthorityHandleLobbyOwnerChanged;
        RtsPlayer.OnClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    public void StartGame() {
        NetworkClient.connection.identity.GetComponent<RtsPlayer>().CmdStartGame();
    }

    private void HandleClientConnected() {
        lobbyUi.SetActive(true);
    }

    private void ClientHandleInfoUpdated() {
        var players = ((RtsNetworkManager) NetworkManager.singleton).Players;
        for (int i = 0; i < players.Count; i++) {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }
        for (int i = players.Count; i < playerNameTexts.Length; i++) {
            playerNameTexts[i].text = emptyPlayerSlotText;
        }
        startGameButton.interactable = players.Count >= 2;
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
