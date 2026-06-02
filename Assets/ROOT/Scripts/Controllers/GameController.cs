using System.Collections;
using System.Collections.Generic;
using ROOT.Scripts.Controllers;
using UnityEditor;
using UnityEngine;
using Watermelon;

namespace Watermelon.JellyMerge
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;
        [SerializeField] UIController uiController;
        private static ParticlesController particlesController;

        // Level counter (số ván đã chơi)
        private SimpleIntSave currentScore;
        public static int CurrentScore
        {
            get { return instance.currentScore.Value; }
            private set { instance.currentScore.Value = value; }
        }

        // Game score (điểm trong ván hiện tại)
        private static int gameScore;
        public static int GameScore => gameScore;

        public static void AddScore(int points)
        {
            gameScore += points;
            UIController.GetPage<UIGame>()?.UpdateScore(gameScore);
        }

        public static void ResetScore()
        {
            gameScore = 0;
            UIController.GetPage<UIGame>()?.UpdateScore(0);
        }

        private void Awake()
        {
            instance = this;

            CacheComponent(out particlesController);

            currentScore = SaveController.GetSaveObject<SimpleIntSave>("current_score");
            
            uiController.Init();
            particlesController.Init();

            uiController.InitPages();
        }

        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            // currentLevel = LevelsDatabase.GetLevel(CurrentLevelIndex);
            //
            // LevelController.Load(currentLevel);
            //
            // UIController.ShowPage<UIMainMenu>();
            //
            // if (CurrentLevelIndex < 2)
            // {
            //     StartCoroutine(TutorialCoroutine());
            // }
            //
            // SavePresets.CreateSave("Level " + (CurrentLevelIndex + 1).ToString("000"), "Levels");
            ResetScore();
            GamePlayController.Instance.OnPlay();
        }

        public static void OnLevelComplete()
        {
            // Play swipe sound
            AudioController.PlaySound(AudioController.AudioClips.gameWinClip);

            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();

            CurrentScore++;
            SaveController.MarkAsSaveIsRequired();

            if (CurrentScore < 2)
            {
                UIGame gameUI = UIController.GetPage<UIGame>();
                gameUI.HideTutorialPanel();
            }

            Tween.DelayedCall(1.5f, delegate
            {
                AdsManager.ShowInterstitial(delegate
                {
                    UIController.HidePage<UIComplete>();
                    instance.StartGame();
                });
            });
        }

        private IEnumerator TutorialCoroutine()
        {
            UIGame gameUI = UIController.GetPage<UIGame>();
            if (CurrentScore == 0)
            {
                gameUI.ShowTutorialPanel(false);
                yield break;
            }
            else if (CurrentScore == 1)
            {
                yield return new WaitForSeconds(4f);
                gameUI.ShowTutorialPanel(true);
            }

            WaitForSeconds delay = new WaitForSeconds(0.5f);

            while (CurrentScore < 2)
            {
                yield return delay;
            }

            gameUI.HideTutorialPanel();
        }

        public static void OnTapPerformed()
        {
            UIController.HidePage<UIMainMenu>();
            UIController.ShowPage<UIGame>();
        }

        public static void Restart()
        {
            
        }
        public static void GameOver()
        {
            GamePlayController.Instance.CurrentState = GameState.Pause;
        }
        // [Button("Load level")]
        // public void LoadLevelDev()
        // {
        //     CurrentLevelIndex = Mathf.Clamp(levelNumberDev - 1, 0, int.MaxValue);
        //
        //     currentLevel = LevelsDatabase.GetLevel(CurrentLevelIndex);
        //     LevelController.Load(currentLevel);
        //
        //     UIController.ShowPage<UIMainMenu>();
        //     UIController.HidePage<UIGame>();
        //     UIController.HidePage<UIComplete>();
        //
        //     if (CurrentLevelIndex < 2)
        //     {
        //         StartCoroutine(TutorialCoroutine());
        //     }
        // }

        // public static void LoadLevelDev(int index)
        // {
        //     if (index >= 2)
        //     {
        //         UIGame gameUI = UIController.GetPage<UIGame>();
        //         gameUI.HideTutorialPanel();
        //     }
        //
        //     instance.levelNumberDev = index + 1;
        //     instance.LoadLevelDev();
        // }

        #region Extensions
        public bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));

            component = null;

            return false;
        }
        #endregion
    }
}