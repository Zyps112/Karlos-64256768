using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    public GameObject promptText;

    public bool isInteracting;

    public float interactionDistance;

    public LayerMask playerLayer;

    public Transform cam;

    public bool track;

    private void Start()
    {
        promptText.SetActive(false);
    }

    private void Update()
    {
        if(track == true)
        {
            promptText.transform.rotation = cam.transform.rotation;
        }

        isInteracting = Physics.CheckSphere(transform.position, interactionDistance, playerLayer);

        if (isInteracting)
        {
            promptText.SetActive(true);
        }
        else
        {
            promptText.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isInteracting ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
