using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesDisplay : MonoBehaviour {
    [SerializeField] private Text resourcesText;
    private RtsPlayer player;

    private void Start() {
        player = NetworkClient.connection.identity.GetComponent<RtsPlayer>();
        player.ClientOnResourcesUpdated += DisplayResources;
        DisplayResources(player.GetResources());
    }

    private void OnDestroy() {
        player.ClientOnResourcesUpdated -= DisplayResources;
    }

    private void DisplayResources(int resources) {
        resourcesText.text = $"Resources: {resources}";
    }
}
