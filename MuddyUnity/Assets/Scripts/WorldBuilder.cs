using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        SetCurLandmarkPosition();

        prev_landmark = null;
        cur_landmark = null;

        cur_path = Instantiate(path_prefab, transform);
        cur_path.name = name;

        return this;
    }

    public WorldBuilder node(string name, int lmargin=-1, int rmargin=-1, int los=-1)
    {
        SetCurLandmarkPosition();

        prev_landmark = cur_landmark;
        cur_landmark = Instantiate(landmark_prefab, cur_path.transform);

        string display_name = name;
        display_name = display_name.Replace('_', ' ');
        cur_landmark.name = display_name;

        margin(vnear);
        margin2(lmargin, rmargin);
        this.los(nearish);
        this.los(los);

        return this;
    }

    public WorldBuilder desc(string text)
    {
        if (cur_landmark)
        {
            cur_landmark.description = text;
        }
        else if (cur_path)
        {
            cur_path.description = text;
        }

        return this;
    }

    public WorldBuilder los(int los)
    {
        if (los >= 0)
        {
            cur_landmark.los = los;
        }
        return this;
    }

    public WorldBuilder margin(int left_right)
    {
        if (left_right >= 0)
        {
            cur_left_margin = left_right;
            cur_right_margin = left_right;
        }
        return this;
    }

    public WorldBuilder margin2(int left, int right = 0)
    {
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
        cur_landmark.door_name = name;
        return this;
    }

    private void SetCurLandmarkPosition()
    {
        if (cur_landmark)
        {
            cur_landmark.position = prev_landmark ? prev_landmark.position + Mathf.Max(prev_right_margin, cur_left_margin) : 0;
            prev_right_margin = cur_right_margin;
            cur_left_margin = 0;
            cur_right_margin = 0;
        }
    }
}
