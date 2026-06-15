using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class OldDayBlessing : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Grant")]
    [SerializeField] private bool grantOnce = true;
    [SerializeField] private bool hideAfterGranted;

    [Header("Feedback")]
    [SerializeField] private bool playSfx = true;
    [SerializeField] private int sfxIndex = 16;
    [SerializeField] private UnityEvent onBlessingGranted;
    [SerializeField] private UnityEvent onAlreadyGranted;

    private bool playerInRange;
    private bool isShowingInteractToolTip;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        UpdateInteractToolTipVisibility();

        if (Input.GetKeyDown(interactKey))
            TryGrantBlessing();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInRange = true;
        UpdateInteractToolTipVisibility();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInRange = false;
        HideInteractToolTip();
    }

    public void TryGrantBlessing()
    {
        if (PlayerManager.instance == null)
        {
            Debug.LogWarning("OldDayBlessing could not find PlayerManager.");
            return;
        }

        if (grantOnce && PlayerManager.instance.ability_HasOldDayBlessing)
        {
            onAlreadyGranted?.Invoke();
            HideInteractToolTip();
            return;
        }

        PlayerManager.instance.ActivateOldDayBlessing();

        if (playSfx && AudioManager.instance != null)
            AudioManager.instance.PlaySFX(sfxIndex, null);

        onBlessingGranted?.Invoke();
        HideInteractToolTip();

        if (hideAfterGranted)
            gameObject.SetActive(false);
    }

    private void UpdateInteractToolTipVisibility()
    {
        bool shouldShow = playerInRange;

        if (grantOnce && PlayerManager.instance != null && PlayerManager.instance.ability_HasOldDayBlessing)
            shouldShow = false;

        if (shouldShow)
            ShowInteractToolTip();
        else
            HideInteractToolTip();
    }

    private void ShowInteractToolTip()
    {
        if (isShowingInteractToolTip)
            return;

        if (UI_MainScene.instance == null)
            return;

        UI_MainScene.instance.SetWhetherShowInteractToolTip(true);
        isShowingInteractToolTip = true;
    }

    private void HideInteractToolTip()
    {
        if (!isShowingInteractToolTip)
            return;

        if (UI_MainScene.instance != null)
            UI_MainScene.instance.SetWhetherShowInteractToolTip(false);

        isShowingInteractToolTip = false;
    }
}
