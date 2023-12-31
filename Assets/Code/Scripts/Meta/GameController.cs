using System;
using System.Collections;
using System.Collections.Generic;
using AmmoRacked2.Runtime.Health;
using AmmoRacked2.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace AmmoRacked2.Runtime.Meta
{
    public class GameController : MonoBehaviour
    {
        public Gamemode gamemode;
        public Vector2 spawnMin;
        public Vector2 spawnMax;
        public bool spawnAll;
        public bool spawnImmediately;
        public Tank[] tanks;
        [SerializeField] private List<int> scores = new();

        public PlayerController playerPrefab;
        public List<PlayerController> players = new();
        
        public float GameTime { get; private set; }
        public static readonly int[] TankSelection = new int[4];
        public float GameTimeLeft => gamemode.keepTime ? gamemode.timeLimitSeconds - GameTime : 0.0f;

        public static readonly InputDevice[] JoinedDevices = new InputDevice[4];
        public static GameController ActiveGameController { get; private set; }
        public static event Action<int, int, int> PlayerScoreChangeEvent;
        public static event Action GameEndEvent;

        public int GetScore(int playerIndex) => scores[playerIndex];

        public void SetScore(int playerIndex, int score)
        {
            var oldScore = scores[playerIndex];
            scores[playerIndex] = ValidateScore(score);
            PlayerScoreChangeEvent?.Invoke(playerIndex, score, oldScore);
        }

        private int ValidateScore(int score)
        {
            if (!gamemode.allowNegativeScore) score = Mathf.Max(0, score);
            return score;
        }

        public void IncrementScore(int playerIndex, int score) => SetScore(playerIndex, GetScore(playerIndex) + score);

        private void OnEnable()
        {
            if (ActiveGameController)
            {
                Destroy(this);
                return;
            }
            
            ActiveGameController = this;

            var playerCount = 0;
            foreach (var device in JoinedDevices)
            {
                if (device != null) playerCount++;
            }

            if (playerCount == 0)
            {
                var head = 0;
                if (Keyboard.current != null) JoinedDevices[head++] = Keyboard.current;

                foreach (var gamepad in Gamepad.all)
                {
                    JoinedDevices[head++] = gamepad;
                }
            }

            for (var i = 0; i < JoinedDevices.Length; i++)
            {
                JoinWithDevice(i);
            }

            PlayerController.KillEvent += OnPlayerKill;
            PlayerController.DeathEvent += OnPlayerDeath;
            
            GameTime = 0.0f;
        }
        
        private void OnDisable()
        {
            foreach (var player in players)
            {
                if (player) Destroy(player.gameObject);
            }

            players.Clear();

            PlayerController.KillEvent -= OnPlayerKill;
            PlayerController.DeathEvent -= OnPlayerDeath;

            if (ActiveGameController == this)
            {
                ActiveGameController = null;
            }

            for (var i = 0; i < JoinedDevices.Length; i++)
            {
                JoinedDevices[i] = null;
            }
        }

        private void Update()
        {
            GameTime += Time.deltaTime;
            if (GameTime > gamemode.timeLimitSeconds && gamemode.keepTime)
            {
                Time.timeScale = 0.0f;
                GameEndEvent?.Invoke();
            }
        }

        private void OnPlayerKill(PlayerController player, Tank tank, DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            IncrementScore(player.Index, gamemode.pointsOnKill);
        }

        private void OnPlayerDeath(PlayerController player, Tank tank, DamageArgs args, GameObject invoker, Vector3 point, Vector3 direction)
        {
            IncrementScore(player.Index, gamemode.pointsOnDeath);
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

        private void JoinWithDevice(int deviceIndex)
        {
            var device = JoinedDevices[deviceIndex];
            if (device is not Keyboard && device is not Mouse && device is not Gamepad) return;

            foreach (var controller in players)
            {
                if (controller is not PlayerController player) continue;
                foreach (var other in player.Devices)
                {
                    if (device == other) return;
                }
            }
            
            foreach (var controller in players)
            {
                if (controller is not PlayerController player) continue;
                if (player.Disconnected)
                {
                    player.SetDevice(device);
                    return;
                }
            }
            
            playerPrefab.tank = tanks[TankSelection[deviceIndex]];
            AddController(playerPrefab.SpawnPlayer(device));
        }

        private void AddController(PlayerController controller)
        {
            var index = players.Count;
            controller.Setup(index, gamemode.playerColors[index]);
            players.Add(controller);
            scores.Add(0);

            if (spawnImmediately)
            {
                controller.SpawnTank(GetSpawnPoint());
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