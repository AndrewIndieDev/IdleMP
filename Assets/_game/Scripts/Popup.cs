using UnityEngine;

public class Popup : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.AddPopup(this);
    }

    private void OnDisable()
    {
        GameManager.Instance.RemovePopup(this);
    }
}
