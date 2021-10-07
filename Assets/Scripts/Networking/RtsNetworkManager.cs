using Mirror;
using UnityEngine;

public class RtsNetworkManager : NetworkManager {
    [SerializeField] private GameObject unitSpawnerPrefab;
    
    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        var playerTransform = conn.identity.transform;
        var unitSpawnerInstance = Instantiate(unitSpawnerPrefab, playerTransform.position, playerTransform.rotation);
        NetworkServer.Spawn(unitSpawnerInstance, conn);
    }
}
