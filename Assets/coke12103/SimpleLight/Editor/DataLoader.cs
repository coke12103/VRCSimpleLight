using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using coke12103.SimpleLight;

namespace coke12103.SimpleLight{
  public class DataLoader{
    private static string data_path = "Assets/coke12103/SimpleLight/LightTemplate";

    private List<LightData> _datas;

    public List<LightSetting> datas;

    public DataLoader(){
      this.Load();
    }

    public void Load(){
      _datas = new List<LightData>();

      string[] filelist = AssetDatabase.FindAssets("", new string[] { data_path });

      foreach(string file_id in filelist){
        string file_path = AssetDatabase.GUIDToAssetPath(file_id);
        string json_string = AssetDatabase.LoadAssetAtPath(file_path, typeof(TextAsset)).ToString();

        LightData data = JsonUtility.FromJson<LightData>(json_string);

        _datas.Add(data);
      }

      _Load();
    }

    private void _Load(){
      datas = new List<LightSetting>();

      foreach(LightData data in _datas){
        LightSetting setting = new LightSetting();

        setting.name = data.name;

        setting.light_mode = is_in_range(data.light_mode, 0, 3) ? data.light_mode : 0;
        setting.color_mode = is_in_range(data.color_mode, 0, 3) ? data.color_mode : 0;
        setting.intensity_mode = is_in_range(data.intensity_mode, 0, 3) ? data.intensity_mode : 0;
        setting.range_mode = is_in_range(data.range_mode, 0, 3) ? data.range_mode : 0;
        setting.angle_mode = is_in_range(data.angle_mode, 0, 3) ? data.angle_mode : 0;

        setting.max_intensity = data.max_intensity;
        setting.max_range = data.max_range;
        setting.max_angle = is_in_range(data.max_angle, 0f, 179f) ? data.max_angle : 179f;
        setting.min_intensity = is_in_range(data.min_intensity, 0f, 9999999f) ? data.min_intensity : 0f;
        setting.min_range = is_in_range(data.min_range, 0f, 9999999f) ? data.min_range : 0f;
        setting.min_angle = is_in_range(data.min_range, 0f, 179f) ? data.min_range : 0f;

        // TODO: templateの長さを動的指定
        Color[] colors = new Color[data.template_colors.Length];

        for(int i = 0; i < colors.Length; i++){
          Color cl = Color.white;
          // パースできなきゃ勝手に白になる
          ColorUtility.TryParseHtmlString("#" + data.template_colors[i], out cl);
          colors[i] = cl;
        }

        setting.template_colors = colors;

        float[] intensities = new float[data.template_intensities.Length];

        for(int i = 0; i < intensities.Length; i++){
          intensities[i] = is_in_range(data.template_intensities[i], 0f, 9999999f) ? data.template_intensities[i] : 0f;
        }

        setting.template_intensities = intensities;

        float[] ranges = new float[data.template_ranges.Length];

        for(int i = 0; i < ranges.Length; i++){
          ranges[i] = is_in_range(data.template_ranges[i], 0f, 9999999f) ? data.template_ranges[i] : 0f;
        }

        setting.template_ranges = ranges;

        float[] angles = new float[data.template_angles.Length];

        for(int i = 0; i < angles.Length; i++){
          angles[i] = is_in_range(data.template_angles[i], 0f, 179f) ? data.template_angles[i] : 0f;
        }

        setting.template_angles = angles;

        Color cl2 = Color.white;
        ColorUtility.TryParseHtmlString("#" + data.single_color, out cl2);
        setting.single_color = cl2;

        setting.single_intensity = data.single_intensity;
        setting.single_range = data.single_range;
        setting.single_angle = is_in_range(data.single_angle, 0f, 179f) ? data.single_angle : 0f;

        datas.Add(setting);
      }
    }

    private bool is_in_range(int val, int min, int max){
      return (val >= min && val <= max);
    }

    private bool is_in_range(float val, float min, float max){
      return (val >= min && val <= max);
    }
  }
}