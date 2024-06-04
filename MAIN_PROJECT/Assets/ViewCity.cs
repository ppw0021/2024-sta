using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ViewCity : MonoBehaviour
{
    public Transform target; // The 3D model's transform
    public RectTransform uiElement; // The UI element's RectTransform
    public Canvas canvas; // The Canvas
    public Camera mainCamera; // The main camera
    public TextMeshProUGUI buttonText; 
    public int user_id_arg;

    public void init(int user_id_arg)
    {
        buttonText.text = user_id_arg.ToString();
    }
    
    void Update()
    {
        if (target != null && uiElement != null && mainCamera != null && canvas != null)
        {
            // Convert the target's position from world space to screen space
            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

            // If the target is in front of the camera
            if (screenPos.z > 0)
            {
                // Convert screen position to Canvas local position
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, screenPos, canvas.worldCamera, out localPoint);

                // Set the UI element's local position
                uiElement.localPosition = localPoint;
                uiElement.gameObject.SetActive(true);
            }
            else
            {
                // Optionally, hide the UI element if the target is behind the camera
                uiElement.gameObject.SetActive(false);
            }
        }
    }
}