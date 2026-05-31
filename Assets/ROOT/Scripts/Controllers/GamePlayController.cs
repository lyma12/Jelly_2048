using UnityEngine;
using Watermelon.JellyMerge;
namespace ROOT.Scripts.Controllers
{
    public class GamePlayController: MonoBehaviour
    {
        private static GamePlayController instance;
        public static GamePlayController Instance => instance;
        [SerializeField] private BoardController boardController;
        [SerializeField] private Swipe swipe;
        private GameState currentState = GameState.PreStart;
        public GameState CurrentState {
            get => currentState;
            set {
                currentState = value;
            }
        }
        public Vector2Int LastMove => boardController.LastMove;
        private void Awake()
        {
            instance = this;
            boardController.Init();
            currentState = GameState.PreStart;
        }
        public void OnPlay()
        {
            boardController.StartGame();
            currentState = GameState.Playing;
        }
        public void Update()
        {
            if (currentState == GameState.Playing)
            {
                if (swipe.SwipeLeft)
                {
                
                    boardController.Move(Index2.left);
                }

                if (swipe.SwipeRight)
                {
                
                    boardController.Move(Index2.right);
                }

                if (swipe.SwipeTop)
                {
                
                    boardController.Move(Index2.up);
                }

                if (swipe.SwipeBottom)
                {
                
                    boardController.Move(Index2.down);
                }
            }
        }
    }
    public enum GameState : byte
    {
        Playing = 0,
        Pause = 1,
        PreStart = 2,
        None = byte.MaxValue
    }
}