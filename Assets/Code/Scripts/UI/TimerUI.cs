
using System;
using AmmoRacked2.Runtime.Meta;
using TMPro;
using UnityEngine;

namespace AmmoRacked2.Runtime.UI
{
    public class TimerUI : MonoBehaviour
    {
        private TMP_Text text;
        private Transform root;

        private void Awake()
        {
            root = transform.Find("Timer");
            text = root.Find<TMP_Text>("Text");
        }

        private void OnEnable()
        {
            root.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            root.gameObject.SetActive(false);
        }

        private void Update()
        {
            var controller = GameController.ActiveGameController;
            if (!controller || !controller.gamemode.keepTime)
            {
                enabled = false;
            }
            
            var timespan = TimeSpan.FromSeconds(controller.GameTimeLeft);
            text.text = timespan.ToString("%m\\:ss");
        }
    }
}