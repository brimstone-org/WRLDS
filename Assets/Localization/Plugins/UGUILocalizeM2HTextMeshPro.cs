//#define USE_TEXTMESHPRO

#if USE_TEXTMESHPRO

using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
[AddComponentMenu("UI/LocalizeM2H UGUI TMPro")]
public class UGUILocalizeM2HTextMeshPro : MonoBehaviour
{
    public string key;
    public string sheet;

    void OnEnable()
    {
        Localize();
    }

    void Start()
    {
        Localize();
    }

    void OnValidate()
    {
        Localize();
    }

    public void ChangedLanguage(LanguageCode code)
    {
        Localize();
    }


    public void Localize()
    {
        var lbl = GetComponent<TMPro.TextMeshProUGUI>();

        // If we still don't have a key, leave the value as blank
        string val = string.IsNullOrEmpty(key) ? "" : (!string.IsNullOrEmpty(sheet) ? Language.Get(key, sheet) : Language.Get(key));

        if (lbl != null)
        {
            lbl.text = val;
        }
    }

}

#endif