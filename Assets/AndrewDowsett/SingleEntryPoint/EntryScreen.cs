using AndrewDowsett.CommonObservers;
using AndrewDowsett.Utility;
using UnityEngine;

namespace AndrewDowsett.SingleEntryPoint
{
    public class EntryScreen : MonoBehaviour, IUpdateObserver
    {
        public SerializableDictionary<EProgressBarType, ProgressBar> ProgressBar;
        
        private ProgressBar barToUse;
        private float _progress = 0;

        public void Show(EProgressBarType eProgressBarType)
        {
            barToUse = ProgressBar[eProgressBarType];
            barToUse.gameObject.SetActive(true);
            gameObject.SetActive(true);
            UpdateManager.RegisterObserver(this);
        }

        public void Hide()
        {
            UpdateManager.UnregisterObserver(this);
            gameObject.SetActive(false);
            //barToUse.SetProgress(0);
            //barToUse.SetText(string.Empty);
            barToUse.gameObject.SetActive(false);
        }

        public float GetBarPercent()
        {
            return barToUse.GetProgress();
        }

        public void SetBarPercent(float percent)
        {
            _progress = percent;
            //barToUse.SetProgress(percent);
        }

        public void SetBarText(string text)
        {
            barToUse.SetText(text);
        }

        public void ObservedUpdate(float deltaTime)
        {
            if (_progress > barToUse.GetProgress())
            {
                barToUse.SetProgress(barToUse.GetProgress() + deltaTime);
            }

            if (_progress < barToUse.GetProgress())
            {
                barToUse.SetProgress(_progress);
            }
        }
    }
}