using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Landmark : MonoBehaviour
{
    public List<LandmarkDesc> descriptions = new List<LandmarkDesc>();
    public int position = 0;
    public int los = 10;
    public string door_name;
    public float x_lerp_speed = 10.0f; 
    public float y_lerp_speed = 2.0f;
    public int index = -1;
    public Landmark other_door;
    public float last_on_path_opacity;

    public Text inspect_indicator;

    private void Awake()
    {
        inspect_indicator = transform.GetChild(0).GetComponent<Text>();
    }

    public Path GetPath()
    {
        return transform.GetComponentInParent<Path>();
    }

    public string GetDesc(float observe_pos)
    {
        float observe_dist = Mathf.Abs(position - observe_pos);
        LandmarkDesc best_desc = null;
        foreach (LandmarkDesc desc in descriptions) {
            bool close_enough = desc.max_dist < 0 || observe_dist < desc.max_dist;
            bool better_desc = best_desc == null || best_desc.max_dist < 0 || (desc.max_dist >= 0 && desc.max_dist < best_desc.max_dist);
            if (close_enough && better_desc)
            {
                best_desc = desc;
            }
        }
        return best_desc != null ? best_desc.text : "A landmark.";
    }

    public static bool Filter(Landmark landmark, string search_str)
    {
        if (search_str.Trim().Length == 0)
        {
            return false;
        }
        string[] words = landmark.name.Split(' ');
        foreach (string word in words)
        {
            if (word.StartsWith(search_str))
            {
                return true;
            }
        }
        return false;
    }

    public void SetText(string text0, string text1, float t)
    {
        Text text = transform.GetComponent<Text>();
        if (t >= 1)
        {
            text.text = text1;
        }
        else
        {
            t = Mathf.Clamp01(t);
            int n = Mathf.Max(text0.Length, text1.Length);
            int x = Mathf.CeilToInt(Mathf.Lerp(0, n - 1, t));
            string str = "";
            for (int i = 0; i < n; ++i)
            {
                str += i >= x ? 
                    (i < text0.Length ? text0[i].ToString() : "") :
                    (i < text1.Length ? text1[i].ToString() : "");
            }
            text.text = str;
        }
    }
}

public class LandmarkDesc
{
    public string text;
    public float max_dist = -1;
}

public class CompareLandmarkPosition : IComparer
{
    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;
        return ((new CaseInsensitiveComparer()).Compare(lm1.position, lm2.position));
    }
}

public class CompareLandmarkName : IComparer
{
    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;
        return string.Compare(lm2.name, lm1.name);
    }
}

public class CompareLandmarkIndex : IComparer
{
    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;
        return ((new CaseInsensitiveComparer()).Compare(lm1.index, lm2.index));
    }
}

public class CompareLandmarkLOS : IComparer
{
    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;
        return ((new CaseInsensitiveComparer()).Compare(lm2.los, lm1.los));
    }
}

public class CompareLandmarkEdgeofVisionPos : IComparer
{
    float position;
    Path path;
    bool left_edge;

    public CompareLandmarkEdgeofVisionPos(float position, Path path, bool left_edge)
    {
        this.position = position;
        this.path = path;
        this.left_edge = left_edge;
    }

    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;

        Path path1 = lm1.GetPath();
        Path path2 = lm2.GetPath();
        if (path1 != path2)
        {
            return path1 == path ? -1 : path2 == path ? 1 : ((new CaseInsensitiveComparer()).Compare(path1.name, path2.name));
        }

        float pos1 = lm1.position + (lm1.los * (left_edge ? -1 : 1));
        float pos2 = lm2.position + (lm2.los * (left_edge ? -1 : 1));
        return ((new CaseInsensitiveComparer()).Compare(pos1, pos2));
    }
}

public class CompareLandmarkDistFromEdgeOfVision : IComparer
{
    float position;
    Path path;

    public CompareLandmarkDistFromEdgeOfVision(float position, Path path)
    {
        this.position = position;
        this.path = path;
    }

    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;

        Path p1 = lm1.GetPath();
        Path p2 = lm2.GetPath();
        if (p1 != p2)
        {
            return p1 == path ? -1 : p2 == path ? 1 : ((new CaseInsensitiveComparer()).Compare(p1.name, p2.name));
        }

        float d1 = Mathf.Abs(lm1.position - position) - lm1.los;
        float d2 = Mathf.Abs(lm2.position - position) - lm2.los;
        return ((new CaseInsensitiveComparer()).Compare(d1, d2));
    }
}

public class CompareLandmarkDist : IComparer
{
    float position;

    public CompareLandmarkDist(float position)
    {
        this.position = position;
    }

    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;
        float d1 = Mathf.Abs(lm1.position - position);
        float d2 = Mathf.Abs(lm2.position - position);
        return ((new CaseInsensitiveComparer()).Compare(d1, d2));
    }
}

public class CompareLandmarkVisibleDist : IComparer
{
    float position;

    public CompareLandmarkVisibleDist(float position)
    {
        this.position = position;
    }

    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;
        float d1 = Mathf.Abs(lm1.position - position);
        d1 = d1 > lm1.los ? float.MaxValue : d1;
        float d2 = Mathf.Abs(lm2.position - position);
        d2 = d2 > lm2.los ? float.MaxValue : d2;
        return ((new CaseInsensitiveComparer()).Compare(d2, d1));
    }
}

public class CompareLandmarkVisiblePos : IComparer
{
    float position;

    public CompareLandmarkVisiblePos(float position)
    {
        this.position = position;
    }

    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;
        float p1 = Mathf.Abs(lm1.position - position) > lm1.los ? float.MaxValue : lm1.position;
        float p2 = Mathf.Abs(lm2.position - position) > lm2.los ? float.MaxValue : lm2.position;
        return ((new CaseInsensitiveComparer()).Compare(p2, p1));
    }
}

public class CompareLandmarkVisibility : IComparer
{
    float position;
    Path path;

    public CompareLandmarkVisibility(float position, Path path)
    {
        this.position = position;
        this.path = path;
    }

    public int Compare(object x, object y)
    {
        Landmark lm1 = (Landmark)x;
        Landmark lm2 = (Landmark)y;

        Path p1 = lm1.GetPath();
        Path p2 = lm2.GetPath();
        if (p1 != p2)
        {
            return p1 == path ? -1 : p2 == path ? 1 : ((new CaseInsensitiveComparer()).Compare(p1.name, p2.name));
        }

        float v1 = lm1.los == 0.0 ? 0.0f : 1.0f - Mathf.Abs(lm1.position - position) / lm1.los;
        float v2 = lm2.los == 0.0 ? 0.0f : 1.0f - Mathf.Abs(lm2.position - position) / lm2.los;
        return ((new CaseInsensitiveComparer()).Compare(v2, v1));
    }
}

public class ReverseComparer : IComparer
{
    public IComparer comparer;

    public ReverseComparer(IComparer comparer)
    {
        this.comparer = comparer;
    }

    public int Compare(object x, object y)
    {
        return comparer.Compare(y, x);
    }
}