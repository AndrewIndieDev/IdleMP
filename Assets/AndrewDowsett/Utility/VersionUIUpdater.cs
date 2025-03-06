using TMPro;
using UnityEngine;

public class VersionUIUpdater : MonoBehaviour
{
    public TMP_Text versionUI;

    private void Start()
    {
        versionUI.text = $"V.{Application.version}";
    }
}
