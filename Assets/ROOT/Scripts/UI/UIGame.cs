using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.JellyMerge;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] TMP_Text levelText;
        [SerializeField] Button skipLevelButton;
        [SerializeField] Button restartButton;

        [Space]
        [SerializeField] GameObject tutorialPanel;
        [SerializeField] GameObject horizontalTutorialPanel;
        [SerializeField] GameObject verticalTutorialPanel;

        public override void Init()
        {
            restartButton.onClick.AddListener(RestartButton);
            skipLevelButton.onClick.AddListener(SkipLevelButton);
        }

        private void OnEnable()
        {
            AdsManager.AdLoaded += OnRewardedAdLoaded;
        }

        private void OnDisable()
        {
            AdsManager.AdLoaded -= OnRewardedAdLoaded;
        }

        private void OnRewardedAdLoaded(AdProvider advertisingModules, AdType advertisingType)
        {
            if(advertisingType == AdType.RewardedVideo)
            {
                skipLevelButton.gameObject.SetActive(true);
            }
        }

        private void OnRewardedAdLoaded()
        {
            skipLevelButton.gameObject.SetActive(true);
        }

        public void ShowTutorialPanel(bool horizontal)
        {
            tutorialPanel.SetActive(true);

            if (horizontal)
            {
                horizontalTutorialPanel.SetActive(true);
                verticalTutorialPanel.SetActive(false);
            }
            else
            {
                horizontalTutorialPanel.SetActive(false);
                verticalTutorialPanel.SetActive(true);
            }
        }

        public void HideTutorialPanel()
        {
            tutorialPanel.SetActive(false);
            verticalTutorialPanel.SetActive(false);
            horizontalTutorialPanel.SetActive(false);
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            levelText.text = "LEVEL " + (GameController.CurrentScore + 1);
            skipLevelButton.gameObject.SetActive(AdsManager.IsRewardBasedVideoLoaded());

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        #endregion

        #region Buttons

        public void RestartButton()
        {
            GameController.Restart();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void SkipLevelButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            AdsManager.ShowRewardBasedVideo((bool reward) =>
            {
                if (reward)
                {
                    //LevelController.SkipLevel();
                }
            });
        }

        #endregion
    }
}
