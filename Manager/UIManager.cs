﻿using UnityEngine;
using UnityEngine.UI;
using SK.Player;

namespace SK.Manager
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
            Player.PlayerController.Instance.OnDamagedReceived += UpdatePlayerHealth;
        }


        public void Initialize(int playerMaxHealth)
        {
            healthSlider.maxValue = playerMaxHealth;
            healthSlider.value = playerMaxHealth;
        }

        public void UpdatePlayerHealth(int currentHealth)
        {
            healthSlider.value = currentHealth;
        }

        public void OnInteractButton(bool isOn) => interactButton.SetActive(isOn);
        

        private void OnDestroy()
        {
            GameManager.Instance.OnInitialize -= Initialize;
            Player.PlayerController.Instance.OnDamagedReceived -= UpdatePlayerHealth;
        }
    }
}