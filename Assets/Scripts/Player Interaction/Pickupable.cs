using UnityEngine;

/// <summary>
/// Marker component - attach this to any object you want to be pickupable.
/// The server checks for this before allowing a pickup request to succeed.
/// IsHeld is tracked explicitly here instead of inferring it from transform.parent,
/// since objects can legitimately have a scene parent (e.g. nested under a rig/room)
/// without being "held" by a player.
/// </summary>
public class Pickupable : MonoBehaviour
{
    public bool IsHeld;
}