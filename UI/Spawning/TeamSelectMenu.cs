using FishNet;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public sealed class TeamSelectMenu : MonoBehaviour     // Not sure if StartCoroutine(WaitForPlayer()); is necessary.... 
{
    Player player;
                   
	[SerializeField]
	private Button redButton;

	[SerializeField]
	private Button blueButton;

    [SerializeField]
	private Button spectatorsButton;


	public void Awake()
	{
        player = Player.Instance;
        
        if (player == null)
        {
            // Player object not spawned yet, wait for it
            StartCoroutine(WaitForPlayer());
        }
    }

    private IEnumerator WaitForPlayer()
    {
        while (player == null)
        {
            yield return null;
            player = Player.Instance;
        }

        if (player != null)    // AddListeners(); potential seperate method
        {
            redButton.onClick.AddListener(() => 
            {
                player.SetPlayerTeam(Player.Team.Red); // Set on server with ServerRpc on Player
                gameObject.SetActive(false); // disable this menu/object
            });

            blueButton.onClick.AddListener(() => 
            {
                player.SetPlayerTeam(Player.Team.Blue);
                gameObject.SetActive(false);
            });

            
            spectatorsButton.onClick.AddListener(() => 
            {
                player.SetPlayerTeam(Player.Team.Spectators);
                gameObject.SetActive(false);
            });
        }
    }
}


        // player.ServerSpawnPlayerObjectRedTeam(); // player.Respawn(blueTeam)  // - Old
