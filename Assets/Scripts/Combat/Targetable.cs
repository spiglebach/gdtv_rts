using UnityEngine;

public class Targetable : MonoBehaviour {
    [SerializeField] private Transform aimAtPoint;

    public Transform GetAimAtPoint() {
        return aimAtPoint;
    }
}
