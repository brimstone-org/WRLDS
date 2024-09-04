using System;

using UnityEngine;

[System.Serializable]
public class LocalizationSettings : ScriptableObject
{
    public string[] sheetTitles;
    public string[] usedLanguages;
    public string[] usedKeys;

    public bool useSystemLanguagePerDefault = true;
    public string defaultLangCode = "EN";

#if UNITY_EDITOR
    public string localizationURL = "";
#endif

    //GENERAL
    public static LanguageCode GetLanguageEnum(string langCode)
    {
        langCode = langCode.ToUpper();
        foreach (LanguageCode item in Enum.GetValues(typeof(LanguageCode)))
        {
            if (item + "" == langCode)
            {
                return item;
            }
        }
        //global::Logger.LogError("ERORR: There is no language: [" + langCode + "]");
        return LanguageCode.EN;
    }
}
