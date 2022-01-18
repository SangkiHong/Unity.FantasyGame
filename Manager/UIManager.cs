using UnityEngine;
using UnityEngine.UI;
using Sangki.Player;

namespace Sangki
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private Slider healthSlider;

        private void OnEnable()
        {
            GameManager.Instance.OnInitialize += Initialize;
            PlayerController.Instance.OnDamagedReceived += UpdatePlayerHealth;
        }


        public void Initialize(int playerMaxHealth)
        {
            healthSlider.maxValue = playerMaxHealth;
            healthSlider.value = playerMaxHealth;
        }

        public void UpdatePlayerHealth(int health)
        {
            healthSlider.value = health;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnInitialize -= Initialize;
            PlayerController.Instance.OnDamagedReceived -= UpdatePlayerHealth;
        }
    }
}