
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AmmoRacked2.Runtime.Utility
{
    public class ButtonHelper
    {
        private Button[] buttons;
        private int head;
        
        public ButtonHelper(Button button, int count)
        {
            buttons = new Button[count];

            if (count == 0) return;
            
            for (var i = 1; i < buttons.Length; i++)
            {
                buttons[i] = Object.Instantiate(button, button.transform.parent);
            }
            buttons[0] = button;
        }

        public void Set(string name, UnityAction callback)
        {
            var button = buttons[head++];
            var text = button.transform.Find<TMP_Text>("Text");
            text.text = name;
            button.onClick.AddListener(callback);
        }
    }
}