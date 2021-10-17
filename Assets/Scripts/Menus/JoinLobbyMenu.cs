using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour {
    [SerializeField] private GameObject landingPagePanel;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private Button joinButton;

    private void OnEnable() {
        RtsNetworkManager.OnClientConnectedToLobby += HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby += HandleClientDisconnected;
    }

    private void OnDisable() {
        RtsNetworkManager.OnClientConnectedToLobby -= HandleClientConnected;
        RtsNetworkManager.OnClientDisconnectedFromLobby -= HandleClientDisconnected;
    }

    public void Join() {
        var address = addressInput.text;
        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();
        joinButton.interactable = false;
    }

    private void HandleClientConnected() {
        joinButton.interactable = true;
        landingPagePanel.SetActive(false);
        gameObject.SetActive(false);
    }

    private void HandleClientDisconnected() {
        joinButton.interactable = true;
        
    }
}
