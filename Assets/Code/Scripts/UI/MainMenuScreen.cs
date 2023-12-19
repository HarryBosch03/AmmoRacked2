using AmmoRacked2.Runtime.Utility;
using UnityEngine.UI;

namespace AmmoRacked2.Runtime.UI
{
    public class MainMenuScreen : MenuScreen
    {
        protected override void Awake()
        {
            base.Awake();
            
            var helper = new ButtonHelper(transform.Find<Button>("Button"), 2);
            helper.Set("Local", SetMenuCallback(StartLocal));
            helper.Set("Quit", Quit);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
