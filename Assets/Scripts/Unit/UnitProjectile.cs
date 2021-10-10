using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour {
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float lifetime = 5f;
    
    void Start() {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer() {
        Invoke(nameof(DestroySelf), lifetime);
    }

    [Server]
    private void DestroySelf() {
        NetworkServer.Destroy(gameObject);
    }
}
