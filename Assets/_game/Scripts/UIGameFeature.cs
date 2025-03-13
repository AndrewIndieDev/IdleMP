using AndrewDowsett.Utility;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameFeature : MonoBehaviour
{
    public Image icon;
    public TMP_Text text;
    public Toggle toggleButton;

    private Feature feature;

    public void Set(Nullable<Feature> toCopy)
    {
        if (toCopy == null)
        {
            gameObject.SetActive(false);
            return;
        }
        feature = new Feature(toCopy.Value);
        icon.sprite = feature.Icon;
        text.text = feature.CostString();
        gameObject.SetActive(true);
    }

    public void AssignToggle(string featureName)
    {
        Feature feature = GameManager.Instance.GetFeatureByName(featureName);
        toggleButton.onValueChanged.AddListener((value) =>
        {
            if (value)
            {
                DevelopGameManager.AddFeature(feature);
            }
            else
            {
                DevelopGameManager.RemoveFeature(feature);
            }
        });
    }
}
