using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class TeamColorSetter : NetworkBehaviour {
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];
    [SerializeField] private Image[] imagesToColor = new Image[0];
    [SyncVar(hook = nameof(HandleTeamColorUpdated))] private Color teamColor = new Color();

    #region Server

    public override void OnStartServer() {
        var player = connectionToClient.identity.GetComponent<RtsPlayer>();
        teamColor = player.GetTeamColor();
    }

    #endregion

    #region Client

    private void HandleTeamColorUpdated(Color oldColor, Color newColor) {
        foreach (var renderer in colorRenderers) {
            renderer.material.color = newColor;
        }
        foreach (var image in imagesToColor) {
            image.color = newColor;
        }
    }

    #endregion
}
