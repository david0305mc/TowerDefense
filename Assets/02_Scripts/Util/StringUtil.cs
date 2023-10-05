using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public partial class Utill
{
    public static string IntConvertCommaString(int _value)
    {
        string value = string.Format("{0:n0}", _value);

        return value;
    }

    public static int CommaStringConvertInt(string _str)
    {
        string replaceStr = _str.Replace(",", "");
        int value = int.Parse(replaceStr);

        return value;
    }

    private static string mLimitedTimeStringDHMS = "{0}D {1:00}:{2:00}:{3:00}";
    private static string mLimitedTimeStringHMS = "{0:00}:{1:00}:{2:00}";
    /// <summary>
    /// 기간한정 남은시간표기
    /// </summary>
    public static string ConvertSecToLimitedTime(int _seconds)
    {
        if (_seconds > 86400)
        {   //하루넘게 남은 경우
            int day = Mathf.FloorToInt(_seconds / 86400);
            _seconds -= day * 86400;
            int hour = Mathf.FloorToInt(_seconds / 3600);
            _seconds -= hour * 3600;
            int min = Mathf.FloorToInt(_seconds / 60);
            _seconds -= min * 60;
            return string.Format(mLimitedTimeStringDHMS, day, hour, min, _seconds);
        }
        else if (_seconds > 0)
        {   //하루도 안 남은 경우
            int hour = Mathf.FloorToInt(_seconds / 3600);
            _seconds -= hour * 3600;
            int min = Mathf.FloorToInt(_seconds / 60);
            _seconds -= min * 60;
            return string.Format(mLimitedTimeStringHMS, hour, min, _seconds);
        }
        else
        {
            return string.Format(mLimitedTimeStringHMS, 0, 0, 0);
        }
    }

    public static DateTime ConvertFromUnixTimestamp(double timeStamp)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddSeconds(timeStamp);
    }

    public static double ConvertToUnitxTimeStamp(DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        TimeSpan diff = date - origin;
        return Math.Floor(diff.TotalSeconds);
    }

    public static string RandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
    }

    public static T DeepCopy<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }
    }


}
