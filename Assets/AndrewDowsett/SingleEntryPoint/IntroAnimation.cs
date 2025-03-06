using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace AndrewDowsett.SingleEntryPoint
{
    public class IntroAnimation : MonoBehaviour
    {
        public MMF_Player introFeedbacks;

        public async UniTask Play()
        {
            // await an animation, or Feel Feedbacks.
            introFeedbacks.PlayFeedbacks();
            await UniTask.WaitForSeconds(introFeedbacks.TotalDuration);
            Destroy(gameObject);
        }
    }
}