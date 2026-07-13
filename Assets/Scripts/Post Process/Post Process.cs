using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcess : MonoBehaviour
{
    public Volume globalVolume;
    public GameObject cam;
    public float dofMaxDistance;
    public LayerMask dofLayer;

    private DepthOfField depthOfField;

    private void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            // 2. Fetch the Depth of Field override from the profile
            if (globalVolume.profile.TryGet<DepthOfField>(out depthOfField))
            {
                Debug.Log("Depth of Field successfully referenced!");
            }
            else
            {
                Debug.LogWarning("Depth of Field override not found in the Volume Profile.");
            }
        }
    }

    private void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, dofMaxDistance, dofLayer))
        {
            if(depthOfField != null)
            {
                depthOfField.focusDistance.value = hit.distance;
            }
        }
    }
}
