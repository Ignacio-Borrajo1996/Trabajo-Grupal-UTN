using UnityEngine;

public class objectManager : MonoBehaviour
{
    [SerializeField] private GameObject object1;
    [SerializeField] private GameObject object2;
    [SerializeField] private GameObject object3;

    [SerializeField] private GameObject objectToActivate;

    private void Start()
    {
        objectToActivate.SetActive(false);
    }

    private void Update()
    {
        if (object1 == null && object2 == null && object3 == null)
        {
            objectToActivate.SetActive(true);
        }
    }
}