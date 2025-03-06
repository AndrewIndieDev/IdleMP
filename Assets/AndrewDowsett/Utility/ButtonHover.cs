using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Transform buttonTransform;

    private Vector3 initialScale;
    private Vector3 hoveredScale;
    private Vector3 toScale;

    private void Start()
    {
        initialScale = buttonTransform.localScale;
        toScale = initialScale;
        hoveredScale = initialScale * 1.1f;
    }

    private void Update()
    {
        if (buttonTransform.localScale != toScale)
        {
            buttonTransform.localScale = Vector3.Slerp(buttonTransform.localScale, toScale, Time.deltaTime * 20);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        toScale = hoveredScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toScale = initialScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //buttonTransform.localScale = initialScale;
        toScale = initialScale;
    }
}
