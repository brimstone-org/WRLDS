using TMPro;
using UnityEngine;

namespace _DPS
{
    public class InfoPost : MonoBehaviour
    {
        public int InfoPostId;
        public string InfoString;
        public TextMeshPro InfoText;

        void Start()
        {
            if (InfoText == null)
            {
                InfoText = GetComponent<TextMeshPro>();
            }
        }

        public void DisplayText()
        {
            GameManager.StopBall?.Invoke();
            if (InfoText == null) return;
            InfoText.text = InfoString;
            //InfoText.autoSizeTextContainer = true;
        }

        public void HideText()
        {
            if (InfoText == null) return;
            InfoText.text = "";
        }
    }
}
