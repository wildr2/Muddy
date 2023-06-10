using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoughDistance
{
    Near,
    Nearish,
    Med,
    Farish,
    Far,
}

public class TestNode : MonoBehaviour
{
    public string desc;
    public RoughDistance los = RoughDistance.Near;
    public RoughDistance margin = RoughDistance.Near;
    public RoughDistance left_margin = RoughDistance.Near;
    public RoughDistance right_margin = RoughDistance.Near;
}
