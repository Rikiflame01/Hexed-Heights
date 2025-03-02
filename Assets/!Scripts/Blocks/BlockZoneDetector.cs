using UnityEngine;

public class BlockZoneDetector : MonoBehaviour
{
    public string zoneTag = "MainZone";

    private bool hasRegisteredExit = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(zoneTag))
            hasRegisteredExit = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(zoneTag))
            return;

        if (hasRegisteredExit)
            return;
        hasRegisteredExit = true;

        if (ZoneExitManager.Instance != null)
            ZoneExitManager.Instance.RegisterExit(gameObject);
    }
}
