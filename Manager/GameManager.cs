using UnityEngine;
using UnityEngine.Events;
using SK.Player;

namespace SK.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState currentGameState { get; private set; }

        public event UnityAction<GameState> OnGameStateChanged;
        public event UnityAction<int> OnInitialize;

        public UIManager UIManager;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (OnInitialize != null) OnInitialize(Player.PlayerController.Instance.MaxHealth);
        }

        public void SetState(GameState newGameState)
        {
            if (newGameState == currentGameState)
                return;

            currentGameState = newGameState;
            OnGameStateChanged?.Invoke(newGameState);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetState(currentGameState == GameState.GamePlay ? GameState.Pause : GameState.GamePlay);
            }
        }
    }
}
