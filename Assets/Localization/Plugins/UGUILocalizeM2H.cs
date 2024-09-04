using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Text))]
[AddComponentMenu("UI/LocalizeM2H UGUI")]
public class UGUILocalizeM2H : MonoBehaviour
{
    public string key = "";
    public string sheet = "";

    void OnEnable()
    {
        Localize();
    }

    void Start()
    {
        Localize();
    }

    public void ChangedLanguage(LanguageCode code)
    {
        Localize();
    }


    public void Localize()
    {
        Text lbl = GetComponent<Text>();
        Texture sp = GetComponent<Texture>();

        // If we still don't have a key, leave the value as blank
        string val = string.IsNullOrEmpty(key) ? "" : (!string.IsNullOrEmpty(sheet) ? Language.Get(key, sheet) : Language.Get(key));

        if (lbl != null)
        {
            // If this is a label used by input, we should localize its default value instead
            //InputField input = GetComponent<InputField>();
            //if (input != null && input.text == lbl) input.defaultText = val;
            //else
            lbl.text = val;
        }
        else if (sp != null)
        {
            sp.name = val;
            //sp.MakePixelPerfect();
        }
    }

}

