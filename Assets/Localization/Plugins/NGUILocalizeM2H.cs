﻿
#if NGUI_ENABLED

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/LocalizeM2H NGUI")]
public class NGUILocalizeM2H : MonoBehaviour
{
    public string key;
    public string sheet;

    [HideInInspector]
    [SerializeField]
    private UIInput input;

    void OnValidate()
    {
        UIWidget w = GetComponent<UIWidget>();
        UILabel lbl = w as UILabel;

        if (lbl != null)
            input = NGUITools.FindInParents<UIInput>(lbl.gameObject);
    }

    void OnEnable()
    {
        Localize();
    }

    public void ChangedLanguage(LanguageCode code)
    {
        Localize();
    }


    public void Localize()
    {
        UIWidget w = GetComponent<UIWidget>();
        UILabel lbl = w as UILabel;
        UISprite sp = w as UISprite;

        // If we still don't have a key, leave the value as blank
        string val = string.IsNullOrEmpty(key) ? "" : (!string.IsNullOrEmpty(sheet) ? Language.Get(key, sheet) : Language.Get(key));

        if (lbl != null)
        {
            // If this is a label used by input, we should localize its default value instead            
            if (input != null && input.label == lbl) input.defaultText = val;
            else lbl.text = val;
        }
        else if (sp != null)
        {
            sp.spriteName = val;
            sp.MakePixelPerfect();
        }
    }

}
#endif
