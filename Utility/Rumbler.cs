using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Utility
{
    public enum RumblePattern
    {
        Constant,
        Pulse,
        Linear
    }

    public class Rumbler : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private RumblePattern activeRumbePattern;
        private float rumbleDuration;
        private float pulseDuration;
        private float lowA;
        private float lowStep;
        private float highA;
        private float highStep;
        private float rumbleStep;
        private bool isMotorActive = false;

        // Public Methods
        public void RumbleConstant(float low, float high, float duration)
        {
            activeRumbePattern = RumblePattern.Constant;
            lowA = low;
            highA = high;
            rumbleDuration = Time.time + duration;
        }

        public void RumblePulse(float low, float high, float burstTime, float duration)
        {
            activeRumbePattern = RumblePattern.Pulse;
            lowA = low;
            highA = high;
            rumbleStep = burstTime;
            pulseDuration = Time.time + burstTime;
            rumbleDuration = Time.time + duration;
            isMotorActive = true;
            var g = GetGamepad();
            g?.SetMotorSpeeds(lowA, highA);
        }

        public void RumbleLinear(float lowStart, float lowEnd, float highStart, float highEnd, float duration)
        {
            activeRumbePattern = RumblePattern.Linear;
            lowA = lowStart;
            highA = highStart;
            lowStep = (lowEnd - lowStart) / duration;
            highStep = (highEnd - highStart) / duration;
            rumbleDuration = Time.time + duration;
        }

        public void StopRumble()
        {
            var gamepad = GetGamepad();
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0, 0);
            }
        }


        // Unity MonoBehaviors
        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            if (Time.time > rumbleDuration)
            {
                StopRumble();
                return;
            }

            var gamepad = GetGamepad();
            if (gamepad == null)
                return;

            switch (activeRumbePattern)
            {
                case RumblePattern.Constant:
                    gamepad.SetMotorSpeeds(lowA, highA);
                    break;

                case RumblePattern.Pulse:

                    if (Time.time > pulseDuration)
                    {
                        isMotorActive = !isMotorActive;
                        pulseDuration = Time.time + rumbleStep;
                        if (!isMotorActive)
                        {
                            gamepad.SetMotorSpeeds(0, 0);
                        }
                        else
                        {
                            gamepad.SetMotorSpeeds(lowA, highA);
                        }
                    }

                    break;
                case RumblePattern.Linear:
                    gamepad.SetMotorSpeeds(lowA, highA);
                    lowA += (lowStep * Time.deltaTime);
                    highA += (highStep * Time.deltaTime);
                    break;
                default:
                    break;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            StopRumble();
        }

        // Private helpers

        private Gamepad GetGamepad()
        {
            return Gamepad.all.FirstOrDefault(g => _playerInput.devices.Any(d => d.deviceId == g.deviceId));

            #region Linq Query Equivalent Logic
            //Gamepad gamepad = null;
            //foreach (var g in Gamepad.all)
            //{
            //    foreach (var d in _playerInput.devices)
            //    {
            //        if(d.deviceId == g.deviceId)
            //        {
            //            gamepad = g;
            //            break;
            //        }
            //    }
            //    if(gamepad != null)
            //    {
            //        break;
            //    }
            //}
            //return gamepad;
            #endregion
        }
    }
}
