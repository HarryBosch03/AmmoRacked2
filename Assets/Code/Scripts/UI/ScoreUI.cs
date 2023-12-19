using AmmoRacked2.Runtime.Meta;
using AmmoRacked2.Runtime.Player;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace AmmoRacked2.Runtime.UI
{
    public class ScoreUI : MonoBehaviour
    {
        private class ScoreTracker
        {
            public const string TextTemplate = "{0}\n<size=30%>PLAYER {1}</size>";
            
            public Transform root;
            public TMP_Text text;
            public Image colorBand;
            public int index;

            public ScoreTracker(Transform canvas, int index)
            {
                this.index = index;
                root = canvas.Find($"Scores/P{index}");

                text = root.Find<TMP_Text>("Text");
                colorBand = root.Find<Image>("Color");
            }
            
            public void Update()
            {
                var gameController = GameController.ActiveGameController;
                if (!gameController || index >= gameController.players.Count || gameController.players[index] is not PlayerController player)
                {
                    Disable();
                }

                Enable();

                var score = gameController.scores[index];
                text.text = string.Format(TextTemplate, score, index);
                colorBand = gameController.players[index].Color;
            }

            public void Enable() => root.gameObject.SetActive(true);
            public void Disable() => root.gameObject.SetActive(false);
        }
    }
}