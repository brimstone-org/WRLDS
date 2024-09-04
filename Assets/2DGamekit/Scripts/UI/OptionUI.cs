using UnityEngine;

namespace Gamekit2D
{
    public class OptionUI : MonoBehaviour
    {
        public void ExitPause()
        {
            PlayerCharacter.PlayerInstance.Unpause();
        }

        public void RestartLevel()
        {
            ExitPause();
            SceneController.RestartZone();
        }
    }
}