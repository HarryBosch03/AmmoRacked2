using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class PlayerController : GenericController
    {
        public static readonly Color[] PlayerColors =
        {
            new Color(1f, 0.54f, 0.12f),
            new Color(0.5f, 1f, 0.17f),
            new Color(0.22f, 0.71f, 1f),
            new Color(0.68f, 0.17f, 1f),
        };
        
        public InputActionAsset inputAsset;
        [TextArea] public string debug;

        [Space]
        public float mouseTurretSensitivity;
        public float gamepadTurretSensitivity;

        private bool useMouse;

        public IReadOnlyList<InputDevice> Devices => inputAsset.devices;
        public bool Disconnected => Devices.Count == 0;
        public Color Color => PlayerColors[Index];

        protected override void Awake()
        {
            base.Awake();
            
            inputAsset = Instantiate(inputAsset);
            inputAsset.devices = new InputDevice[0];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            inputAsset.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            inputAsset.Disable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            inputAsset.Disable();
        }

        private void Update()
        {
            if (tank)
            {
                tank.Throttle = inputAsset.FindAction("Throttle").ReadValue<float>();
                tank.Turning = inputAsset.FindAction("Turning").ReadValue<float>();

                tank.Shoot = inputAsset.FindAction("Shoot").IsPressed();

                DoTurretInput();
            }
        }

        private void DoTurretInput()
        {
            Vector2 screenPos;
            if (useMouse)
            {
                screenPos = Mouse.current.position.ReadValue();
            }
            else
            {
                screenPos = mainCamera.WorldToScreenPoint(tank.transform.position);
                screenPos += inputAsset.FindAction("Turret").ReadValue<Vector2>() * 200.0f;
            }

            var ray = mainCamera.ScreenPointToRay(screenPos);
            var plane = new Plane(Vector3.up, tank.turret.position);
            if (plane.Raycast(ray, out var enter))
            {
                tank.AimPosition = ray.GetPoint(enter);
            }
        }

        public void SetDevice(InputDevice device)
        {
            if (device == Keyboard.current || device == Mouse.current)
            {
                inputAsset.devices = new InputDevice[] { Keyboard.current, Mouse.current };
                useMouse = true;
            }
            else
            {
                inputAsset.devices = new[] { device };
            }
        }
        
        public PlayerController SpawnPlayer(InputDevice device)
        {
            var player = Instantiate(this);
            player.SetDevice(device);
            return player;
        }
    }
}