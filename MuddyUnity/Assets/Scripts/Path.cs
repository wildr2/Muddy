using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public const float outer_wall_margin = 2.0f;
    public const float inner_wall_margin = -1.0f;

    public string description;
    public float left_wall_pos;
    public float right_wall_pos;

    public void UpdateWallPosition(float player_pos)
    {
        left_wall_pos = float.MaxValue;
        right_wall_pos = float.MinValue;

        Landmark[] landmarks = GetComponentsInChildren<Landmark>();
        foreach (Landmark lm in landmarks)
        {
            left_wall_pos = Mathf.Min(left_wall_pos, lm.position);
            right_wall_pos = Mathf.Max(right_wall_pos, lm.position);
        }

        left_wall_pos -= outer_wall_margin;
        right_wall_pos += outer_wall_margin;

        foreach (Landmark lm in landmarks)
        {
            if (lm.is_wall)
            {
                if (lm.position > player_pos)
                {
                    right_wall_pos = Mathf.Min(right_wall_pos, lm.position + inner_wall_margin);
                }
                else
                {
                    left_wall_pos = Mathf.Max(left_wall_pos, lm.position - inner_wall_margin);
                }
            } 
        }
    }
}
