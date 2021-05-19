using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionData
{
    public float[] root_pos { get; set; }
    public float[][] local_quat { get; set; }
    public string[] joint_names { get; set; }
}