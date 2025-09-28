using UnityEngine;
using UnityEngine.UI;

public class RespawnMenuDisplayer : MonoBehaviour
{
    Canvas canvas;

    [SerializeField]
    private Button respawnButton;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvas.enabled = false;
        Player player = Player.Instance;
    }

    public void DeathScreenEnable()
    {
        // if (player == null || player.controlledShip == null) return;

        canvas.enabled = true; // enable death screen

        respawnButton.onClick.AddListener(() =>
            {
                canvas.enabled = false;
                if (Player.Instance.isAlive.Value == true) return;
                TriggerRespawn();  
            });
    }
        
    private void TriggerRespawn()
    {
        Player player = Player.Instance;
        player.TryRespawnServerRpc();
        player.TryRespawn();
    }
}


    // OLD COROUTINE OF FINDING PLAYER AND SPAWNING WHEN DEAD
    // Using a coroutine to constantly check if a player was alive or dead? No. Send a TargetRpc instead to respawn from Player

    //StartCoroutine(WaitForPlayer()); // was in start
    // private IEnumerator WaitForPlayer()  // Wait until the player is found.
    // {
    //     while (player == null)
    //     {
    //         player = Player.Instance;
    //         Debug.Log(player + "from wait for player");
    //         yield return null;
    //     }

    //     StopCoroutine(WaitForPlayer()); // Stop coroutine to avoid memory leaks
    // }

    // private void Update()
    // {
    //     Debug.Log(canvas.enabled);
    // }


    //     if (player != null) // && player.isAlive == false)  // if not null && is dead
    //     {
    //         Debug.Log("DeathScreenEnable if statement");
    //         canvas.enabled = true; // enable death screen
    //         Debug.Log(canvas.enabled);

    //         respawnButton.onClick.AddListener(() =>
    //         {
    //             canvas.enabled = false;
    //             Debug.Log("clicked");
    //             TriggerRespawn();
    //             // previousIsAliveStatus = player.isAlive; // This might be too early, potentially set this variable from player.
    //         });
    //     }
    // }
