using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{

    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float maxHealth = 100f;
	public readonly SyncVar<float> health = new SyncVar<float>(100);
    private float lerpSpeed = 0.03f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnStartServerStart()
    {
        health.Value = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != health.Value)
        {
            healthSlider.value =  health.Value;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value =  Mathf.Lerp(easeHealthSlider.value, health.Value, lerpSpeed);
        }
    }

    public void TakeDamage(float damage)
    {
        health.Value -= damage;
        UpdateHealthBarObserversRpc(health.Value);
    }

    public void ResetHealthBar()
    {
        health.Value = 100;
        UpdateHealthBarObserversRpc(health.Value);
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    private void UpdateHealthBarObserversRpc(float updatedHealth)
    {
        healthSlider.value = updatedHealth; // Update health slider
        easeHealthSlider.value =  Mathf.Lerp(easeHealthSlider.value, health.Value, lerpSpeed);
        // easeHealthSlider.value = updatedHealth; // Optional: immediately sync eased slider
    }

}
