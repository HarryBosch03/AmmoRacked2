using System.Collections;
using System.Collections.Generic;
using AmmoRacked2.Runtime.Health;
using AmmoRacked2.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AmmoRacked2.Runtime.Meta
{
    public class GameController : MonoBehaviour
    {
        public Gamemode gamemode;
        public InputAction joinAction;
        public Vector2 spawnMin;
        public Vector2 spawnMax;
        public bool spawnAll;
        public bool spawnImmediately;
        public List<int> scores = new();

        public PlayerController playerPrefab;
        public static List<PlayerController> players = new();

        private void OnEnable()
        {
            joinAction.Enable();
            joinAction.performed += OnJoinPerformed;

            PlayerController.KillEvent += OnPlayerKill;
            PlayerController.DeathEvent += OnPlayerDeath;
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

            PlayerController.KillEvent -= OnPlayerKill;
            PlayerController.DeathEvent -= OnPlayerDeath;
        }

        private void OnPlayerKill(PlayerController player, Tank tank, DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            scores[player.index] += gamemode.pointsOnKill;
        }

        private void OnPlayerDeath(PlayerController player, Tank tank, DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            scores[player.index] += gamemode.pointsOnDeath;
            
            if (gamemode.respawn)
            {
                StartCoroutine(RespawnRoutine(player, gamemode.respawnTime));
            }
        }

        private IEnumerator RespawnRoutine(PlayerController player, float time)
        {
            yield return new WaitForSeconds(time);
            player.SpawnTank(GetSpawnPoint());
        }

        private void OnJoinPerformed(InputAction.CallbackContext ctx)
        {
            var device = ctx.control.device;
            if (device is not Keyboard && device is not Mouse && device is not Gamepad) return;

            if (!playerPrefab.TrySpawnPlayer(device, out var player)) return;

            players.Add(player);
            scores.Add(0);
            
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

        private Vector3 GetSpawnPoint()
        {
            for (var i = 0; i < 1000; i++)
            {
                var point = new Vector3(Random.Range(spawnMin.x, spawnMax.x), 0.0f, Random.Range(spawnMin.y, spawnMax.y));
                if (Physics.CheckSphere(point, 2.0f, 0b1)) continue;
                return point;
            }

            return transform.position;
        }

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