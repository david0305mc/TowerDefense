using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeManager : Singleton<LocalizeManager>
{

    //private static bool _isLoaded;
    //private static string filePath => ZString.Format("{0}/{1}/{2}", PathInfo.DataPath, ServerSetting.serverName, "Localization.csv");

    //public static string language
    //{
    //    get => Localization.language;
    //    set
    //    {
    //        Debug.LogFormat("[Localization/SetLanguage] {0}", value);
    //        Localization.language = value;
    //    }
    //}

    public static void Initialize()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Language")))
            return;

        string language = "en";
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                language = "ko";
                break;
            case SystemLanguage.English:
                language = "en";
                break;
            //case SystemLanguage.Japanese:
            //    language = "ja";
            //    break;
            //case SystemLanguage.Chinese:
            //case SystemLanguage.ChineseSimplified: //간체(중국)
            //    language = "zh-Hans";
            //    break;
            //case SystemLanguage.ChineseTraditional: //번체(대만)
            //    language = "zh-Hant";
            //    break;
            //case SystemLanguage.Portuguese:
            //    language = "pt-BR";
            //    break;
            //case SystemLanguage.Spanish:
            //    language = "es";
            //    break;
            //case SystemLanguage.Russian:
            //    language = "ru";
            //    break;
            case SystemLanguage.Indonesian:
                language = "id";
                break;
                //case SystemLanguage.German:
                //    language = "de";
                //    break;
                //case SystemLanguage.French:
                //    language = "fr";
                //    break;
        }

        PlayerPrefs.SetString("Language", language);
    }


//    public static async UniTask LoadDataAsync(CancellationToken cancellationToken = default, bool force = false)
//    {
//        if (!force && _isLoaded)
//            return;

//#if DISABLE_TABLE_CRYPTO
//        SetData(await File.ReadAllBytesAsync(filePath, cancellationToken).AsUniTask(), '|', force);
//#else
//        SetData(await Encryptor.Default.ReadAllBytesAsyncFromFile(PathInfo.GetFileNamePath("Localization.csv")), '|', force);
//#endif
//    }

//    public static void SetData(byte[] bytes, char separator = ',', bool force = false)
//    {
//        if (!force && _isLoaded)
//            return;

//        Localization.LoadCSV(bytes, true, separator);
//        _isLoaded = true;
//        Debug.Log("[Localization/SetData] OnLoaded");
//    }

    public static string GetLanguageISO()
    {
        //string lang = Localization.language;
        //if (lang == "ko") return "KO";
        //else if (lang == "en") return "EN";
        //else if (lang == "ja") return "JP";
        //else if (lang == "zh-Hans") return "CN";
        //else if (lang == "zh-Hant") return "TW";
        //else if (lang == "pt-BR") return "PT";
        //else if (lang == "es") return "ES";
        //else if (lang == "ru") return "RU";
        //else if (lang == "id") return "ID";
        //else if (lang == "de") return "DE";
        //else if (lang == "fr") return "FR";
        return "EN";
    }
}
