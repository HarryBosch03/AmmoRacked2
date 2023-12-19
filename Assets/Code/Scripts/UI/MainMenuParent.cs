
using AmmoRacked2.Runtime.Utility;
using UnityEngine;

namespace AmmoRacked2.Runtime.UI
{
    [SelectionBase, DisallowMultipleComponent]
    public class MainMenuParent : MonoBehaviour
    {
        private MenuScreen[] menus;

        private int menuIndex;
        
        private void Awake()
        {
            menus = new MenuScreen[transform.childCount - 1];
            for (var i = 0; i < menus.Length; i++)
            {
                menus[i] = transform.GetChild(i + 1).GetComponent<MenuScreen>();
            }
            
            foreach (var e in menus) e.gameObject.SetActive(false);

            SetMenu(0);
        }

        public void SetMenu(int index)
        {
            menus[menuIndex].gameObject.SetActive(false);
            
            menus[index].previousMenu = menuIndex;
            menus[index].gameObject.SetActive(true);
            menuIndex = index;
        }

        public void Back()
        {
            SetMenu(menus[menuIndex].previousMenu);
        }
    }
}