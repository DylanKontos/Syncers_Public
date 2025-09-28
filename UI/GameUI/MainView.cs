using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainView : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private TextMeshProUGUI syncText;

    public Image shieldImage;

    void Update()
    {
        Player player = Player.Instance;
        if (player == null || player.controlledShip.Value == null) return;

        int clampedHealth = Mathf.Max(0, player.controlledShip.Value.health.Value);
        healthText.text = $" {clampedHealth} % "; 
        syncText.text = $" {player.controlledShip.Value.sync.Value:F0} % ";  
        CheckShield(player);
    }

    private void CheckShield(Player player)
    {
        if (player.controlledShip.Value.isShieldActive.Value) 
        { 
            shieldImage.gameObject.SetActive(true);
        }

        else 
        { 
            shieldImage.gameObject.SetActive(false);
        }
    }
}
