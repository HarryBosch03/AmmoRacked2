using AmmoRacked2.Runtime.Player;
using AmmoRacked2.Runtime.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AmmoRacked2.Runtime.UI
{
    public class PauseMenu : MonoBehaviour
    {
        public InputAction pauseAction;

        private Transform root;
        private bool paused;

        private void Awake()
        {
            root = transform.Find("Pause");

            var button = root.Find("Buttons").GetChild(0).GetComponent<Button>();
            var helper = new ButtonHelper(button, 2);
            helper.Set("Resume", () => SetPause(false));
            helper.Set("Quit", Quit);
            
            SetPause(false);
        }

        private void Quit()
        {
            SceneManager.LoadScene(0);
        }

        private void OnEnable()
        {
            pauseAction.Enable();
            pauseAction.performed += TogglePause;
        }

        private void OnDisable()
        {
            pauseAction.performed -= TogglePause;
            pauseAction.Disable();
        }

        private void TogglePause(InputAction.CallbackContext ctx) => TogglePause();
        private void TogglePause() { SetPause(!paused); }

        private void SetPause(bool state)
        {
            paused = state;
            
            root.gameObject.SetActive(state);
            Time.timeScale = paused ? 0.0f : 1.0f;
            PlayerController.FreezeInput = paused;
        }
    }
}