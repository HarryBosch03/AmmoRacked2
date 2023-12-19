using AmmoRacked2.Runtime.Meta;
using AmmoRacked2.Runtime.Player;
using UnityEngine;

namespace AmmoRacked2.Runtime.UI
{
    public class PlayerTankPreview : MonoBehaviour
    {
        private int index;

        private void Awake()
        {
            index = transform.GetSiblingIndex();
        }

        private void Update()
        {
            var tankIndex = GameController.TankSelection[index];
            var hatIndex = PlayerController.HatSelection[index] - 1;

            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(i == tankIndex);
            }

            var tank = transform.GetChild(tankIndex);
            var hatParent = tank.Find("Hats");
            for (var i = 0; i < hatParent.childCount; i++)
            {
                hatParent.GetChild(i).gameObject.SetActive(i == hatIndex);
            }
        }
    }
}