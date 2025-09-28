using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPrefab;

    // Call this method to create a new popup
    public void ShowPopup(string message, bool isError)
    {

        GameObject popupInstance = Instantiate(popupPrefab, transform);
        TextMeshProUGUI popupText = popupInstance.GetComponentInChildren<TextMeshProUGUI>();
        Canvas canvas = popupInstance.GetComponentInParent<Canvas>();
        popupInstance.transform.SetParent(canvas.transform, false);

        // if (canvas != null)
        // {
        //     Debug.Log(canvas);
        //     canvas.sortingOrder = 100;  // Set a high value to ensure it's on top
        // }
        // Debug.Log(isError);


        if (popupText != null)
        {
            popupText.text = message;
            StartCoroutine(AnimatePopup(popupInstance, isError));
        }

    }

    private IEnumerator AnimatePopup(GameObject popupInstance, bool isError)
    {
        float moveSpeed = 1f;
        float fadeDuration = 3f;

        CanvasGroup canvasGroup = popupInstance.GetComponent<CanvasGroup>();
        Vector3 startPos = popupInstance.transform.position;
        Vector3 endPos = startPos + new Vector3(0, 100, 0);  // Adjust the end position based on your UI setup

        TextMeshProUGUI popupText = popupInstance.GetComponentInChildren<TextMeshProUGUI>();

        if (popupText != null)
        {
            if (isError)
            {
                //Debug.Log(isError);
                popupText.color = Color.red; // Set the text color to red
            }
            else
            {
                //Debug.Log(isError);
                popupText.color = Color.green; // Set the text color to green (default)
            }
        }

        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            popupInstance.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / fadeDuration);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(popupInstance);
    }

}
