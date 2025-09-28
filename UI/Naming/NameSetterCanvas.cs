using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NameSetterCanvas : MonoBehaviour  // Attatched to UI
{
    [SerializeField]
    private TMP_InputField _input;

    [SerializeField]
    private Button randomizeButton;


    // When kills/deaths need to be updated, we call from the start of the chain, _input_OnSubmit(string text), because scoreboard NEEDS a name pass in...
    // But if players are randomzing without submitting, we dont want that randomzied name to be submitted.
    // So we seperate into 2 variables, that way when we update kills/deaths, only the submitted name passes in, and not the randomized name sitting in the input field.
    private string generatedName = ""; // To store the generated name
    private string submittedText = ""; // To store the submitted text

    private const int MaxCharacterLimit = 10;

    void Awake()
    {
        _input.onSubmit.AddListener(_input_OnSubmit);
        _input.characterLimit = MaxCharacterLimit;

        randomizeButton.onClick.AddListener(() =>
        {
            generatedName = GenerateName(); // Store the generated name
            _input.text = generatedName; // Set the generated name as the text
        });
    }

    public void UpdateName()
    {
        _input_OnSubmit(submittedText);
    }

    private void _input_OnSubmit(string text)
    {
        if (text.Length > MaxCharacterLimit)
        {
            text = text.Substring(0, MaxCharacterLimit);      // Truncate the input text to the maximum character limit
        }

        submittedText = text; // Store the submitted text
        Player.SetName(submittedText);
    }
 
    public string GenerateName()
    {
        char[] consonants = { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };
        char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
        int nameLength = UnityEngine.Random.Range(3, 8);
        int count = 0;
        string generatedName = "";

        while (count < nameLength)
        {
            if (count % 2 == 0)
            {
                generatedName += consonants[UnityEngine.Random.Range(0, consonants.Length)];
            }
            else
            {
                generatedName += vowels[UnityEngine.Random.Range(0, vowels.Length)];
            }
            count++;
        }
        return generatedName;
    }
}
