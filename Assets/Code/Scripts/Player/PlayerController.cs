using System;
using System.Collections.Generic;
using AmmoRacked2.Runtime.Health;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        public InputActionAsset inputAsset;
        public Tank tank;
        [TextArea] public string debug;

        [Space]
        public float mouseTurretSensitivity;
        public float gamepadTurretSensitivity;

        private bool useMouse;
        [HideInInspector] public Camera mainCamera;

        public IReadOnlyList<InputDevice> Devices => inputAsset.devices;
        public bool Disconnected => Devices.Count == 0;
        public Color Color { get; private set; }
        public int Index { get; private set; }

        public static bool FreezeInput { get; set; }
        public static event Action<PlayerController, Tank, DamageArgs, GameObject, Vector3, Vector3> KillEvent;
        public static event Action<PlayerController, Tank, DamageArgs, GameObject, Vector3, Vector3> DeathEvent;
        public static event Action<PlayerController, Tank> TankSpawnEvent;



        private void Awake()
        {
            tank = Instantiate(tank);
            tank.gameObject.SetActive(false);

            mainCamera = Camera.main;

            inputAsset = Instantiate(inputAsset);
            inputAsset.devices = new InputDevice[0];
        }

        private void OnEnable()
        {
            inputAsset.Enable();
            Tank.DeathEvent += OnTankDeath;
        }

        private void OnDisable()
        {
            inputAsset.Disable();
            Tank.DeathEvent -= OnTankDeath;
        }

        private void OnTankDeath(Tank tank, DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            if (tank == this.tank)
            {
                DeathEvent?.Invoke(this, tank, args, invoker, point, direction);
            }

            if (invoker.gameObject == this.tank.gameObject)
            {
                KillEvent?.Invoke(this, tank, args, invoker, point, direction);
            }
        }


        private void OnDestroy()
        {
            inputAsset.Disable();
            if (tank) Destroy(tank.gameObject);
        }

        private void Update()
        {
            if (tank)
            {
                PassInput();
            }
        }

        private void PassInput()
        {
            if (FreezeInput) return;
            
            tank.Throttle = inputAsset.FindAction("Throttle").ReadValue<float>();
            tank.Turning = inputAsset.FindAction("Turning").ReadValue<float>();

            tank.Shoot = inputAsset.FindAction("Shoot").IsPressed();

            DoTurretInput();
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

        public void Setup(int index, Color color)
        {
            Index = index;
            SetColor(color);
        }

        public void SpawnTank(Vector3 spawnPoint)
        {
            tank.gameObject.SetActive(true);
            tank.transform.position = spawnPoint;

            TankSpawnEvent?.Invoke(this, tank);
        }

        public PlayerController SpawnPlayer(InputDevice device)
        {
            var player = Instantiate(this);
            player.SetDevice(device);
            return player;
        }

        public void SetColor(Color color)
        {
            Color = color;
            tank.SetColor(color);
        }
    }
}