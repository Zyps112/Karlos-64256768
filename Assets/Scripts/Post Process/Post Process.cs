using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Netcode;

public class PostProcess : NetworkBehaviour
{
    public GameObject cam;
    public float dofMaxDistance;
    public LayerMask dofLayer;

    private Volume globalVolume;
    private DepthOfField depthOfField;

    public override void OnNetworkSpawn()
    {
        globalVolume = FindAnyObjectByType<Volume>();

        if (globalVolume != null && globalVolume.profile != null)
        {
            if (globalVolume.profile.TryGet<DepthOfField>(out depthOfField))
            {
                return;
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
