using System.Collections;
using AmmoRacked2.Runtime.Meta;
using AmmoRacked2.Runtime.Player;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace AmmoRacked2.Runtime.UI
{
    public class ScoreUI : MonoBehaviour
    {
        public AnimationCurve scoreAnimation;
        public float scoreAnimationAmplitude;
        public float scoreAnimationDuration;
        
        private ScoreTracker[] scoreTrackers;

        private void Start()
        {
            var scores = transform.Find("Scores");
            scores.gameObject.SetActive(true);
            
            scoreTrackers = new ScoreTracker[scores.childCount];
            for (var i = 0; i < scoreTrackers.Length; i++)
            {
                scoreTrackers[i] = new ScoreTracker(this, transform, i);
            }
        }

        private void OnDestroy()
        {
            foreach (var e in scoreTrackers) e.Dispose();
        }

        private class ScoreTracker
        {
            private const string TextTemplate = "{0}\n<size=30%>PLAYER {1}</size>";

            private Transform root;
            private TMP_Text text;
            private Image colorBand;
            private int index;
            private ScoreUI parent;
            
            public ScoreTracker(ScoreUI parent, Transform canvas, int index)
            {
                this.parent = parent;
                this.index = index;
                root = canvas.Find($"Scores/P{index + 1}");

                text = root.Find<TMP_Text>("Text");
                colorBand = root.Find<Image>("Color");
                
                GameController.PlayerScoreChangeEvent += OnPlayerScoreChanged;
                PlayerController.TankSpawnEvent += OnPlayerTankSpawned;
                Update();
            }

            public void Dispose()
            {
                GameController.PlayerScoreChangeEvent -= OnPlayerScoreChanged;
                PlayerController.TankSpawnEvent -= OnPlayerTankSpawned;
            }

            private void OnPlayerTankSpawned(PlayerController player, Tank tank)
            {
                if (player.Index != index) return;
                Update();
            }

            private void OnPlayerScoreChanged(int playerIndex, int newScore, int oldScore)
            {
                if (playerIndex != index) return;
                if (newScore > oldScore)
                {
                    parent.StartCoroutine(AnimateRoutine());
                }
                
                Update();
            }

            private IEnumerator AnimateRoutine()
            {
                var p = 0.0f;
                while (p < 1.0f)
                {
                    var v = 1.0f + (parent.scoreAnimation.Evaluate(p) - 1.0f) * parent.scoreAnimationAmplitude;
                    root.transform.localScale = Vector3.one * v;
                    
                    p += Time.deltaTime / parent.scoreAnimationDuration;
                    yield return null;
                }

                root.transform.localScale = Vector3.one;
            }

            private void Update()
            {
                var gameController = GameController.ActiveGameController;
                if (!gameController || index >= gameController.players.Count || !gameController.gamemode.keepScore)
                {
                    Hide();
                    return;
                }

                Show();

                var score = gameController.GetScore(index);
                text.text = string.Format(TextTemplate, score, index);
                colorBand.color = gameController.players[index].Color;
            }

            public void Show()
            {
                if (root) root.gameObject.SetActive(true);
            }

            public void Hide()
            {
                if (root) root.gameObject.SetActive(false);
            }
        }
    }
}