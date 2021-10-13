using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class GameOverDisplay : MonoBehaviour {
    [SerializeField] private GameObject gameOverDisplayParent;
    [SerializeField] private Text winnerNameText;
    
    void Start() {
        GameOverHandler.ClientOnGameOver += ShowWinner;
        gameOverDisplayParent.SetActive(false);
    }

    private void OnDestroy() {
        GameOverHandler.ClientOnGameOver -= ShowWinner;
    }

    private void ShowWinner(string winner) {
        winnerNameText.text = $"{winner} has won!";
        gameOverDisplayParent.SetActive(true);
    }

    public void LeaveGame() {
        if (NetworkServer.active && NetworkClient.isConnected) {
            NetworkManager.singleton.StopHost();
        } else {
            NetworkManager.singleton.StopClient();
        }
    }
}
