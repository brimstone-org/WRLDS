//Mike Hergaarden - M2H.nl
using UnityEngine;
using UnityEditor;

using System.IO;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

using Match = System.Text.RegularExpressions.Match;
using System.Text;

public class LanguageEditor : EditorWindow
{
    //Settings
    //private string urlRequestProxy = "http://www.M2H.nl/unity/editorPageProxy.php";

    private static LocalizationSettings m_settings = null;
    public static LocalizationSettings settings
    {
        get
        {
            if (m_settings == null)
            {
                if (File.Exists(Language.settingsAssetPath))
                    m_settings = (LocalizationSettings)AssetDatabase.LoadAssetAtPath(Language.settingsAssetPath, typeof(LocalizationSettings));
                else
                {
                    m_settings = (LocalizationSettings)ScriptableObject.CreateInstance(typeof(LocalizationSettings));
                    string settingsPath = Path.GetDirectoryName(Language.settingsAssetPath);
                    if (!Directory.Exists(settingsPath))
                    {
                        Directory.CreateDirectory(settingsPath);
                        AssetDatabase.ImportAsset(settingsPath);
                    }
                    AssetDatabase.CreateAsset(settings, Language.settingsAssetPath);
                }
            }
            return m_settings;
        }
    }

    [MenuItem("Tools/Localization")]
    static void OpenWindow()
    {
        EditorWindow.GetWindow(typeof(LanguageEditor));
    }

    string gDocsURL
    {
        get
        {
            return settings.localizationURL;
        }
        set
        {
            settings.localizationURL = value;
            EditorUtility.SetDirty(settings);
        }
    }

    int unresolvedErrors = 0;
    int foundSheets = 0;
    bool useSystemLang = true;
    LanguageCode langCode = LanguageCode.EN;
    bool loadedSettings = false;

    void LoadSettings()
    {
        if (loadedSettings || settings == null)
            return;
        loadedSettings = true;

        useSystemLang = settings.useSystemLanguagePerDefault;
        langCode = LocalizationSettings.GetLanguageEnum(settings.defaultLangCode);
    }

    Vector2 scrollView;

    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            GUILayout.Label("Editor is in play mode.");
            return;
        }
        LoadSettings();

        scrollView = GUILayout.BeginScrollView(scrollView);

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        useSystemLang = EditorGUILayout.Toggle("Try system language", useSystemLang);
        langCode = (LanguageCode)EditorGUILayout.EnumPopup("Default language", langCode);
        gDocsURL = EditorGUILayout.TextField("gDocs Link", gDocsURL);
        if (GUI.changed)
        {
            SaveSettingsFile();
        }
        if (GUILayout.Button("Update translations"))
        {
            gDocsURL = gDocsURL.Trim();
            if (gDocsURL.Contains("&output"))
            {
                //OLD FORMAT
                if (!gDocsURL.Contains("&single="))
                    gDocsURL += "&single=true";
                if (!gDocsURL.Contains("&gid="))
                    gDocsURL += "&gid=0";
                if (!gDocsURL.Contains("&output=csv"))
                    gDocsURL += "&output=csv";
                if (gDocsURL.Contains("&output=html"))
                    gDocsURL = gDocsURL.Replace("&output=html", "&output=csv");
            }
            if (!gDocsURL.Contains(".google.com"))
            {
                EditorUtility.DisplayDialog("Error", "You have entered an incorrect spreadsheet URL. Please read the manuals instructions (See readme.txt)", "OK");
            }
            else
            {
                EditorPrefs.SetString(PlayerSettings.productName + "gDocs", gDocsURL);
                LoadSettings(gDocsURL);
            }
        }
        if (unresolvedErrors > 0)
        {
            Rect rec = GUILayoutUtility.GetLastRect();
            GUI.color = Color.red;
            EditorGUI.DropShadowLabel(new Rect(0, rec.yMin + 15, 200, 20), "Unresolved errors: " + unresolvedErrors);
            GUI.color = Color.white;
        }


        GUILayout.Space(25);
        GUILayout.Label("For full instructions read the localization package documentation.", EditorStyles.miniLabel);
        if (GUILayout.Button("Open documentation"))
        {
            Application.OpenURL("http://www.M2H.nl/files/LocalizationPackage.pdf");
        }
        if (GUILayout.Button("Verify localized assets"))
        {
            VerifyLocAssets();
        }
        if (GUILayout.Button("More Unity resources"))
        {
            Application.OpenURL("http://www.M2H.nl/unity/");
        }

        GUILayout.EndScrollView();
    }

    static string PregReplace(string input, string[] pattern, string[] replacements)
    {
        if (replacements.Length != pattern.Length)
            throw new ArgumentException("Replacement and Pattern Arrays must be balanced");

        for (var i = 0; i < pattern.Length; i++)
        {
            input = Regex.Replace(input, pattern[i], replacements[i]);
        }

        return input;
    }

    void VerifyLocAssets()
    {
        string langRootFolder = GetLanguageFolder() + Path.DirectorySeparatorChar + "Assets" + +Path.DirectorySeparatorChar;
        string[] files = Directory.GetFiles(langRootFolder, "*", SearchOption.AllDirectories);

        List<string> langList = new List<string>();
        List<string> uniqueAssets = new List<string>();
        foreach (string file in files)
        {
            if (file.Length > 5 && file.Substring(file.Length - 5, 5) == ".meta")
                continue;//Ignore meta files

            string file2 = file.Substring(langRootFolder.Length);

            string lang = file2.Substring(0, 2);
            if (!langList.Contains(lang))
                langList.Add(lang);
            string uniqueAsset = file2.Substring(3);
            if (!uniqueAssets.Contains(uniqueAsset))
            {
                uniqueAssets.Add(uniqueAsset);
            }
        }

        int missing = 0;
        //Test assets
        foreach (string lang in langList)
        {
            foreach (string asset in uniqueAssets)
            {
                if (!File.Exists(langRootFolder + lang + "/" + asset))
                {
                    missing++;
                    Debug.LogError("[" + lang + "] MISSING " + langRootFolder + lang + "/" + asset);
                }
            }
        }

        if (missing > 0)
        {
            EditorUtility.DisplayDialog("Verifying assets", "Missing asset translations: " + missing + ". See console for details.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Verifying assets", "All seems to be OK!", "OK");
        }
    }

    void LoadSettings(string gDocsPage)
    {
        SaveSettingsFile();


        gDocsPage += ((gDocsPage.Contains("?")) ? "&" : "?") + "timestamp=" + EditorApplication.timeSinceStartup;//Prevent caching

        float progress = 0.1f;

        List<SheetInfo> sheetIDs = GetSpreadSheetIDs(gDocsPage);
        settings.sheetTitles = new string[0];
        settings.usedKeys = new string[0];
        settings.usedLanguages = new string[0];
        unresolvedErrors = 0;
        foundSheets = 0;


        for (int i = sheetIDs.Count - 1; i >= 0; i--)
        {
            int downloadSheet = sheetIDs[i].ID;
            if (sheetIDs[i].title.Length > 3 && sheetIDs[i].title.Substring(0, 3) == "___") continue; //Skip sheets starting with "___"

            string thisURL = gDocsPage;
            bool oldFormat = false;
            if (gDocsPage.Contains("&output="))
            {
                //Old format
                oldFormat = true;

                thisURL = gDocsPage.Replace("gid=0", "gid=" + (downloadSheet)) + "&" + EditorApplication.timeSinceStartup;
            }
            //Debug.Log(thisURL);

            EditorUtility.DisplayProgressBar("Downloading gDoc data", "Page: " + downloadSheet, progress);
            string data = GetWebpage(thisURL);
            //Debug.Log("WWWDATA from Gdocs:[" + thisURL + "] ");
            if (data != "")
            {
                if (oldFormat)
                {
                    data = CleanData(data);
                    if (data.StartsWith("<html>") || data.StartsWith("<!DOCTYPE html>"))
                    {
                        //Sheet does not exist
                        Debug.LogError("Sheet #" + downloadSheet + " does not exist!");
                        continue; //If more than X in a row do now exist, it's probably the end
                    }
                }
                if (!ParseData(data, sheetIDs[i].title, i))
                {
                    //Failed
                }
                else
                {
                    //Success
                    CreateGeneratedCode();
                }
            }
        }
        if (foundSheets == 0)
        {
            EditorUtility.DisplayDialog("Error", "No sheets could be imported. Either they were all empty, or you entered a wrong link. Please copy your LINK again and verify your spreadsheet.", "OK");
        }


        EditorUtility.ClearProgressBar();
        if (unresolvedErrors > 0)
        {
            EditorUtility.DisplayDialog("Errors", "There are " + unresolvedErrors + " open errors in your localization. See the console for more information.", "OK");
        }
    }

    void LoadCSV(Hashtable loadLanguages, Hashtable loadEntries, string data, string debugSheetTitle)
    {
        List<string> sheetNames = new List<string>(settings.sheetTitles);
        if (!sheetNames.Contains(debugSheetTitle))
            sheetNames.Add(debugSheetTitle);
        settings.sheetTitles = sheetNames.ToArray();
        SaveSettingsFile();

        List<string> lines = GetCVSLines(data);

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            List<string> contents = GetCVSLine(line);
            if (i == 0)
            {
                //Language titles
                for (int j = 1; j < contents.Count; j++)
                {
                    loadLanguages[j] = contents[j];
                    loadEntries[j] = new Hashtable();
                }
            }
            else if (contents.Count > 1)
            {
                string key = contents[0];
                if (key == "")
                    continue; //Skip entries with empty keys (the other values can be used as labels)
                for (int j = 1; j < (loadLanguages.Count + 1) && j < contents.Count; j++)
                {

                    string content = contents[j];
                    Hashtable hTable = (Hashtable)loadEntries[j];
                    if (hTable.ContainsKey(key))
                    {
                        Debug.LogError("ERROR: Double key [" + key + "] Sheet: " + debugSheetTitle);
                        unresolvedErrors++;
                    }
                    hTable[key] = System.Security.SecurityElement.Escape(content);
                }
            }
            else
            {
                //Debug.Log(line+" was empty");
            }
        }
    }

    void LoadHTML(Hashtable loadLanguages, Hashtable loadEntries, string data, string sheetTitle, int sheetID)
    {
        //all languages on 1 page.

        var document = new HtmlAgilityPack.HtmlDocument();
        document.LoadHtml(data);

        int tableNR = 0;
        foreach (HtmlAgilityPack.HtmlNode node in document.DocumentNode.SelectNodes("//table"))
        {
            if (tableNR == sheetID)
            {
                //Only parse 1 "sheet"
                ParseHTMLTable(loadLanguages, loadEntries, node);
            }
            tableNR++;
            foundSheets++;
        }

        //Parse sheet titles
        List<string> sheetNames = new List<string>(settings.sheetTitles);

        List<string> newTitles = GetSheetTitles(document);
        foreach (string newTitl in newTitles)
        {
            if (!sheetNames.Contains(newTitl))
                sheetNames.Add(newTitl);
        }
        settings.sheetTitles = sheetNames.ToArray();
        SaveSettingsFile();

    }

    void ParseHTMLTable(Hashtable loadLanguages, Hashtable loadEntries, HtmlAgilityPack.HtmlNode node)
    {
        int row = -1;
        bool firstRow = true;
        foreach (HtmlAgilityPack.HtmlNode trNode in node.SelectNodes(".//tr"))
        {

            if (trNode.SelectNodes(".//td") == null)
            {
                continue;
            }
            row++;
            int i = -1;
            string key = "";


            foreach (HtmlAgilityPack.HtmlNode tdNode in trNode.SelectNodes(".//td"))
            {
                i++;

                if (firstRow && row == 0)
                {
                    //Language header
                    if (i == 0)
                        continue; //Ignore this top-left empty cell	
                    loadLanguages[i] = tdNode.InnerTextAndNewLines();
                    if (!loadEntries.ContainsKey(i))
                        loadEntries[i] = new Hashtable();
                }
                else
                {
                    //Data rows				

                    if (i == 0)
                    {
                        key = tdNode.InnerText;
                        continue;
                    }
                    if (key == "")
                        continue; //Skip entries with empty keys (the other values can be used as labels)


                    string content = tdNode.InnerTextAndNewLines(false);
                    if (content.Length < tdNode.InnerText.Length) content = tdNode.InnerText;
                    Hashtable hTable = (Hashtable)loadEntries[i];
                    if (hTable.ContainsKey(key))
                    {
                        Debug.LogError("ERROR: Double key [" + key + "]");
                        unresolvedErrors++;
                    }
                    hTable[key] = content;
                }
            }
            firstRow = false;
        }
    }

    bool ParseData(string data, string sheetTitle, int sheetID)
    {
        string langFolder = GetLanguageFolder();

        Hashtable loadLanguages = new Hashtable();
        Hashtable loadEntries = new Hashtable();

        if (data.Contains("<html>"))
        {
            LoadHTML(loadLanguages, loadEntries, data, sheetTitle, sheetID);
        }
        else
        {
            //CSV
            LoadCSV(loadLanguages, loadEntries, data, sheetTitle);
        }


        if (loadEntries.Count < 1)
        {
            unresolvedErrors++;
            Debug.LogError("Sheet \"" + sheetTitle + "\" [" + sheetID + "] contains no languages! " + data);
            return false;
        }
        //Verify loaded data
        Hashtable sampleData = (Hashtable)loadEntries[1];
        for (int j = 2; j < loadEntries.Count; j++)
        {
            Hashtable otherData = ((Hashtable)loadEntries[j]);

            foreach (DictionaryEntry item in otherData)
            {
                if (!sampleData.ContainsKey(item.Key))
                {
                    Debug.LogError("[" + loadLanguages[1] + "] [" + item.Key + "] Key is missing!");
                    unresolvedErrors++;
                }
            }
            foreach (DictionaryEntry item in sampleData)
            {
                if (!otherData.ContainsKey(item.Key))
                {
                    Debug.LogError("Sheet(" + sheetTitle + ") [" + loadLanguages[j] + "] [" + item.Key + "] Key is missing!");
                    unresolvedErrors++;
                }
            }
        }

        List<string> langsList = new List<string>(settings.usedLanguages);
        List<string> keyList = new List<string>(settings.usedKeys);

        //Save the loaded data
        foreach (DictionaryEntry langs in loadLanguages)
        {
            string langCode = ((string)langs.Value).Replace("\n", "").Replace("\r", "");  // NewLine doesnt work on OSX: ((string)langs.Value).TrimEnd(System.Environment.NewLine.ToCharArray());
            int langID = (int)langs.Key;


            if (!langsList.Contains(langCode))
            {
                langsList.Add(langCode);
            }


            string output = "<entries>\n";
            Hashtable entries = (Hashtable)loadEntries[langID];
            foreach (DictionaryEntry item in entries)
            {
                string key = item.Key + "";
                string tvalue = item.Value + "";
                key.Replace("\"", "\\\"");
                //tvalue = (tvalue);
                if (IsArabic(tvalue))
                {
                    tvalue = ReverseString(tvalue); //USE ARABICFIXER FROM ASSETSTORE INSTEAD: tvalue = ArabicFixer.Fix(tvalue, false, false);//   ReverseString(tvalue);
                }
                output += "<entry name=\"" + key + "\">" + tvalue + "</entry>\n";

                if (!keyList.Contains(key))
                    keyList.Add(key);
            }
            output += "</entries>\n";
            foundSheets++;
            SaveFile("Assets" + Path.DirectorySeparatorChar + langFolder + Path.DirectorySeparatorChar + langCode + "_" + sheetTitle + ".xml", output);
        }

        settings.usedLanguages = langsList.ToArray();
        settings.usedKeys = keyList.ToArray();

        Debug.Log("Imported " + loadLanguages.Count + " languages");


        return true;
    }

    string CleanData(string data)
    {
        //if(data.Contains("TUTORIAL_WELCOMETEXT"))
        //    Debug.Log("@@ BEFORE="+data);

        //Cut of formula data
        int formulaIndex = data.IndexOf("\n\n\n[");
        if (formulaIndex != -1)
            data = data.Substring(0, formulaIndex);

        string[] patterns = new string[4];
        string[] replacements = new string[4];
        int patrs = 0;
        int reps = 0;

        patterns[patrs++] = @" \[[0-9]+\],";
        replacements[reps++] = ",";

        patterns[patrs++] = @" \[[0-9]+\]""";
        replacements[reps++] = "\"";

        patterns[patrs++] = @" \[[0-9]+\]([\n\r$]+)";
        replacements[reps++] = "$1";
        patterns[patrs++] = @" \[[0-9]+\]\Z";
        replacements[reps++] = "";

        data = PregReplace(data, patterns, replacements);

        //if (data.Contains("TUTORIAL_WELCOMETEXT"))
        //    Debug.Log("@@ AFTER=" + data);


        return data;

    }

    void SaveFile(string file, string data)
    {
        if (File.Exists(file))
            File.Delete(file);
        try
        {
            TextWriter tw = new StreamWriter(file);
            tw.Write(data);
            tw.Close();
        }
        catch (System.IO.IOException IOEx)
        {
            Debug.LogError("Incorrect file permissions? " + IOEx);
        }

        AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
        AssetDatabase.Refresh();

    }

    List<string> GetCVSLines(string data)
    {
        List<string> lines = new List<string>();
        int i = 0;
        int searchCloseTags = 0;
        int lastSentenceStart = 0;
        while (i < data.Length)
        {
            if (data[i] == '"')
            {
                if (searchCloseTags == 0)
                    searchCloseTags++;
                else
                    searchCloseTags--;
            }
            else if (data[i] == '\n')
            {
                if (searchCloseTags == 0)
                {
                    lines.Add(data.Substring(lastSentenceStart, i - lastSentenceStart));
                    lastSentenceStart = i + 1;
                }
            }
            i++;
        }
        if (i - 1 > lastSentenceStart)
        {
            lines.Add(data.Substring(lastSentenceStart, i - lastSentenceStart));
        }
        return lines;
    }

    List<string> GetCVSLine(string line)
    {
        List<string> list = new List<string>();
        int i = 0;
        int searchCloseTags = 0;
        int lastEntryBegin = 0;
        while (i < line.Length)
        {
            if (line[i] == '"')
            {
                if (searchCloseTags == 0)
                    searchCloseTags++;
                else
                    searchCloseTags--;
            }
            else if (line[i] == ',')
            {
                if (searchCloseTags == 0)
                {
                    list.Add(StripQuotes(line.Substring(lastEntryBegin, i - lastEntryBegin)));
                    lastEntryBegin = i + 1;
                }
            }
            i++;
        }
        if (line.Length > lastEntryBegin)
        {
            list.Add(StripQuotes(line.Substring(lastEntryBegin)));//Add last entry
        }
        return list;
    }

    //Remove the double " that CVS adds inside the lines, and the two outer " as well
    string StripQuotes(string input)
    {
        if (input.Length < 1 || input[0] != '"')
            return input;//Not a " formatted line

        string output = "";
        ;
        int i = 1;
        bool allowNextQuote = false;
        while (i < input.Length - 1)
        {
            string curChar = input[i] + "";
            if (curChar == "\"")
            {
                if (allowNextQuote)
                    output += curChar;
                allowNextQuote = !allowNextQuote;
            }
            else
            {
                output += curChar;
            }
            i++;
        }
        return output;
    }

    string GetWebpage(string url)
    {
        WWW wwwReq = new WWW(url);
        while (!wwwReq.isDone)
        {
        }
        //if(wwwReq.error == null) {
        return wwwReq.text;
        /*} else {
			//Unity Editor needs Crossdomain.xml when running in Editor with Webplayer target
			//If you are concerned about privacy/performance: You can also host this proxy yourself, see PDF
			Debug.LogWarning("Error grabbing gDocs data:" + wwwReq.error + " (URL=" + url + ")");
			Debug.LogWarning("Trying again via proxy. Switch to standalone target to prevent this!");
			
			WWWForm form = new WWWForm();
			form.AddField("page", url);
			WWW wwwReq2 = new WWW(urlRequestProxy, form);
			while(!wwwReq2.isDone) {
			}
			if(wwwReq2.error == null) {
				return wwwReq2.text;
			} else {
				Debug.LogError(wwwReq2.error);
			}
		}*/


    }

    string GetLanguageFolder()
    {
        string[] subdirEntries = Directory.GetDirectories(Application.dataPath, "Languages", SearchOption.AllDirectories);
        foreach (string subDir in subdirEntries)
        {
            if (subDir.Contains("Resources"))
            {
                string outp = subDir.Replace(Application.dataPath, "");
                if (outp[0] == '\\') outp = outp.Substring(1);
                return outp;
            }
        }

        //Create folder
        string folder = Application.dataPath + "/Localization";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        folder = folder + "/Resources";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.ImportAsset(folder, ImportAssetOptions.ForceUpdate);
        }
        folder = folder + "/Languages";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.ImportAsset(folder, ImportAssetOptions.ForceUpdate);
        }
        AssetDatabase.Refresh();

        return folder;
    }


    List<string> GetSheetTitles(HtmlAgilityPack.HtmlDocument document)
    {
        List<string> titles = new List<string>();

        if (document.DocumentNode.SelectNodes("//a") == null)
        {
            //Single sheet in this document
            HtmlAgilityPack.HtmlNode node = document.DocumentNode.SelectSingleNode("//span ");

            int cutOffFrom = node.InnerText.IndexOf(":") + 2;
            string title = node.InnerText.Substring(cutOffFrom);

            titles.Add(title);

        }
        else
        {
            //Parse sheet titles
            foreach (HtmlAgilityPack.HtmlNode node in document.DocumentNode.SelectNodes("//a"))
            {
                if (node.ParentNode.ChildNodes.Count != 1) //Filter out the REPORT ABUSE and BY GOOGLE links
                {
                    //Debug.Log("Ignoring: "+node.InnerText+", unlikely to be a language title");
                    continue;
                }
                titles.Add(node.InnerText);
            }
        }
        return titles;

    }


    List<SheetInfo> GetSpreadSheetIDs(string gDocsUrl)
    {
        List<SheetInfo> res = new List<SheetInfo>();

        if (!gDocsUrl.Contains("output="))
        {
            //2014 format
            string output = GetWebpage(gDocsUrl);


            var document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(output);

            List<string> titles = GetSheetTitles(document);

            int i = 0;
            foreach (string titl in titles)
            {
                SheetInfo inf = new SheetInfo();
                inf.ID = i;
                inf.title = titl;
                res.Add(inf);
                i++;
            }

        }
        else
        {
            //2013 format
            string gDocskey = "";
            System.Text.RegularExpressions.Match match = Regex.Match(gDocsUrl, "key=(?<gDocsKey>[^&#]+)");
            if (match.Captures.Count == 1)
            {
                gDocskey = match.Groups["gDocsKey"].Value;
            }
            else
            {
                int startIndex = gDocsUrl.IndexOf("/d/") + 3;
                int endIndex = gDocsUrl.Substring(startIndex).IndexOf('/');
                gDocskey = gDocsUrl.Substring(startIndex, endIndex);
                //Debug.Log("key="+gDocskey);
            }


            string URL = "https://spreadsheets.google.com/spreadsheet/pub?key=" + gDocskey;
            res = GetSheetIDs(URL, res);

            //Also fetch the ID of the default sheet.
            if (res.Count > 0)
                GetSheetIDs(URL + "&gid=" + res[0].ID, res);
        }

        if (res.Count == 0)
        {
            Debug.LogWarning("No sheets found, or your spreadsheet has only 1 sheet. We are assuming that the first sheet has ID '0'. (You can fix this by simply adding a second sheet as this will allow ID lookup via HTML output)");
            SheetInfo info = new SheetInfo();
            info.ID = 0;
            info.title = "Sheet1";
            res.Add(info);
        }
        return res;
    }

    //Parse sheet IDs from the HTML view.
    List<SheetInfo> GetSheetIDs(string URL, List<SheetInfo> sheetList)
    {
        string output = GetWebpage(URL);

        //Debug.Log("GetSheetIDs ["+URL+"] "+ output);

        MatchCollection matches = Regex.Matches(output, ";gid=(?<sheetID>[0-9]+)\">(?<sheetTitle>[^<]+)</");
        if (matches.Count < 1)
        {
            //Try new Google format: onclick="switchToSheet('0')" >Main</a>
            matches = Regex.Matches(output, "\'(?<sheetID>[0-9]+)\'.\" >(?<sheetTitle>[^<]+)</");
            if (matches.Count < 1)
            {

                //Newer google format (April 2017) id="sheet-button-449696457"><a href="#">Sheet2</a>
                matches = Regex.Matches(output, "-(?<sheetID>[0-9]+)\"><a href=\"#\">(?<sheetTitle>[^<]+)</");

            }
        }


        foreach (System.Text.RegularExpressions.Match mat in matches)
        {
            int sheetID = int.Parse(mat.Groups["sheetID"].Value);
            string sheetTitle = mat.Groups["sheetTitle"].Value;
            //Debug.Log("[" + sheetID + "] title=" + sheetTitle);
            bool present = false;
            foreach (SheetInfo info in sheetList)
            {
                if (info.ID == sheetID)
                {
                    present = true;
                    break;
                }
            }
            if (!present)
            {
                SheetInfo inf = new SheetInfo();
                inf.ID = sheetID;
                inf.title = sheetTitle;
                sheetList.Add(inf);
            }
        }

        return sheetList;
    }

    struct SheetInfo
    {
        public int ID;
        public string title;
    }

    void SaveSettingsFile()
    {
        settings.defaultLangCode = langCode.ToString();
        settings.useSystemLanguagePerDefault = useSystemLang;
        EditorUtility.SetDirty(settings);

    }

    public bool IsArabic(string strCompare)
    {
        char[] chars = strCompare.ToCharArray();
        foreach (char ch in chars)
            if (ch >= '\u0627' && ch <= '\u0649')
                return true;
        return false;
    }

    public static string ReverseString(string s)
    {
        char[] arr = s.ToCharArray();
        Array.Reverse(arr);
        string bl = new string(arr);
        for (int i = 0; i < 10; i++)
        {
            bl = bl.Replace("}" + i + "{", "{" + i + "}");
        }
        return new string(arr);
    }

    void CreateGeneratedCode()
    {
        string output = "//Auto generated code via LanguageEditor.cs - so that we have enums that are correct with our google spreadsheet \n\n";
        output += "/* public enum LocalizationLanguages { Null } \n public enum LocalizationSheets { Null } public enum LocalizationKeys { Null } */ ";

        //Languages
        output += "public enum LocalizationLanguages {";

        bool first = true;
        int validVals = 0;
        for (int i = 0; i < settings.usedLanguages.Length; i++)
        {
            string val = GenerateCheckEnumValue(settings.usedLanguages[i], validVals);
            if (!string.IsNullOrEmpty(val))
            {
                if (!first) { output += ", "; }
                first = false;
                output += val;
                if (i % 10 == 0) output += "\n";
                validVals++;
            }
        }
        output += "}\n\n";

        //Sheets
        output += "public enum LocalizationSheets {";
        first = true;
        validVals = 0;
        for (int i = 0; i < settings.sheetTitles.Length; i++)
        {
            string val = GenerateCheckEnumValue(settings.sheetTitles[i], validVals);
            if (!string.IsNullOrEmpty(val))
            {
                if (!first) { output += ", "; }
                first = false;
                output += val;
                if (i % 10 == 0) output += "\n";
                validVals++;
            }
        }
        output += "}\n\n";

        //Global-keys
        output += "public enum LocalizationKeys {";
        first = true;
        validVals = 0;
        for (int i = 0; i < settings.usedKeys.Length; i++)
        {
            string val = GenerateCheckEnumValue(settings.usedKeys[i], validVals);
            if (!string.IsNullOrEmpty(val))
            {
                if (!first) { output += ", "; }
                first = false;
                output += val;
                if (i % 10 == 0) output += "\n";
                validVals++;
            }
        }
        output += "}\n\n";


        string path = Language.generatedCodePath;
        if (!File.Exists(path) || File.ReadAllText(path) != output)
        {
            System.IO.File.WriteAllText(path, output);
        }
    }

    string GenerateCheckEnumValue(string theValue, int ID)
    {
        if (string.IsNullOrEmpty(theValue) || Regex.IsMatch(theValue, @"^\d") || theValue.Contains("=") || theValue.Contains("-") || theValue.Contains("'") || theValue.Contains(" ") || theValue.Contains("\""))
        {
            Debug.LogWarning("Couldn't generate enum value for localiaztion value: \"" + theValue + "\" (Enums may not contain ', -, =, \" space or start with a digit)");
            return "";
        }
        return theValue + " = " + ID;
    }
}


public static class HtmlHelper
{
    public static string InnerTextAndNewLines(this HtmlAgilityPack.HtmlNode node, bool log = false)
    {
        var sb = new StringBuilder();
        foreach (var x in node.ChildNodes)
        {
            if (log)
            {
                Debug.Log(x.NodeType + " en name=" + x.Name);
            }
            if (x.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
                sb.Append(x.InnerText);
            else if (x.NodeType == HtmlAgilityPack.HtmlNodeType.Element && x.Name == "br")
                sb.AppendLine();

        }

        return sb.ToString();
    }
}