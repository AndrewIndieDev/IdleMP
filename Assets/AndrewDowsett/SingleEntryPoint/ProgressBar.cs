using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AndrewDowsett.SingleEntryPoint
{
    public enum EProgressBarType
    {
        Bar_Left_To_Right,
        Bar_Right_To_Left,
        Bar_Top_To_Bottom,
        Bar_Bottom_To_Top,
        Radial_Default_Clockwise,
        Radial_Default_AntiClockwise,
        Radial_Reverse_Clockwise,
        Radial_Reverse_AntiClockwise,
        No_Bar_Only_Text_And_Animation
    }

    public class ProgressBar : MonoBehaviour
    {
        public TMP_Text text;
        public Image progress;

        public float GetProgress() => (progress != null) ? progress.fillAmount : 0.0f;
        public void SetProgress(float value) { if (progress != null) progress.fillAmount = value; }
        public void SetText(string value) { if (text != null) text.text = value; }
    }
}