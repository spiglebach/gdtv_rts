using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour {
    private Targetable target;

    #region Server

    public override void OnStartServer() {
        GameOverHandler.ServerOnGameOver += ClearTarget;
    }

    public override void OnStopServer() {
        GameOverHandler.ServerOnGameOver -= ClearTarget;
    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject) {
        if (!targetGameObject.TryGetComponent(out Targetable newTarget)) return;
        target = newTarget;
    }

    [Server]
    public void ClearTarget() {
        target = null;
    }

    #endregion

    public Targetable GetTarget() {
        return target;
    }
}
