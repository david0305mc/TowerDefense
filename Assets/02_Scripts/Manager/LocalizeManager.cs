using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeManager : Singleton<LocalizeManager>
{
    private LANGUAGE_TYPE language;
    public LANGUAGE_TYPE Language
    {
        get => language;
        set
        {
            Debug.LogWarningFormat("[Localization/SetLanguage] {0}", value);
            language = value;
            PlayerPrefs.SetString("Language", language.ToString());
            // broad Casting
            UIMain.Instance.Broadcast("OnLocalize");
        }
    }

    public string GetLocalString(string _key)
    {
        var localInfo = DataManager.Instance.GetLocalizationData(_key);

        if (localInfo == null)
        {
            return _key;
        }

        switch (language)
        {
            case LANGUAGE_TYPE.KO:
                return localInfo.ko;
            case LANGUAGE_TYPE.EN:
                return localInfo.en;
            case LANGUAGE_TYPE.JP:
                return localInfo.jp;
            default:
                return localInfo.en;
        }
    }
         

    public void Initialize()
    {
        string lang = PlayerPrefs.GetString("Language");
        if (!string.IsNullOrEmpty(lang))
        {
            language = Utill.StirngToEnum<LANGUAGE_TYPE>(lang); 
            return;
        }
        
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                language = LANGUAGE_TYPE.KO;
                break;
            case SystemLanguage.English:
                language = LANGUAGE_TYPE.EN;
                break;
            case SystemLanguage.Japanese:
                language = LANGUAGE_TYPE.JP;
                break;
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified: //간체(중국)
                language = LANGUAGE_TYPE.CN;
                break;
            case SystemLanguage.ChineseTraditional: //번체(대만)
                language = LANGUAGE_TYPE.TW;
                break;
            //case SystemLanguage.Portuguese:
            //    language = "pt-BR";
            //    break;
            //case SystemLanguage.Spanish:
            //    language = "es";
            //    break;
            case SystemLanguage.Russian:
                language = LANGUAGE_TYPE.RU;
                break;
            case SystemLanguage.German:
                language = LANGUAGE_TYPE.DE;
                break;
            case SystemLanguage.French:
                language = LANGUAGE_TYPE.FR;
                break;
        }
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
}

public static class LocalStringExtension
{
    public static string GetEnumLocalization<T>(this T _enum)
    {
        string titleStrKey = $"{_enum.GetType().Name.ToLower()}_{_enum.ToString().ToLower()}";
        return LocalizeManager.Instance.GetLocalString(titleStrKey);
    }
}