using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour {
    [SerializeField] private Targeter targeter;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    [ServerCallback]
    void Update() {
        var target = targeter.GetTarget();
        if (!CanFireAtTarget(target)) return;
        var targetRotation = Quaternion.LookRotation(target.transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); // FIXME

        if (Time.time > 1 / fireRate + lastFireTime) {
            var projectileVector = target.GetAimAtPoint().position - projectileSpawnPoint.position;
            var projectileRotation = Quaternion.LookRotation(projectileVector);
            var projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget(Targetable target) {
        if (!target) return false;
        var targetPosition = target.transform.position;
        var squareDistance = (targetPosition - transform.position).sqrMagnitude;
        return squareDistance <= fireRange * fireRange;
    }
}
