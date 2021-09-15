using UnityEngine;
using System;

namespace coke12103.SimpleLight{
  [Serializable]
  public class LightData{
    // setting name
    public string name;
    // spot, point, spot and point
    public int light_mode;
    // rgb, template, single
    public int color_mode;
    // stepless, template, single
    public int intensity_mode;
    public int range_mode;
    public int angle_mode;

    public float max_intensity;
    public float min_intensity;
    public float max_range;
    public float min_range;
    public float max_angle;
    public float min_angle;

    // TODO: templateの長さを動的指定
    // public int template_colors_count;
    // public int template_intensity_count;
    // public int template_range_count;
    // public int template_angle_count;

    // 現状の仕様では最初の8個しか使わない
    public string[] template_colors;
    public float[] template_intensities;
    public float[] template_ranges;
    public float[] template_angles;

    public float single_color;
    public float single_intensity;
    public float single_range;
    public float single_angle;
  }

  public class LightSetting{
    // setting name
    public string name;
    // spot, point, spot and point
    public int light_mode;
    // rgb, template, single
    public int color_mode;
    // stepless, template, single
    public int intensity_mode;
    public int range_mode;
    public int angle_mode;

    public float max_intensity;
    public float min_intensity;
    public float max_range;
    public float min_range;
    public float max_angle;
    public float min_angle;

    // TODO: templateの長さを動的指定
    // public int template_colors_count;
    // public int template_intensity_count;
    // public int template_range_count;
    // public int template_angle_count;

    // 現状の仕様では最初の8個しか使わない
    public Color[] template_colors;
    public float[] template_intensities;
    public float[] template_ranges;
    public float[] template_angles;

    public Color single_color;
    public float single_intensity;
    public float single_range;
    public float single_angle;
  }
}