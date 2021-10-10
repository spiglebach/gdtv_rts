using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour {
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 30;
    
    void Start() {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer() {
        Invoke(nameof(DestroySelf), lifetime);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out NetworkIdentity networkIdentity)) {
            if (networkIdentity.connectionToClient == connectionToClient) return;
        }
        if (other.TryGetComponent(out Health health)) health.DealDamage(damage);
        
        DestroySelf();
    }

    [Server]
    private void DestroySelf() {
        NetworkServer.Destroy(gameObject);
    }
}
