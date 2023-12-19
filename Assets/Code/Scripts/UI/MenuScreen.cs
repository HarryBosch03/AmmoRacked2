using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime.UI
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class MenuScreen : MonoBehaviour
    {
        private InputAction backAction;

        public const int MainMenu = 0;
        public const int StartLocal = 1;
        
        private MainMenuParent controller;

        [HideInInspector] public int previousMenu;

        protected virtual void Awake()
        {
            controller = GetComponentInParent<MainMenuParent>();

            backAction = new InputAction();
            backAction.AddBinding("<Keyboard>/escape");
            backAction.AddBinding("<Gamepad>/buttonEast");
            backAction.Enable();
        }

        protected virtual void OnEnable()
        {
            backAction.performed += Back;
        }

        protected virtual void OnDisable()
        {
            backAction.performed -= Back;
        }

        private void Back(InputAction.CallbackContext ctx)
        {
            controller.Back();
        }

        protected void SetMenu(int index)
        {
            controller.SetMenu(index);
        }

        protected UnityAction SetMenuCallback(int index) => () => SetMenu(index);
    }
}