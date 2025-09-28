using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ChangeInput : MonoBehaviour
{
    private EventSystem system;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;

    void Start()
    {
        system = EventSystem.current;
        usernameInput.Select();
    }

    void Update()
    {
        if (usernameInput == null || !usernameInput.isActiveAndEnabled) return;

        if (Input.GetKeyDown(KeyCode.Tab)) // Use new input for other devices
        {
            if (system.currentSelectedGameObject == usernameInput.gameObject)
            {
                if (emailInput != null && emailInput.isActiveAndEnabled)
                {
                    emailInput.Select();
                }
                else
                {
                    passwordInput.Select();
                }
            }
            else if (system.currentSelectedGameObject == emailInput?.gameObject)
            {
                passwordInput.Select();
            }
            else if (system.currentSelectedGameObject == passwordInput.gameObject)
            {
                usernameInput.Select();
            }
        }
    }
}
