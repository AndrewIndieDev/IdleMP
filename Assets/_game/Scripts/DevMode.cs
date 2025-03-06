using UnityEngine;

public class DevMode : MonoBehaviour
{
    int debugCount = 0;
    bool debugEnabled = false;

    private void Start()
    {
        DisableDebug();
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!debugEnabled)
                {
                    debugCount++;
                    if (debugCount >= 3)
                        EnableDebug();
                }
                else
                {
                    debugCount = 0;
                    DisableDebug();
                }
            }
            else
            {
                debugCount = 0;
            }
        }
    }

    private void EnableDebug()
    {
        debugEnabled = true;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void DisableDebug()
    {
        debugEnabled = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
