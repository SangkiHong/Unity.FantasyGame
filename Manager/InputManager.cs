using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SK.Manager
{
    public enum InputState
    {
        onStart,
        onPerform,
        onEnd
    }

    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public UnityAction<Vector2> Input_Move;
        public UnityAction<InputState> Input_Button_Jump;
        public UnityAction Input_Button_Dodge;
        public UnityAction<InputState> Input_Button_Attack;
        public UnityAction<InputState> Input_Button_Shield;
        public UnityAction<InputState> Input_Button_Interacting;
        public UnityAction<InputState> Input_Button_Targeting;

        [SerializeField] private PlayerInput playerInput;
        
        private Vector2 _movement;
        private bool _isOnControl = true;
        private bool _isRunning;

        public Vector2 Movement 
        { 
            get => _movement;
            set => _movement = value;
        }
        public bool IsOnControl => _isOnControl;
        public bool IsRunning => _isRunning;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(this);
                return;
            }

            // 인풋 액션 초기화
            InputAction inputAction_Move = playerInput.actions["Move"];
            inputAction_Move.performed += Move;
            inputAction_Move.canceled += Move;

            InputAction inputAction_Run = playerInput.actions["Run"];
            inputAction_Run.started += (x) => { _isRunning = true; };
            inputAction_Run.canceled += (x) => { _isRunning = false; };

            InputAction inputAction_Jump = playerInput.actions["Jump"];
            inputAction_Jump.started += Jump;
            inputAction_Jump.canceled += Jump;

            InputAction inputAction_Dodge = playerInput.actions["Dodge"];
            inputAction_Dodge.started += Dodge;

            InputAction inputAction_Attack = playerInput.actions["Attack"];
            inputAction_Attack.started += Attack;
            inputAction_Attack.canceled += Attack;

            InputAction inputAction_Shield = playerInput.actions["Shield"];
            inputAction_Shield.started += Shield;
            inputAction_Shield.canceled += Shield;

            InputAction inputAction_Interacting = playerInput.actions["Interacting"];
            inputAction_Interacting.started += Interacting;
            inputAction_Interacting.canceled += Interacting;

            InputAction inputAction_Targeting = playerInput.actions["Targeting"];
            inputAction_Targeting.started += Targeting;
            inputAction_Targeting.canceled += Targeting;
        }

        private void Move(InputAction.CallbackContext context)
        {
            if (context.performed)
                _movement = context.ReadValue<Vector2>();
            else if (context.canceled)
                _movement = Vector3.zero;
        }

        private void Jump(InputAction.CallbackContext context)
        {
            if (context.started) Input_Button_Jump?.Invoke(InputState.onStart);
            else if (context.canceled) Input_Button_Jump?.Invoke(InputState.onEnd);
        }

        private void Dodge(InputAction.CallbackContext context)
            => Input_Button_Dodge?.Invoke();

        private void Attack(InputAction.CallbackContext context)
        {
            if (context.started) Input_Button_Attack?.Invoke(InputState.onStart);
            else if (context.canceled) Input_Button_Attack?.Invoke(InputState.onEnd);
        }

        private void Shield(InputAction.CallbackContext context)
        {
            if (context.started) Input_Button_Shield?.Invoke(InputState.onStart);
            else if (context.canceled) Input_Button_Shield?.Invoke(InputState.onEnd);
        }

        private void Interacting(InputAction.CallbackContext context)
        {
            if (context.started) Input_Button_Interacting?.Invoke(InputState.onStart);
            else if (context.canceled) Input_Button_Interacting?.Invoke(InputState.onEnd);
        }

        private void Targeting(InputAction.CallbackContext context)
        {
            if (context.started) Input_Button_Targeting?.Invoke(InputState.onStart);
            else if (context.canceled) Input_Button_Targeting?.Invoke(InputState.onEnd);
        }

        public void SetControlState(bool set)
            => _isOnControl = set;
    }
}