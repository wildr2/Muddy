using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public const float wall_margin = 2.0f;

    public string description;
    public float left_wall_pos;
    public float right_wall_pos;

    private void Start()
    {
        DetermineWallPositions();
    }

    private void DetermineWallPositions()
    {
        left_wall_pos = float.MaxValue;
        right_wall_pos = float.MinValue;

        Landmark[] landmarks = GetComponentsInChildren<Landmark>();
        foreach (Landmark lm in landmarks)
        {
            left_wall_pos = Mathf.Min(left_wall_pos, lm.position);
            right_wall_pos = Mathf.Max(right_wall_pos, lm.position);
        }

        left_wall_pos -= wall_margin;
        right_wall_pos += wall_margin;
    }
}
