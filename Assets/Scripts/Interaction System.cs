using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InteractionSystem : MonoBehaviour
{
    public Transform cam;

    public LayerMask interactableLayer;

    public float maxDistance = 5f;

    public float knockbackForce;

    public TMP_Text interactionText;

    public TMP_Text interactionInputText;

    private void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, interactableLayer))
        {
            Debug.DrawRay(cam.position, cam.forward * hit.distance, Color.blue);
            int layer = hit.collider.gameObject.layer;
            string layerName = LayerMask.LayerToName(layer);

            if (layerName == "Interactable Physics Objects")
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.rigidbody.AddForce(cam.TransformDirection(Vector3.forward) * knockbackForce, ForceMode.Impulse);

                }
            }

            InteractableObject interactObjectScript = hit.collider.GetComponent<InteractableObject>();
            if(hit.collider.GetComponent<InteractableObject>() != null)
            {
                interactionInputText.color = interactObjectScript.inputKeyColor;
                interactionText.color = interactObjectScript.objectNameColor;

                interactionInputText.text = interactObjectScript.inputKey;
                interactionText.text = interactObjectScript.objectName;
            }
        }
        else
        {
            interactionInputText.text = "";
            interactionText.text = "";
        }    
    }
}
