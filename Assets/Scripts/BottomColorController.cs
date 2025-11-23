using UnityEngine;

public class BottomColorController : MonoBehaviour
{
    private RectTransform thisRect;
    private BoxCollider2D thisCollider;
    public Transform particle;

    public Color particleColor;

    public float width;
      void Start()
    {
        Canvas.ForceUpdateCanvases();
        thisRect = this.gameObject.GetComponent<RectTransform>();
        thisCollider = this.gameObject.GetComponent<BoxCollider2D>();
        thisCollider.size = new Vector2(thisRect.rect.width, thisRect.rect.height);
        width = thisRect.rect.width;

    }
}
