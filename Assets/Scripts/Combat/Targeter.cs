using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour {
    private Targetable target;

    #region Server

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
