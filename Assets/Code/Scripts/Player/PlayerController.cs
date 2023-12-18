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
        
        [Space]
        public float mouseTurretSensitivity;
        public float gamepadTurretSensitivity;

        private bool useMouse;
        public int index;

        private Vector2 lastTurretInput;

        public static readonly List<InputDevice> Devices = new();
        
        public static event System.Action<PlayerController, Tank, DamageArgs, GameObject, Vector3, Vector3> KillEvent;
        public static event System.Action<PlayerController, Tank, DamageArgs, GameObject, Vector3, Vector3> DeathEvent;

        private void Awake()
        {
            inputAsset = Instantiate(inputAsset);
            inputAsset.devices = new InputDevice[0];

            tank = Instantiate(tank);
            tank.gameObject.SetActive(false);
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
                tank.Throttle = inputAsset.FindAction("Throttle").ReadValue<float>();
                tank.Turning = inputAsset.FindAction("Turning").ReadValue<float>();

                if (inputAsset.FindAction("Shoot").WasPerformedThisFrame()) tank.Shoot = true;

                DoTurretInput
                (
                    useMouse ?
                        Mouse.current.delta.ReadValue() * mouseTurretSensitivity :
                        inputAsset.FindAction("Turret").ReadValue<Vector2>() * gamepadTurretSensitivity
                );
            }
        }

        private void DoTurretInput(Vector2 input)
        {
            var delta = input.normalized - lastTurretInput.normalized;

            tank.TurretInput = -Vector3.Cross(input, delta).z / Time.deltaTime;
            lastTurretInput = input;
        }

        private void SetIndex(int index)
        {
            this.index = index;

            var device = Devices[index];
            if (device == Keyboard.current || device == Mouse.current)
            {
                inputAsset.devices = new InputDevice[] { Keyboard.current, Mouse.current };
                useMouse = true;
            }
            else
            {
                inputAsset.devices = new[] { device };
            }

            name = $"[{index + 1}: {device.name}]PlayerController";
            tank.name = $"{name}.Tank";
        }

        public bool TrySpawnPlayer(InputDevice device, out PlayerController player)
        {
            player = null;
            if (Devices.Contains(device)) return false;

            var index = Devices.Count;
            Devices.Add(device);

            player = Instantiate(this);
            player.SetIndex(index);
            return true;
        }

        public void SpawnTank(Vector3 spawnPoint)
        {
            tank.gameObject.SetActive(true);
            tank.transform.position = spawnPoint;
        }
    }
}