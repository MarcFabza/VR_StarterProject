using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShootableButton : MonoBehaviour
{
    public float colliderDepth = 10f;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();

        RectTransform rect = GetComponent<RectTransform>();
        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider>();

        box.isTrigger = true;
        box.size = new Vector3(rect.rect.width, rect.rect.height, colliderDepth);
        box.center = new Vector3((0.5f - rect.pivot.x) * rect.rect.width, (0.5f - rect.pivot.y) * rect.rect.height, 0f);
    }

    public void Press()
    {
        if (button != null && button.interactable)
            button.onClick.Invoke();
    }
}
