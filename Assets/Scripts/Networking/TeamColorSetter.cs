using Mirror;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour {
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];
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
    }

    #endregion
}
