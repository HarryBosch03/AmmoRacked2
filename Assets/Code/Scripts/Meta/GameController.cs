using System.Collections.Generic;
using AmmoRacked2.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime.Meta
{
    public class GameController : MonoBehaviour
    {
        public InputAction joinAction;
        public Vector2 spawnMin;
        public Vector2 spawnMax;
        public bool spawnAll;
        public bool spawnImmediately;

        public PlayerController playerPrefab;
        public static List<PlayerController> players = new();

        private void OnEnable()
        {
            joinAction.Enable();
            joinAction.performed += OnJoinPerformed;
        }

        private void OnDisable()
        {
            joinAction.performed -= OnJoinPerformed;
            joinAction.Disable();

            foreach (var player in players)
            {
                if (player) Destroy(player.gameObject);
            }

            players.Clear();
        }

        private void OnJoinPerformed(InputAction.CallbackContext ctx)
        {
            var device = ctx.control.device;
            if (device is not Keyboard && device is not Mouse && device is not Gamepad) return;

            if (!playerPrefab.TrySpawnPlayer(device, out var player)) return;
            
            players.Add(player);
            if (spawnImmediately)
            {
                player.SpawnTank(GetSpawnPoint());
            }
        }

        public void OnValidate()
        {
            if (spawnAll)
            {
                spawnAll = false;
                SpawnAll();
            }
        }

        private void SpawnAll()
        {
            foreach (var player in players)
            {
                player.SpawnTank(GetSpawnPoint());
            }
        }

        private Vector3 GetSpawnPoint() { return new Vector3(Random.Range(spawnMin.x, spawnMax.x), 0.0f, Random.Range(spawnMin.y, spawnMax.y)); }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;

            var center = (spawnMin + spawnMax) * 0.5f;
            var size = spawnMax - spawnMin;

            Gizmos.DrawSphere(new Vector3(spawnMin.x, 1.0f, spawnMin.y), 0.2f);
            Gizmos.DrawSphere(new Vector3(spawnMax.x, 1.0f, spawnMax.y), 0.2f);
            Gizmos.DrawWireCube(new Vector3(center.x, 1.0f, center.y), new Vector3(size.x, 0.0f, size.y));
        }
    }
}