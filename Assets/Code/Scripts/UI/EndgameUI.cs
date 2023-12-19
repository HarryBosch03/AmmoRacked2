using System;
using System.Collections.Generic;
using System.Linq;
using AmmoRacked2.Runtime.Meta;
using AmmoRacked2.Runtime.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AmmoRacked2.Runtime.UI
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class EndgameUI : MonoBehaviour
    {
        public Color[] badgeColorsPrimary = new Color[4];
        public Color[] badgeColorsSecondary = new Color[4];
        
        private Transform root;
        private TMP_Text title;

        private List<PlayerController> placements;

        private void Start()
        {
            root = transform.Find("EndScreen");
            title = root.Find<TMP_Text>("WinText");
            
            var game = GameController.ActiveGameController;
            var playerCount = game.players.Count;
            var prefab = root.Find("ScoreBoard/P1");
            for (var i = 1; i < playerCount; i++)
            {
                Instantiate(prefab, prefab.parent);
            }

            root.gameObject.SetActive(false);
            GameController.GameEndEvent += OnGameEnd;

            var button = root.Find<Button>("Button");
            button.onClick.AddListener(ReturnToMenu);
        }

        private void ReturnToMenu()
        {
            SceneManager.LoadScene(0);
        }

        private void OnDisable()
        {
            GameController.GameEndEvent -= OnGameEnd;
            Time.timeScale = 1.0f;
        }

        private void OnGameEnd()
        {
            
            var game = GameController.ActiveGameController;
            placements = game.players.OrderBy(e => -game.GetScore(e.Index)).ToList();
            root.gameObject.SetActive(true);
            
            title.text = $"Player {placements[0].Index + 1} Wins";
            
            var playerCount = game.players.Count;
            for (var i = 0; i < playerCount; i++)
            {
                SetupPillar(i);
            }
        }

        public void SetupPillar(int index)
        {
            var primaryColor = badgeColorsPrimary[index];
            var secondaryColor = badgeColorsSecondary[index];
            var root = this.root.Find("ScoreBoard").GetChild(index);

            var mainText = root.Find<TMP_Text>("Text");
            var player = placements[index];
            var score = GameController.ActiveGameController.GetScore(player.Index);
            mainText.text = $"<size=70%>Player {player.Index + 1}</size>\n{score} Points";
            mainText.color = primaryColor;
            
            var placementText = root.Find<TMP_Text>("Badge/Text");
            placementText.text = (index + 1).ToString();

            var badge = new Image[2];
            badge[0] = root.Find<Image>("Badge");
            badge[1] = root.Find<Image>("Badge/Badge-Underlay");

            badge[0].color = primaryColor;
            badge[1].color = secondaryColor;
        }
    }
}