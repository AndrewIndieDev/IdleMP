using AndrewDowsett.Utility;
using UnityEngine;

public class LookAtMousePosition : MonoBehaviour
{
    public Transform trans;

    private void Update()
    {
        trans.LookAt(Utilities.GetMouseWorldPosition());
    }
}
