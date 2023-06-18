using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class WorldBuilder : MonoBehaviour
{
    public const int vnear = 3;
    public const int near = 5;
    public const int nearish = 10;
    public const int med = 15;
    public const int farish = 30;
    public const int far = 100;

    public Path path_prefab;
    public Landmark landmark_prefab;

    private Path cur_path;
    private Landmark prev_landmark;
    private Landmark cur_landmark;
    private int prev_right_margin;
    private int cur_left_margin;
    private int cur_right_margin;
    private Stack<bool> cond_stack = new Stack<bool>();

    private Dictionary<string, Path> path_cache = new Dictionary<string, Path>();
    private Dictionary<string, Landmark> node_cache = new Dictionary<string, Landmark>();
    public WorldState state = new WorldState();

    public void UpdateWorld()
    {
        Landmark[] landmarks = GetComponentsInChildren<Landmark>();
        foreach (Landmark lm in landmarks)
        {
            lm.PreRebuild();
        }

        cur_landmark = null;
        cur_path = null;
        prev_landmark = null;
        prev_right_margin = 0;
        cur_left_margin = 0;
        cur_right_margin = 0;
        cond_stack.Clear();

        Build();

        landmarks = GetComponentsInChildren<Landmark>();
        foreach (Landmark lm in landmarks)
        {
            if (lm.reference_count == 0)
            {
                Destroy(lm.gameObject);
            }
        }
        
        SetupLandmarks();
    }

    protected virtual void Build()
    {
    }

    private void SetupLandmarks()
    {
        Landmark[] landmarks = GetComponentsInChildren<Landmark>();
        for (int i = 0; i < landmarks.Length; ++i)
        {
            Landmark lm = landmarks[i];
            if (lm.door_name != "")
            {
                for (int j = i + 1; j < landmarks.Length; ++j)
                {
                    Landmark lm2 = landmarks[j];
                    if (lm.door_name == lm2.door_name)
                    {
                        lm.other_door = lm2;
                        lm2.other_door = lm;
                    }
                }
            }
        }
    }

    public WorldBuilder begin()
    {
        return this;
    }

    public void end()
    {
        SetCurLandmarkPosition();
    }

    public WorldBuilder path(string name)
    {
        if (!CheckCond()) { return this; }
        SetCurLandmarkPosition();

        prev_landmark = null;
        cur_landmark = null;

        if (!path_cache.TryGetValue(name, out cur_path))
        {
            cur_path = Instantiate(path_prefab, transform);
            path_cache[name] = cur_path;
        }
        cur_path.name = name;

        return this;
    }

    public WorldBuilder node(string name, int lmargin=-1, int rmargin=-1, int los=-1)
    {
        if (!CheckCond()) { return this; }
        SetCurLandmarkPosition();

        prev_landmark = cur_landmark;

        string fullname = cur_path.name + "/" + name;
        if (!node_cache.TryGetValue(fullname, out cur_landmark))
        {
            cur_landmark = Instantiate(landmark_prefab, cur_path.transform);
            node_cache[fullname] = cur_landmark;
        }
        ++cur_landmark.reference_count;
        if (cur_landmark.reference_count > 1)
        {
            Debug.LogError(String.Format("Landmark '{0}' referenced multiple times.", fullname));
        }

        string display_name = name;
        display_name = display_name.Replace('_', ' ');
        display_name = Regex.Replace(display_name, @"#(\w+)", "");
        cur_landmark.name = display_name;

        margin(vnear);
        margin2(lmargin, rmargin);
        this.los(nearish);
        this.los(los);

        return this;
    }

    public WorldBuilder cond(Func<WorldState, bool> func)
    {
        cond_stack.Push(CheckCond() && func(state));
        return this;
    }

    public WorldBuilder end_cond()
    {
        cond_stack.Pop();
        return this;
    }

    public WorldBuilder desc(string text, float dist=-1)
    {
        if (!CheckCond()) { return this; }
        if (cur_landmark)
        {
            LandmarkDesc desc = new LandmarkDesc
            {
                text = text,
                max_dist = dist,
            };
            cur_landmark.descriptions.Add(desc);
        }
        else if (cur_path)
        {
            cur_path.description = text;
        }

        return this;
    }

    public WorldBuilder act(float dist=-1, System.Action<WorldState> action=null)
    {
        if (!CheckCond()) { return this; }
        if (cur_landmark)
        {
            cur_landmark.action = new LandmarkAction
            {
                action = action,
                max_dist = dist,
            };
        }

        return this;
    }


    public WorldBuilder los(int los)
    {
        if (!CheckCond()) { return this; }
        if (los >= 0)
        {
            cur_landmark.los = (int)(los * 1.5f);
        }
        return this;
    }

    public WorldBuilder margin(int left_right)
    {
        if (!CheckCond()) { return this; }
        if (left_right >= 0)
        {
            cur_left_margin = left_right;
            cur_right_margin = left_right;
        }
        return this;
    }

    public WorldBuilder margin2(int left, int right = 0)
    {
        if (!CheckCond()) { return this; }
        if (left >= 0)
        {
            cur_left_margin = left;
        }
        if (right >= 0)
        {
            cur_right_margin = right;
        }
        return this;
    }

    public WorldBuilder door(string name)
    {
        if (!CheckCond()) { return this; }
        cur_landmark.door_name = name;
        return this;
    }

    public WorldBuilder wall()
    {
        if (!CheckCond()) { return this; }
        cur_landmark.is_wall = true;
        return this;
    }

    private bool CheckCond()
    {
        return cond_stack.Count == 0 || cond_stack.Peek();
    }

    private void SetCurLandmarkPosition()
    {
        if (cur_landmark)
        {
            float margin = Mathf.Max(prev_right_margin, cur_left_margin);

            string fullname = cur_path.name + "/" + cur_landmark.position;
            UnityEngine.Random.InitState(fullname.GetHashCode());
            float noise = 5 * UnityEngine.Random.value;

            cur_landmark.position = prev_landmark ? prev_landmark.position + (int)(margin + noise) : 0;
            prev_right_margin = cur_right_margin;
            cur_left_margin = 0;
            cur_right_margin = 0;
        }
    }
}
