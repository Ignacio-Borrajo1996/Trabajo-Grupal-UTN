using System.Collections.Generic;
using UnityEngine;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    [SerializeField] private List<string> collectedItemsLog = new List<string>();
    
    public int currentScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnItemCollected += RegisterItem;
    }

    private void OnDisable()
    {
        GameEvents.OnItemCollected -= RegisterItem;
    }

    private void RegisterItem(string itemId)
    {
        if (!collectedItemsLog.Contains(itemId))
        {
            collectedItemsLog.Add(itemId);
            currentScore += 100;
        }
    }
}
