using Game.Player;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private bool playerInRange;
    private bool isActivated;

    [SerializeField] private GameObject saveVFX;
    [SerializeField] private float vfxDuration = 0.4f;

    private PlayerRespawn cachedRespawn;
    private PlayerHealth cachedHealth;
    private PlayerInputHandler cachedInput;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;


        playerInRange = true;

        cachedRespawn = other.GetComponent<PlayerRespawn>();
        cachedHealth = other.GetComponent<PlayerHealth>();
        cachedInput = other.GetComponent<PlayerInputHandler>();

        // TODO: show UI prompt
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // TODO: hide UI prompt
    }

    private void Update()
    {
        if (!playerInRange || isActivated) return;

        if (cachedInput != null && cachedInput.InteractPressed)
        {
            Debug.Log("Interact pressed");
            Activate();
        }
    }

    private void Activate()
    {
        isActivated = true;
        Debug.Log("Checkpoint set");

        cachedRespawn?.SetCheckpoint(transform.position);
        cachedHealth?.Heal(999);

        if (saveVFX != null)
        {
            StartCoroutine(PlaySaveVFX());
        }

        ScreenFlash flash = FindObjectOfType<ScreenFlash>();
        if (flash != null)
            StartCoroutine(flash.Flash());
    }

    private IEnumerator PlaySaveVFX()
    {
        saveVFX.SetActive(true);

        yield return new WaitForSeconds(vfxDuration);

        saveVFX.SetActive(false);
    }
}