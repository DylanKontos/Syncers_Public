using FishNet;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerMenuNamespace
{
	public sealed class MultiplayerMenu : MonoBehaviour
	{
		[SerializeField]
		private Button hostButton;

		[SerializeField]
		private Button connectButton;

		public bool Clicked = false; 


		private GameObject ingameDebugConsole;
    	private GameObject graphy;

		private void Start()
		{
			ingameDebugConsole = GameObject.Find("IngameDebugConsole");
			graphy = GameObject.Find("[Graphy]");

			hostButton.onClick.AddListener(() =>
			{
				Clicked = true;
				InstanceFinder.ServerManager.StartConnection();
				InstanceFinder.ClientManager.StartConnection();
			});

			connectButton.onClick.AddListener(() => 
			{
				Clicked = true;
				InstanceFinder.ClientManager.StartConnection();
			});
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Z))
			{
				ToggleDebugger();
				ToggleMultiplayerMenu();
			}
		}

		private void ToggleMultiplayerMenu()
		{
			hostButton.gameObject.SetActive(!hostButton.gameObject.activeSelf);
		
			connectButton.gameObject.SetActive(!connectButton.gameObject.activeSelf);
		}


		private void ToggleDebugger()
		{

			if (ingameDebugConsole != null)
			{
				SetChildrenActive(ingameDebugConsole);
			}

			if (graphy != null)
			{
				SetChildrenActive(graphy);
			}
		}

		private void SetChildrenActive(GameObject parent)
		{
			foreach (Transform child in parent.transform)
			{
				child.gameObject.SetActive(!child.gameObject.activeSelf);
			}
		}


	}
}