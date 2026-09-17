using System;

public static class GameEvents
{
    public static Action<string> OnItemCollected;

    public static void TriggerItemCollected(string itemId)
    {
        if (OnItemCollected != null)
        {
            OnItemCollected.Invoke(itemId);
        }
    }
}
