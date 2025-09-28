using UnityEngine;

public class Hover : MonoBehaviour
{
    public float hoverAmount = 0.1f;  // The amount of hover on the Y-axis
    public float hoverSpeed = 0.5f;     // The speed of the hover

    private Vector3 originalPosition;

    void Start()
    {
        // Store the original position of the mesh
        originalPosition = transform.position;
    }

    void Update()
    {
        // Calculate the new position using Mathf.Sin for smooth oscillation
        float newY = originalPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;

        // Update the position of the mesh
        transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
    }
}
