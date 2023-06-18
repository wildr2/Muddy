using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldTagEntry
{
    public string name;
    public string tag_name;
    public WorldTag tag;

    public WorldTagEntry(string tag_name, int value)
    {
        this.tag_name = tag_name;
        tag = new WorldTag(value);
        UpdateInspectorName();
    }

    public void UpdateInspectorName()
    {
        name = tag_name + " " + tag.value;
    }

    public override string ToString()
    {
        return name;
    }
}

[System.Serializable]
public class WorldTag
{
    public int value;

    public WorldTag(int value)
    {
        this.value = value;
    }

    public static implicit operator WorldTag(int value)
    {
        return new WorldTag(value);
    }

    public static implicit operator int(WorldTag value)
    {
        return value.value;
    }

    public static implicit operator WorldTag(bool value)
    {
        return new WorldTag(value ? 1 : 0);
    }

    public static implicit operator bool(WorldTag value)
    {
        return value.value > 0;
    }

    public static implicit operator WorldTag(System.Enum value)
    {
        return new WorldTag(System.Convert.ToInt32(value));
    }
}


[System.Serializable]
public class WorldState
{
    public List<WorldTagEntry> tags;

    public WorldState()
    {
        tags = new List<WorldTagEntry>();
    }

    public WorldTag this[string key]
    {
        get => GetTagEntry(key).tag;
        set => SetTag(key, value);
    }

    public void SetTag(string tag, int value)
    {
        WorldTagEntry entry = GetTagEntry(tag);
        entry.tag = value;
        entry.UpdateInspectorName();
    }

    private WorldTagEntry GetTagEntry(string tag)
    {
        int index = tags.FindIndex((t) => t.tag_name == tag);
        if (index < 0)
        {
            tags.Add(new WorldTagEntry(tag, 0));
            index = tags.Count - 1;
        }
        return tags[index];
    }
}
