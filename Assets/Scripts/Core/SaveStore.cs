using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every write to saved progress goes through here instead of straight to
/// PlayerPrefs. PlayerPrefs can't list its keys, so this keeps an index of
/// every key it has written. That index is what lets the whole save be
/// exported to the cloud and restored after a reinstall.
///
/// Reads can still use PlayerPrefs.Get* directly.
/// </summary>
public static class SaveStore
{
    private const string IndexKey = "CityBloxx_SaveIndex";
    private const string LastModifiedKey = "CityBloxx_SaveLastModified";

    private enum ValueType { Int, Float, String }

    [Serializable]
    private class Entry
    {
        public string k;   // key
        public int t;      // ValueType
        public string v;   // value (only used in exports)
    }

    [Serializable]
    private class EntryList
    {
        public long modified;
        public List<Entry> entries = new List<Entry>();
    }

    /// <summary>Raised after any change, e.g. to schedule a cloud upload.</summary>
    public static event Action Changed;

    /// <summary>Unix ms of the last local change (0 = nothing saved yet).</summary>
    public static long LastModified =>
        long.TryParse(PlayerPrefs.GetString(LastModifiedKey, "0"), out long t) ? t : 0;

    private static Dictionary<string, ValueType> index;

    // =========================================================
    // WRITES
    // =========================================================

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        Track(key, ValueType.Int);
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        Track(key, ValueType.Float);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        Track(key, ValueType.String);
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);

        LoadIndex();

        if (index.Remove(key))
            SaveIndex();

        MarkModified();
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    // =========================================================
    // CLOUD EXPORT / IMPORT
    // =========================================================

    /// <summary>Every tracked key and value as JSON.</summary>
    public static string Export()
    {
        LoadIndex();

        EntryList list = new EntryList { modified = LastModified };

        foreach (KeyValuePair<string, ValueType> pair in index)
        {
            if (!PlayerPrefs.HasKey(pair.Key))
                continue;

            string value;

            switch (pair.Value)
            {
                case ValueType.Int:
                    value = PlayerPrefs.GetInt(pair.Key).ToString();
                    break;
                case ValueType.Float:
                    value = PlayerPrefs.GetFloat(pair.Key)
                        .ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                default:
                    value = PlayerPrefs.GetString(pair.Key);
                    break;
            }

            list.entries.Add(new Entry { k = pair.Key, t = (int)pair.Value, v = value });
        }

        return JsonUtility.ToJson(list);
    }

    /// <summary>Timestamp stored inside an export, or 0 if unreadable.</summary>
    public static long GetExportModified(string json)
    {
        try
        {
            EntryList list = JsonUtility.FromJson<EntryList>(json);
            return list != null ? list.modified : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// Replaces all tracked local data with an export. Returns false (and
    /// changes nothing) if the JSON can't be read.
    /// </summary>
    public static bool Import(string json)
    {
        EntryList list;

        try
        {
            list = JsonUtility.FromJson<EntryList>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("SaveStore: Cloud save could not be read: " + e.Message);
            return false;
        }

        if (list == null || list.entries == null)
            return false;

        LoadIndex();

        foreach (string key in index.Keys)
            PlayerPrefs.DeleteKey(key);

        index.Clear();

        foreach (Entry entry in list.entries)
        {
            if (string.IsNullOrEmpty(entry.k))
                continue;

            ValueType type = (ValueType)entry.t;

            switch (type)
            {
                case ValueType.Int:
                    if (int.TryParse(entry.v, out int i))
                        PlayerPrefs.SetInt(entry.k, i);
                    break;
                case ValueType.Float:
                    if (float.TryParse(entry.v,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out float f))
                        PlayerPrefs.SetFloat(entry.k, f);
                    break;
                default:
                    PlayerPrefs.SetString(entry.k, entry.v ?? string.Empty);
                    break;
            }

            index[entry.k] = type;
        }

        SaveIndex();

        // Keep the cloud's timestamp so this device doesn't think it
        // has newer data and upload the same thing straight back.
        PlayerPrefs.SetString(LastModifiedKey, list.modified.ToString());
        PlayerPrefs.Save();

        return true;
    }

    // =========================================================
    // INDEX
    // =========================================================

    private static void Track(string key, ValueType type)
    {
        LoadIndex();

        if (!index.TryGetValue(key, out ValueType existing) || existing != type)
        {
            index[key] = type;
            SaveIndex();
        }

        MarkModified();
    }

    private static void MarkModified()
    {
        PlayerPrefs.SetString(
            LastModifiedKey,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
        );

        Changed?.Invoke();
    }

    private static void LoadIndex()
    {
        if (index != null)
            return;

        index = new Dictionary<string, ValueType>();

        string json = PlayerPrefs.GetString(IndexKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return;

        try
        {
            EntryList list = JsonUtility.FromJson<EntryList>(json);

            if (list?.entries == null)
                return;

            foreach (Entry entry in list.entries)
            {
                if (!string.IsNullOrEmpty(entry.k))
                    index[entry.k] = (ValueType)entry.t;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("SaveStore: Save index was unreadable: " + e.Message);
        }
    }

    private static void SaveIndex()
    {
        EntryList list = new EntryList();

        foreach (KeyValuePair<string, ValueType> pair in index)
            list.entries.Add(new Entry { k = pair.Key, t = (int)pair.Value });

        PlayerPrefs.SetString(IndexKey, JsonUtility.ToJson(list));
    }
}
