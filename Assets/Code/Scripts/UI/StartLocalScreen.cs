
using System.Collections;
using AmmoRacked2.Runtime.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AmmoRacked2.Runtime.UI
{
    [SelectionBase, DisallowMultipleComponent]
    public class StartLocalScreen : MenuScreen
    {
        public InputAction joinAction;
        public InputAction changeTankAction;
        public Gamemode gamemode;
        public Texture[] tankPortraits;
        
        private TMP_Text text;
        private PlayerManager[] players;
        private Coroutine startRoutine;

        private const string JoinText = "PRESS SPACE/A TO JOIN";
        
        protected override void Awake()
        {
            base.Awake();

            text = transform.Find<TMP_Text>("Text");
            
            var prefab = transform.Find("Players/P1");
            players = new PlayerManager[4];
            for (var i = 1; i < 4; i++)
            {
                players[i] = new PlayerManager(this, i, Instantiate(prefab, prefab.transform.parent));
                players[i].root.name = $"P{i + 1}";
            }
            players[0] = new PlayerManager(this, 0, prefab);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            joinAction.Enable();
            joinAction.performed += OnJoin;

            changeTankAction.Enable();
            changeTankAction.performed += OnChangeTankPerformed;

            for (var i = 0; i < 4; i++)
            {
                GameController.JoinedDevices[i] = null;
                players[i].Reset();
            }
            
            text.text = JoinText;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            joinAction.Disable();
            joinAction.performed -= OnJoin;
            
            changeTankAction.Disable();
            changeTankAction.performed -= OnChangeTankPerformed;
        }

        private void OnChangeTankPerformed(InputAction.CallbackContext ctx)
        {
            var device = ctx.control.device;
            for (var i = 0; i < GameController.JoinedDevices.Length; i++)
            {
                var other = GameController.JoinedDevices[i];
                if (device != other) continue;

                players[i].ChangeTankSelection(Mathf.RoundToInt(ctx.ReadValue<float>()));
                
                break;
            }
        }

        private void OnJoin(InputAction.CallbackContext ctx)
        {
            var device = ctx.control.device;
            for (var i = 0; i < GameController.JoinedDevices.Length; i++)
            {
                var other = GameController.JoinedDevices[i];
                if (other == device)
                {
                    ToggleReady(i);
                    return;
                }
            }
            
            for (var i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (player.occupied) continue;
                
                player.Join(device);
                return;
            }
        }

        private void ToggleReady(int index)
        {
            players[index].SetReady(!players[index].Ready);

            var playerCount = 0;
            foreach (var e in players)
            {
                if (e.occupied) playerCount++;
            }
            
            var readyCount = 0;
            foreach (var e in players)
            {
                if (e.Ready) readyCount++;
            }

            if (readyCount == playerCount && readyCount > 1)
            {
                startRoutine = StartCoroutine(StartGame());
            }
            else if (startRoutine != null)
            {
                StopCoroutine(startRoutine);
                startRoutine = null;
                text.text = JoinText;
            }
        }

        private IEnumerator StartGame()
        {
            for (var i = 3; i >= 0; i--)
            {
                text.text = $"STARTING IN {i}";
                yield return new WaitForSeconds(1.0f);
            }

            SceneManager.LoadScene(1);
        }

        private class PlayerManager
        {
            public int index;
            public StartLocalScreen screen;
            public Transform root;
            public RawImage portrait;
            public bool occupied;
            public TMP_Text text;
            public Image[] borders;

            public bool Ready { get; private set; }

            public PlayerManager(StartLocalScreen screen, int index, Transform root)
            {
                this.screen = screen;
                this.index = index;
                this.root = root;

                portrait = root.Find<RawImage>("TankSelect");

                borders = new Image[2];
                borders[0] = root.Find<Image>("Border0");
                borders[1] = root.Find<Image>("Border1");
                
                text = root.Find<TMP_Text>("Text");

                Reset();
            }
            
            public void Reset()
            {
                occupied = false;
                Ready = false;
                
                borders[0].color = screen.gamemode.playerColors[index];
                borders[1].color = borders[0].color;

                root.gameObject.SetActive(false);
                Update();
            }

            private void Update()
            {
                text.text = $"Player {index + 1}\n<size=50%>{(Ready ? "Ready" : "Not Ready")}";
                portrait.texture = screen.tankPortraits[GameController.TankSelection[index]];
            }

            public void ChangeTankSelection(int delta)
            {
                var tankIndex = GameController.TankSelection[index] + delta;
                var c = screen.tankPortraits.Length;
                tankIndex = (tankIndex % c + c) % c;
                GameController.TankSelection[index] = tankIndex;

                Update();
            }

            public void SetReady(bool ready)
            {
                Ready = ready;
                Update();
            }
            
            public void Join(InputDevice device)
            {
                root.gameObject.SetActive(true);
                GameController.JoinedDevices[index] = device;
                occupied = true;
                Ready = false;
                Update();
            }
        }
    }
}