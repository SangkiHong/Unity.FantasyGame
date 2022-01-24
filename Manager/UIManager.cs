using UnityEngine;
using UnityEngine.UI;
using Sangki.Player;

namespace Sangki.Manager
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private Slider healthSlider;
        [SerializeField]
        private GameObject interactButton;

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

        public void OnInteractButton(bool isOn) => interactButton.SetActive(isOn);
        

        private void OnDestroy()
        {
            GameManager.Instance.OnInitialize -= Initialize;
            PlayerController.Instance.OnDamagedReceived -= UpdatePlayerHealth;
        }
    }
}