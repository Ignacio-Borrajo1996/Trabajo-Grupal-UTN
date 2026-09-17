using UnityEngine;

public class UniqueItem : MonoBehaviour, ICollectible
{
    [SerializeField] private string uniqueID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        GameEvents.TriggerItemCollected(uniqueID);
        Destroy(gameObject);
    }
}
