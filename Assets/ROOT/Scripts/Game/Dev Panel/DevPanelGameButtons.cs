using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Watermelon.JellyMerge;

namespace Watermelon
{
    [RequireComponent(typeof(DevPanel))]
    public class DevPanelGameButtons : MonoBehaviour
    {
        [SerializeField] Button firstLevelButton;
        [SerializeField] Button nextLevelButton;
        [SerializeField] Button prevLevelButton;
        [SerializeField] Button changeColorButton;

        private DevPanel devPanel;

        private void Awake()
        {
            devPanel = GetComponent<DevPanel>();

            firstLevelButton.onClick.AddListener(() => OnFirstLevelButtonClicked());
            nextLevelButton.onClick.AddListener(() => OnNextLevelButtonClicked());
            prevLevelButton.onClick.AddListener(() => OnPrevLevelButtonClicked());
            changeColorButton.onClick.AddListener(() => OnChangeColorButtonClicked());
        }

        private void OnFirstLevelButtonClicked()
        {
            SaveController.GetSaveObject<SimpleIntSave>("current_level_index").Value = 0;
            //GameController.LoadLevelDev(0);

            devPanel.DisablePanel();
        }

        private void OnPrevLevelButtonClicked()
        {
            // int currentLevelIndex = SaveController.GetSaveObject<SimpleIntSave>("current_level_index").Value;
            // currentLevelIndex--;
            //
            // if (currentLevelIndex < 0)
            // {
            //     currentLevelIndex = LevelsDatabase.LevelsCount - 1;
            // }
            //
            // SaveController.GetSaveObject<SimpleIntSave>("current_level_index").Value = currentLevelIndex;
            // GameController.LoadLevelDev(currentLevelIndex);
            //
            // devPanel.DisablePanel();
        }

        private void OnNextLevelButtonClicked()
        {
            // int currentLevelIndex = SaveController.GetSaveObject<SimpleIntSave>("current_level_index").Value;
            // currentLevelIndex++;
            //
            // if (currentLevelIndex > LevelsDatabase.LevelsCount - 1)
            // {
            //     currentLevelIndex = 0;
            // }
            //
            // SaveController.GetSaveObject<SimpleIntSave>("current_level_index").Value = currentLevelIndex;
            // GameController.LoadLevelDev(currentLevelIndex);
            //
            // devPanel.DisablePanel();
        }

        private void OnChangeColorButtonClicked()
        {
            devPanel.DisablePanel();
        }
    }
}
