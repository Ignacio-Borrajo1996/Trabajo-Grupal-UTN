using UnityEngine;
public class LifesCounter : MonoBehaviour
{
    public RectTransform rectTransform;
    float Lifes;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    void Update()
    {
        Lifes = VGM.Instance.Lifes;
        float UpdateLifes = Mathf.MoveTowards(rectTransform.rect.height, Lifes, 5.00f);
        rectTransform.sizeDelta = new Vector2(100f, Mathf.Clamp(UpdateLifes, 0.0f, 3));
    }
}