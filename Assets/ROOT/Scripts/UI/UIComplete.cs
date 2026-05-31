using UnityEngine;
using TMPro;
using Watermelon.JellyMerge;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        [SerializeField] TMP_Text lableText;
        [SerializeField] UIFadeAnimation backFadeAnimation;
        [SerializeField] UIScaleAnimation lableScaleAnimation;
        [SerializeField] ParticleSystem confettiParticle;

        public override void Init()
        {
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            lableText.text = "LEVEL " + (GameController.CurrentScore + 1) + "\nCOMPLETE";

            backFadeAnimation.Show(0.2f);
            lableScaleAnimation.Show(duration: 0.25f);

            confettiParticle.Play();

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            backFadeAnimation.Hide(0.1f);
            lableScaleAnimation.Hide(duration: 0.1f);

            UIController.OnPageClosed(this);
        }
        #endregion
    }
}
