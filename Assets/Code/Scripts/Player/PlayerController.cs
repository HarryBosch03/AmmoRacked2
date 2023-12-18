using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        public Tank tank;
        public float controllerTurretSensitivity;
        
        public InputActionAsset inputAsset;
        private int index;

        private Vector2 lastTurretInput;

        public static readonly List<InputDevice> Devices = new();

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
        }

        private void OnDisable()
        {
            inputAsset.Disable();
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
                DoTurretInput();
            }
        }

        private void DoTurretInput()
        {
            var input = inputAsset.FindAction("Turret").ReadValue<Vector2>();

            var a0 = Mathf.Atan2(lastTurretInput.y, lastTurretInput.x);
            var a1 = Mathf.Atan2(input.y, input.x);
            var da = Mathf.DeltaAngle(a0 * Mathf.Rad2Deg, a1 * Mathf.Rad2Deg) * Mathf.Deg2Rad;

            var output = Vector2.zero;
            output.x = (float.IsFinite(da) ? -da * input.magnitude : 0.0f) * controllerTurretSensitivity;

            tank.TurretInput = output;
            lastTurretInput = input;
        }

        private void SetIndex(int index)
        {
            this.index = index;
            
            var device = Devices[index];
            if (device == Keyboard.current || device == Mouse.current)
            {
                inputAsset.devices = new InputDevice[] { Keyboard.current, Mouse.current };
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