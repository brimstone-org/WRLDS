// Localization pacakge by Mike Hergaarden - M2H.nl
// DOCUMENTATION: http://www.m2h.nl/files/LocalizationPackage.pdf
// Thank you for buying this package!

//Version 1.01 - 04-07-2011

using UnityEngine;
using System.Globalization;

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public static class Language
{

    /* Want to replace all string calls with the generated enums Run this visual studio find&replace regexp
     * 1:
     * FIND:    Language\.Get\("([a-zA-Z0-9_]+)"\)
     * REPLACE: Language.Get(LocalizationKeys.$1)
     * 
     * 2:
     * FIND:    Language\.Get\("([a-zA-Z0-9_]+)", "([a-zA-Z0-9_]+)"\)
     * REPLACE: Language.Get(LocalizationKeys.$1, LocalizationSheets.$2)
     */

    //For settings, see TOOLS->LOCALIZATION

    public static string settingsAssetPath = "Assets/Localization/Resources/Languages/LocalizationSettings.asset";
    public static string generatedCodePath = "Assets/Localization/Plugins/GeneratedLocalizationEnums.cs";
    
    public static LocalizationSettings settings
	{
		get
		{
			//automatically load settings from resources if the pointer is null (to avoid null-ref-exceptions!)
			if(_settings == null)
			{
				string settingsFile = "Languages/" + System.IO.Path.GetFileNameWithoutExtension(settingsAssetPath);
				_settings = (LocalizationSettings)Resources.Load(settingsFile, typeof(LocalizationSettings));
			}
			return _settings;
		}
	}
	private static LocalizationSettings _settings = null;
	
	//Privates
	static List<string> availableLanguages;
	static LanguageCode currentLanguage = LanguageCode.N;
	
	
	static Dictionary<string, Dictionary<string, string>> currentEntrySheets;
	
	static Language()
	{
		//        if(settings == null)
		//            settings = (LocalizationSettings)Resources.Load("Languages/" + System.IO.Path.GetFileNameWithoutExtension(settingsAssetPath), typeof(LocalizationSettings));
        LoadAvailableLanguages();

        bool useSystemLanguagePerDefault = settings.useSystemLanguagePerDefault;
        LanguageCode useLang = LocalizationSettings.GetLanguageEnum(settings.defaultLangCode);    //ISO 639-1 (two characters). See: http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes

	//See if we can use the last used language (playerprefs)
	string lastLang = PlayerPrefs.GetString("M2H_lastLanguage", "");
	
	if (lastLang != "" && availableLanguages.Contains(lastLang))
	{
		SwitchLanguage(lastLang);
	}
	else
	{
		//See if we can use the local system language: if so, we overwrite useLang
		if (useSystemLanguagePerDefault)
	        {
	            //Attempt 1. Use Unity system lang
	            LanguageCode localLang = LanguageNameToCode(Application.systemLanguage);	            
	            if (localLang == LanguageCode.N)
	            {
			//Attempt 2. Otherwise try .NET cultureinfo; doesnt work on mobile systems
			// Also returns EN (EN-US) on my dutch pc (interface is english but Country&region is Netherlands)
			//BUGGED IN MONO? See: http://forum.unity3d.com/threads/5452-Getting-user-s-language-preference
			string langISO = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
	                if(langISO!="iv") //IV = InvariantCulture
                        localLang = LocalizationSettings.GetLanguageEnum(langISO);
                }
                if (availableLanguages.Contains(localLang + ""))
                {
                    useLang = localLang;
                }
                else
                {
                    //We dont have the local lang..try a few common exceptions
                    if (localLang == LanguageCode.PT)
                    { //  we did't have PT, can we show PT_BR instead?
                        if (availableLanguages.Contains(LanguageCode.PT_BR + ""))
                        {
                            useLang = LanguageCode.PT_BR;
                        }
                    }
                    if (localLang == LanguageCode.ZH)
                    { //  we did't have CN, can we show ZH_CN instead?
                        if (availableLanguages.Contains(LanguageCode.ZH_CN + ""))
                        {
                            useLang = LanguageCode.ZH_CN;
                        }else if (availableLanguages.Contains(LanguageCode.ZH_TW + ""))
                        {
                            useLang = LanguageCode.ZH_TW;
                        }
                    }
                    else if (localLang == LanguageCode.EN)
                    { // Same idea as above..
                        if (availableLanguages.Contains(LanguageCode.EN_GB + ""))
                        {
                            useLang = LanguageCode.EN_GB;
                        }
                    }
                    else if (localLang == LanguageCode.EN)
                    { // Same idea as above..
                        if (availableLanguages.Contains(LanguageCode.EN_US + ""))
                        {
                            useLang = LanguageCode.EN_US;
                        }
			}
	  	    }
	        }        
		SwitchLanguage(useLang);
	}
    }



    static void LoadAvailableLanguages()
	{
		availableLanguages = new List<string>();
		if (settings == null || settings.sheetTitles == null || settings.sheetTitles.Length <= 0)
		{
			global::Logger.Log("None available");
			return;
			
		}
		foreach (LanguageCode item in Enum.GetValues(typeof(LanguageCode)))
		{
			if (HasLanguageFile(item + "", settings.sheetTitles[0]) || HasLanguageFile(item + "", settings.sheetTitles[settings.sheetTitles.Length-1]))
			{
                //global::Logger.Log("Have "+item);
				availableLanguages.Add(item + "");
			}
		}
		//Removed this as you don't want this to unload assetbundles accidently.. --> Resources.UnloadUnusedAssets();//Clear all loaded language files
	}
	
	
	
	public static string[] GetLanguages()
	{
		return availableLanguages.ToArray();
	}
	
	public static bool SwitchLanguage(string langCode)
	{
		return SwitchLanguage(LocalizationSettings.GetLanguageEnum(langCode));
	}
	public static bool SwitchLanguage(LanguageCode code)
	{
		if (availableLanguages.Contains(code + ""))
		{
			DoSwitch(code);
			return true;
		}
		else
		{
			//global::Logger.LogError("Could not switch from language " + currentLanguage + " to " + code);
			if (currentLanguage == LanguageCode.N)
			{
				if (availableLanguages.Count > 0)
				{
					DoSwitch(LocalizationSettings.GetLanguageEnum(availableLanguages[0]));
					//global::Logger.LogError("Switched to " + currentLanguage + " instead");
				}
				else
				{
					//global::Logger.LogError("Please verify that you have the file: Resources/Languages/" + code + "");
					global::Logger.Break();
				}
			}
			
			return false;
		}
		
	}
	
	
	static void DoSwitch(LanguageCode newLang)
	{
		PlayerPrefs.SetString("M2H_lastLanguage", newLang + "");
		
		currentLanguage = newLang;
		currentEntrySheets = new Dictionary<string, Dictionary<string, string>>();
		
		
		foreach (string sheetTitle in settings.sheetTitles)
		{
			currentEntrySheets[sheetTitle] = new Dictionary<string, string>();
			
			string sheetContent = GetLanguageFileContents(sheetTitle);
			if(sheetContent != "")
			{
				using (XmlReader reader = XmlReader.Create(new StringReader(sheetContent)))
				{
					while(reader.ReadToFollowing("entry")){	
						reader.MoveToFirstAttribute();
						string tag = reader.Value;	
						reader.MoveToElement();
                        string data = reader.ReadElementContentAsString(); //get error here? Probably malformed XML in your (arabic?) language files. If arabic is troublesome, use ARABICFIXER in LanguageEditor(line +/-440) and re-download your languages.
                        data = data.Trim();		               
						data = data.Replace("\\n", "\n");
						data = data.UnescapeXML();
						(currentEntrySheets[sheetTitle])[tag] = data;
					}
				}
			}
		}
		
		//Update all localized assets
		LocalizedAsset[] assets = (LocalizedAsset[])GameObject.FindObjectsOfType(typeof(LocalizedAsset));
		foreach (LocalizedAsset asset in assets)
		{
			asset.LocalizeAsset();
		}
		
		SendMonoMessage("ChangedLanguage", currentLanguage);
	}
	
	//Get a localized asset for the current language
	static public UnityEngine.Object GetAsset(string name)
	{
		return Resources.Load("Languages/Assets/" + CurrentLanguage() + "/" + name);
	}
	
	//Lang files
	static bool HasLanguageFile(string lang, string sheetTitle)
	{
		return ((TextAsset)Resources.Load("Languages/" + lang + "_" + sheetTitle, typeof(TextAsset)) != null);
	}
	
	static string GetLanguageFileContents(string sheetTitle)
	{
		TextAsset ta = (TextAsset)Resources.Load("Languages/" + currentLanguage + "_" + sheetTitle, typeof(TextAsset));
		return ta != null ? ta.text : "";
	}
	
	
	public static LanguageCode CurrentLanguage()
	{
		return currentLanguage;
	}

    private static string[] keyNamesCache;
    private static string[] sheetNamesCache;

    private static void InitEnumToString()
    {
        if (keyNamesCache == null)
        {
            keyNamesCache = System.Enum.GetNames(typeof(LocalizationKeys));
            sheetNamesCache = System.Enum.GetNames(typeof(LocalizationSheets));
        }
    }

    public static string ToStringCached(this LocalizationKeys property)
    {
        InitEnumToString();
        //the first property starts at 0, our array starts at 0.
        return keyNamesCache[(int)property];
    }

    public static string ToStringCached(this LocalizationSheets property)
    {
        InitEnumToString();
        return sheetNamesCache[(int)property];
    }

    public static string Get(string key)
	{
		return Get(key, settings.sheetTitles[0]);
	}

    public static string Get(LocalizationKeys key)
    {
        return Get(key.ToStringCached(), settings.sheetTitles[0]);
    }

    public static string Get(LocalizationKeys key, LocalizationSheets sheetTitle)
    {
        return Get(key.ToStringCached(), sheetTitle.ToStringCached());
    }

    public static string Get(string key, LocalizationSheets sheetTitle)
    {
        return Get(key, sheetTitle.ToStringCached());
    }

    public static string Get(LocalizationKeys key, string sheetTitle)
    {
        return Get(key.ToStringCached(), sheetTitle);
    }

    public static string Get(string key, string sheetTitle)
	{
		Dictionary<string, string> dict;
		if (currentEntrySheets == null || !currentEntrySheets.TryGetValue(sheetTitle, out dict))
		{
			//global::Logger.LogError("The sheet with title \""+sheetTitle+"\" does not exist!");
			return "";
		}

		string str;
		if (dict.TryGetValue(key, out str))
		{
			return str;
		}
		else
		{
			////global::Logger.LogError("MISSING LANG:" + key);
			return "#!#"+ key+"#!#";
		}
	}
	
    public static Dictionary<string, string> GetSheet(string sheetTitle)
    {
        return currentEntrySheets[sheetTitle];
    }

	public static bool Has(string key)
	{
		return Has(key, settings.sheetTitles[0]);
	}

    public static bool Has(LocalizationKeys key)
    {
        return Has(key.ToStringCached(), settings.sheetTitles[0]);
    }

    public static bool Has(LocalizationKeys key, LocalizationSheets sheet)
    {
        return Has(key.ToStringCached(), sheet.ToStringCached());
    }

    public static bool Has(string key, LocalizationSheets sheet)
    {
        return Has(key, sheet.ToStringCached());
    }

    public static bool Has(string key, string sheetTitle)
	{
		if (currentEntrySheets == null || !currentEntrySheets.ContainsKey(sheetTitle)) return false;
		return currentEntrySheets[sheetTitle].ContainsKey(key);
    }
	

	/// <summary>
	/// Used to send ChangedLanguage
	/// </summary>
	/// <param name="methodString">Method string.</param>
	/// <param name="parameters">Parameters.</param>
    static void SendMonoMessage(string methodString, params object[] parameters)
    {
        if (!Application.isPlaying)
            return;

		if (parameters != null && parameters.Length > 1) global::Logger.LogError("We cannot pass more than one argument currently!");
		GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject go in gos)
		{
			if (go && go.transform.parent == null)
			{
				if (parameters != null && parameters.Length == 1)
				{
					go.gameObject.BroadcastMessage(methodString, parameters[0], SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					go.gameObject.BroadcastMessage(methodString, SendMessageOptions.DontRequireReceiver);
				}
            }
        }
    }

	/// <summary>
	/// Languagesname to code. Note that this will not take variations such as EN_GB and PT_BR into account
	/// </summary>
	/// <returns>The name to code.</returns>
	/// <param name="name">Name.</param>
    public static LanguageCode LanguageNameToCode(SystemLanguage name)
    {
        if (name == SystemLanguage.Afrikaans) return LanguageCode.AF;
        else if (name == SystemLanguage.Arabic) return LanguageCode.AR;
		else if (name == SystemLanguage.Basque) return LanguageCode.BA;
		else if (name == SystemLanguage.Belarusian) return LanguageCode.BE;
		else if (name == SystemLanguage.Bulgarian) return LanguageCode.BG;
		else if (name == SystemLanguage.Catalan) return LanguageCode.CA;
		else if (name == SystemLanguage.Chinese) return LanguageCode.ZH;
		else if (name == SystemLanguage.Czech) return LanguageCode.CS;
		else if (name == SystemLanguage.Danish) return LanguageCode.DA;
		else if (name == SystemLanguage.Dutch) return LanguageCode.NL;
		else if (name == SystemLanguage.English) return LanguageCode.EN;
		else if (name == SystemLanguage.Estonian) return LanguageCode.ET;
		else if (name == SystemLanguage.Faroese) return LanguageCode.FA;
		else if (name == SystemLanguage.Finnish) return LanguageCode.FI;
		else if (name == SystemLanguage.French) return LanguageCode.FR;
		else if (name == SystemLanguage.German) return LanguageCode.DE;
		else if (name == SystemLanguage.Greek) return LanguageCode.EL;
		else if (name == SystemLanguage.Hebrew) return LanguageCode.HE;
		else if (name == SystemLanguage.Hungarian) return LanguageCode.HU;
		else if (name == SystemLanguage.Icelandic) return LanguageCode.IS;
		else if (name == SystemLanguage.Indonesian) return LanguageCode.ID;
		else if (name == SystemLanguage.Italian) return LanguageCode.IT;
		else if (name == SystemLanguage.Japanese) return LanguageCode.JA;
		else if (name == SystemLanguage.Korean) return LanguageCode.KO;
		else if (name == SystemLanguage.Latvian) return LanguageCode.LA;
		else if (name == SystemLanguage.Lithuanian) return LanguageCode.LT;
		else if (name == SystemLanguage.Norwegian) return LanguageCode.NO;
		else if (name == SystemLanguage.Polish) return LanguageCode.PL;
		else if (name == SystemLanguage.Portuguese) return LanguageCode.PT;
		else if (name == SystemLanguage.Romanian) return LanguageCode.RO;
		else if (name == SystemLanguage.Russian) return LanguageCode.RU;
		else if (name == SystemLanguage.SerboCroatian) return LanguageCode.SH;
		else if (name == SystemLanguage.Slovak) return LanguageCode.SK;
		else if (name == SystemLanguage.Slovenian) return LanguageCode.SL;
		else if (name == SystemLanguage.Spanish) return LanguageCode.ES;
		else if (name == SystemLanguage.Swedish) return LanguageCode.SW;
		else if (name == SystemLanguage.Thai) return LanguageCode.TH;
		else if (name == SystemLanguage.Turkish) return LanguageCode.TR;
		else if (name == SystemLanguage.Ukrainian) return LanguageCode.UK;
		else if (name == SystemLanguage.Vietnamese) return LanguageCode.VI;
		else if (name == SystemLanguage.Hungarian) return LanguageCode.HU;
		else if (name == SystemLanguage.Unknown) return LanguageCode.N;
		return LanguageCode.N;
	}
	
}

#region enums


/// <summary>
/// Language code. Those marked with a * are not auto-detected 
/// </summary>
public enum LanguageCode
{
    N,//null
    AA, //Afar
    AB, //Abkhazian
    AF, //(Zuid) Afrikaans
    AM, //Amharic
    AR, //Arabic
	AR_SA, //* Arabic (Saudi Arabia)
	AR_EG, //* Arabic (Egypt)
	AR_DZ, //* Arabic (Algeria)
	AR_YE, //* Arabic (Yemen)
	AR_JO, //* Arabic (Jordan)
	AR_KW, //* Arabic (Kuwait)
	AR_BH, //* Arabic (Bahrain)
	AR_IQ, //* Arabic (Iraq)
	AR_MA, //* Arabic (Libya) 
	AR_LY, //* Arabic (Morocco)
	AR_OM, //* Arabic (Oman)
	AR_SY, //* Arabic (Syria)
	AR_LB, //* Arabic (Lebanon)
	AR_AE, //* Arabic (U.A.E.)
	AR_QA, //* Arabic (Qatar)
    AS, //Assamese
    AY, //Aymara
    AZ, //Azerbaijani
    BA, //Bashkir
    BE, //Byelorussian
    BG, //Bulgarian
    BH, //Bihari
    BI, //Bislama
    BN, //Bengali
    BO, //Tibetan
    BR, //Breton
    CA, //Catalan
    CO, //Corsican
    CS, //Czech
    CY, //Welch
    DA, //Danish
    DE, //German
	DE_AT, //* German (Austria)
	DE_LI, //* German (Liechtenstein)
	DE_CH, //* German (Switzerland)
	DE_LU, //* German (Luxembourg)
    DZ, //Bhutani
    EL, //Greek
    EN, //English
	EN_US, //* English (United States)
	EN_AU, //* English (Australia)
	EN_NZ, //* English (New Zealand)
	EN_ZA, //* English (South Africa)
	EN_CB, //* English (Caribbean)
	EN_TT, //* English (Trinidad)
	EN_GB, //* English (United Kingdom)
	EN_CA, //* English (Canada)
	EN_IE, //* English (Ireland)
	EN_JM, //* English (Jamaica)
	EN_BZ, //* English (Belize)
    EO, //Esperanto
    ES, //Spanish (Spain)
	ES_MX, //* Spanish (Mexico)
	ES_CR, //* Spanish (Costa Rica)
	ES_DO, //* Spanish (Dominican Republic)
	ES_CO, //* Spanish (Colombia)
	ES_AR, //* Spanish (Argentina)	
	ES_CL, //* Spanish (Chile)	
	ES_PY, //* Spanish (Paraguay)	
	ES_SV, //* Spanish (El Salvador)	
	ES_NI, //* Spanish (Nicaragua)	
	ES_GT, //* Spanish (Guatemala)	
	ES_PA, //* Spanish (Panama)	
	ES_VE, //* Spanish (Venezuela)	
	ES_PE, //* Spanish (Peru)
	ES_EC, //* Spanish (Ecuador)
	ES_UY, //* Spanish (Uruguay)
	ES_BO, //* Spanish (Bolivia)
	ES_HN, //* Spanish (Honduras)
	ES_PR, //* Spanish (Puerto Rico)
    ET, //Estonian
    EU, //Basque
    FA, //Persian
    FI, //Finnish
    FJ, //Fiji
    FO, //Faeroese
    FR, //French (Standard)
	FR_BE, //* French (Belgium)
	FR_CH, //* French (Switzerland)
	FR_CA, //* French (Canada)
	FR_LU, //* French (Luxembourg)
    FY, //Frisian
    GA, //Irish
    GD, //Scots Gaelic
    GL, //Galician
    GN, //Guarani
    GU, //Gujarati
    HA, //Hausa
    HI, //Hindi
    HE, //Hebrew
    HR, //Croatian
    HU, //Hungarian
    HY, //Armenian
    IA, //Interlingua
    ID, //Indonesian
    IE, //Interlingue
    IK, //Inupiak
    IN, //former Indonesian
    IS, //Icelandic
    IT, //Italian
	IT_CH, //* Italian (Switzerland)
    IU, //Inuktitut (Eskimo)
	IW, //DEPRECATED: former Hebrew
    JA, //Japanese
    	JI, //DEPRECATED: former Yiddish
    JW, //Javanese
    KA, //Georgian
    KK, //Kazakh
    KL, //Greenlandic
    KM, //Cambodian
    KN, //Kannada
    KO, //Korean
    KS, //Kashmiri
    KU, //Kurdish
    KY, //Kirghiz
    LA, //Latin
    LN, //Lingala
    LO, //Laothian
    LT, //Lithuanian
    LV, //Latvian, Lettish
    MG, //Malagasy
    MI, //Maori
    MK, //Macedonian
    ML, //Malayalam
    MN, //Mongolian
    MO, //Moldavian
    MR, //Marathi
    MS, //Malay
    MT, //Maltese
    MY, //Burmese
    NA, //Nauru
    NE, //Nepali
    NL, //Dutch (Standard)
	NL_BE, //*  Dutch (Belgium)
    NO, //Norwegian
    OC, //Occitan
    OM, //(Afan) Oromo
    OR, //Oriya
    PA, //Punjabi
    PL, //Polish
    PS, //Pashto, Pushto
    PT, //Portuguese
	PT_BR, //* BRazilian PT - Only used if you manually set it
    QU, //Quechua
    RM, //Rhaeto-Romance
    RN, //Kirundi
    RO, //Romanian
	RO_MO, //* Romanian (Republic of Moldova)
    RU, //Russian
	RU_MO, //* Russian (Republic of Moldova)
    RW, //Kinyarwanda
    SA, //Sanskrit
    SD, //Sindhi
    SG, //Sangro
    SH, //Serbo-Croatian
    SI, //Singhalese
    SK, //Slovak
    SL, //Slovenian
    SM, //Samoan
    SN, //Shona
    SO, //Somali
    SQ, //Albanian
    SR, //Serbian
    SS, //Siswati
    ST, //Sesotho
    SU, //Sudanese
    SV, //Swedish
	SV_FI, //* Swedish (finland)
    SW, //Swahili
    TA, //Tamil
    TE, //Tegulu
    TG, //Tajik
    TH, //Thai
    TI, //Tigrinya
    TK, //Turkmen
    TL, //Tagalog
    TN, //Setswana
    TO, //Tonga
    TR, //Turkish
    TS, //Tsonga
    TT, //Tatar
    TW, //Twi
    UG, //Uigur
    UK, //Ukrainian
    UR, //Urdu
    UZ, //Uzbek
    VI, //Vietnamese
    VO, //Volapuk
    WO, //Wolof
    XH, //Xhosa
    YI, //Yiddish
    YO, //Yoruba
    ZA, //Zhuang
    ZH, //Chinese Simplified
	ZH_TW, //* Chinese Traditional / Chinese (Taiwan)
	ZH_HK, //* Chinese (Hong Kong SAR)
	ZH_CN, //* Chinese (PRC)
	ZH_SG, //* Chinese (Singapore)
    ZU  //Zulu
}


public static class StringExtensions
{
	
	
	public static string UnescapeXML(this string s)
	{
		if (string.IsNullOrEmpty(s)) return s;
		
		string returnString = s;
		returnString = returnString.Replace("&apos;", "'");
		returnString = returnString.Replace("&quot;", "\"");
		returnString = returnString.Replace("&gt;", ">");
		returnString = returnString.Replace("&lt;", "<");
		returnString = returnString.Replace("&amp;", "&");
		
		return returnString;
	}
}
#endregion