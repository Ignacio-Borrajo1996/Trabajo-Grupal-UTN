using UnityEngine;

public class ZonaTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public int zoneNumber = 1;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (MusicManager.Instance != null)
            {
                
                MusicManager.Instance.SwitchToZone(zoneNumber);
                Debug.Log("Player entered Zone " + zoneNumber);
            }
        }
    }
}
