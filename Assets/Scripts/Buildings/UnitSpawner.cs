using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler {
    [SerializeField] private Health health;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TMP_Text remainingUnitsText;
    [SerializeField] private GameObject queueDisplayParent;
    [SerializeField] private Image unitProgressImage;
    [SerializeField] private int unitQueueLimit = 5;
    [SerializeField] private float spawnMoveRange = 7f;
    [SerializeField] private float unitSpawnDuration = 5f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private RtsPlayer player;
    private float progressImageVelocity;
    
    #region Server

    public override void OnStartServer() {
        health.ServerOnDie += ServerHandleDie;
        unitTimer = unitSpawnDuration;
    }

    public override void OnStopServer() {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie() {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdQueueUnit() {
        if (queuedUnits >= unitQueueLimit) return;
        if (!player) player = connectionToClient.identity.GetComponent<RtsPlayer>();
        var unitCost = unitPrefab.GetResourceCost();
        if (player.GetResources() < unitCost) return;
        player.DecreaseResources(unitCost);
        queuedUnits++;
        if (!queueDisplayParent.activeSelf) queueDisplayParent.SetActive(true);
    }

    private void Update() {
        if (isServer) {
            ProduceUnits();
        }
        if (isClient) {
            UpdateTimerDisplay();
        }
    }

    [Server]
    private void ProduceUnits() {
        if (queuedUnits <= 0) return;
        unitTimer -= Time.deltaTime;
        if (unitTimer > 0) return;
        unitTimer += unitSpawnDuration;
        queuedUnits--;
        var unitInstance = Instantiate(unitPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(unitInstance.gameObject, connectionToClient);
        var spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPoint.position.y;

        var unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(spawnPoint.position + spawnOffset);
    }

    #endregion

    #region Client
    
    public void OnPointerClick(PointerEventData eventData) {
        if (!hasAuthority) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        CmdQueueUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldQueuedUnits, int newQueuedUnits) {
        if (newQueuedUnits <= 0) queueDisplayParent.SetActive(false);
        remainingUnitsText.text = newQueuedUnits.ToString();
    }

    private void UpdateTimerDisplay() {
        if (queuedUnits > 0 && !queueDisplayParent.activeSelf) queueDisplayParent.SetActive(true);
        var newProgress = unitTimer / unitSpawnDuration;
        if (newProgress > unitProgressImage.fillAmount) {
            unitProgressImage.fillAmount = newProgress;
        } else {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(unitProgressImage.fillAmount,
                newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    #endregion
    
}
