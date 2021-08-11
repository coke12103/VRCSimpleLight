using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Animations;

using AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using ExpressionsMenu = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using RuntimeAnimatorController = UnityEngine.RuntimeAnimatorController;

public class SimpleLightGUI : EditorWindow
{
  // 設定用情報
  private AvatarDescriptor target_avatar;
  private Light target_light1;
  private Light target_light2;
  // 調整値の種類
  private static readonly string[] LightTypes = { "Spot", "Point", "Spot and Point" };
  private static readonly string[] ColorTypes = { "RGB(Radial Puppet)", "Template(8 colors)", "Single color" };
  private static readonly string[] StrengthTypes = { "Stepless(Radial Puppet)", "8 Stage", "Single strength" };
  private static readonly string[] RangeTypes = { "Stepless(Radial Puppet)", "8 Stage", "Single range" };
  private static readonly string[] AngleTypes = { "Stepless(Radial Puppet)", "8 Stage", "Single angle"};

  // デフォルト値
  private const string prefix = "SimpleLight";
  private const string root = "Assets/coke12103/SimpleLight";

  private const string default_fx_path = "Assets/VRCSDK/Examples3/Animation/Controllers/vrc_AvatarV3HandsLayer.controller";
  private const string default_ex_menu_path = "Assets/VRCSDK/Examples3/Expressions Menu/DefaultExpressionsMenu.asset";
  private const string default_ex_param_path = "Assets/VRCSDK/Examples3/Expressions Menu/DefaultExpressionParameters.asset";

  private const int fx_index = 4;

  // 調整値
  private int light_mode;
  private int color_mode;
  private int strength_mode;
  private int range_mode;
  private int angle_mode;

  // その他
  private string user_asset_path;

  // TODO: デフォルト値を作る
  // color
  private Color[] template_colors = {Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white};
  private Color single_color = Color.white;

  // Strength
  private float min_strength, max_strength, single_strength;
  private float[] template_strengths = new float[8];

  // Range
  private float min_range, max_range, single_range;
  private float[] template_ranges = new float[8];
  // Angle
  private float min_angle, max_angle, single_angle;
  private float[] template_angles = new float[8];

  // その他
  private string message;

  [MenuItem("SimpleLight/Editor")]
  private static void Create(){
    SimpleLightGUI win = GetWindow<SimpleLightGUI>("SimpleLight");
  }

  private void OnGUI(){
    EditorGUILayout.LabelField("SimpleLightのインストール");

    target_avatar = EditorGUILayout.ObjectField("Avatar", target_avatar, typeof(AvatarDescriptor), true) as AvatarDescriptor;

    // そもそもAvatarがなければ無効化する
    EditorGUI.BeginDisabledGroup(target_avatar == null);
      light_mode = EditorGUILayout.Popup("Light type", light_mode, LightTypes);

      target_light1 = EditorGUILayout.ObjectField("Light 1", target_light1, typeof(Light), true) as Light;

      // ライトが1つで足りるモードであれば無効化
      if(light_mode == 2){
        target_light2 = EditorGUILayout.ObjectField("Light 2", target_light2, typeof(Light), true) as Light;
      }

      // color
      color_mode = EditorGUILayout.Popup("Color type", color_mode, ColorTypes);

      if(color_mode == 1){
        EditorGUILayout.LabelField("Colors");

        for(int i = 0; i < template_colors.Length; i++){
          template_colors[i] = EditorGUILayout.ColorField("Color " + (i + 1), template_colors[i]);
        }
      }else if(color_mode == 2){
        single_color = EditorGUILayout.ColorField("Color", single_color);
      }

      // strength
      strength_mode = EditorGUILayout.Popup("Strength type", strength_mode, StrengthTypes);

      if(strength_mode == 0){
        min_strength = EditorGUILayout.FloatField("Min strength", min_strength);
        max_strength = EditorGUILayout.FloatField("Max strength", max_strength);
      }else if(strength_mode == 1){
        EditorGUILayout.LabelField("Strength");

        for(int i = 0; i < template_strengths.Length; i++){
          template_strengths[i] = EditorGUILayout.FloatField("Strength " + (i + 1), template_strengths[i]);
        }
      }else if(strength_mode == 2){
        single_strength = EditorGUILayout.FloatField("Strength", single_strength);
      }

      // range
      range_mode = EditorGUILayout.Popup("Range type", range_mode, RangeTypes);

      if(range_mode == 0){
        min_range = EditorGUILayout.FloatField("Min range", min_range);
        max_range = EditorGUILayout.FloatField("Max range", max_range);
      }else if(range_mode == 1){
        EditorGUILayout.LabelField("Range");
        for(int i = 0; i < template_ranges.Length; i++){
          template_ranges[i] = EditorGUILayout.FloatField("Range " + (i + i), template_ranges[i]);
        }
      }else if(range_mode == 2){
        single_range = EditorGUILayout.FloatField("Range", single_range);
      }

      // angle
      if(light_mode == 0 || light_mode == 2){
        angle_mode = EditorGUILayout.Popup("Angle type", angle_mode, AngleTypes);

        if(angle_mode == 0){
          min_angle = EditorGUILayout.FloatField("Min angle", min_angle);
          max_range = EditorGUILayout.FloatField("Max angle", max_range);
        }else if(angle_mode == 1){
          EditorGUILayout.LabelField("Angle");

          for(int i = 0; i < template_angles.Length; i++){
            template_angles[i] = EditorGUILayout.FloatField("Angle " + (i + 1), template_angles[i]);
          }
        }else if(angle_mode == 2){
          single_angle = EditorGUILayout.FloatField("Angle", single_angle);
        }
      }

      EditorGUILayout.HelpBox("使用パラメーター: " + CountParams().ToString(), MessageType.Info);

      EditorGUILayout.HelpBox(message, MessageType.Info);

      EditorGUI.BeginDisabledGroup(!CheckCondition());
        if(GUILayout.Button("Install")){
          try{
            CheckInstallCondition();
          }catch(System.Exception e){
            Debug.Log(e.ToString());
            message = "Error: " + e.Message;
            return;
          }
          Install();
        }
      EditorGUI.EndDisabledGroup();
    EditorGUI.EndDisabledGroup();
  }

  bool CheckCondition(){
    if(target_avatar == null || target_light1 == null) return false;

    // 必要なライトがなければダメ
    if(light_mode == 2 && target_light2 == null) return false;

    return true;
  }

  int CountParams(){
    // bool on/off 1bit
    int result = 1;

    // bool spot/point 1bit
    if(light_mode == 2) result += 1;

    // float color(r, g, b) 8bit * 3
    if(color_mode == 0) result += (8*3);
    // int color template 8bit
    else if(color_mode == 1) result += 8;

    // float strength 8bit / int strength 8bit
    if(strength_mode == 0 || strength_mode == 1) result += 8;

    // float range 8bit / int range 8bit
    if(range_mode == 0 || range_mode == 1) result += 8;

    // spotのみの設定値
    // float angle 8bit / int angle 8bit
    if((light_mode == 0 || light_mode == 2) && (angle_mode == 0 || angle_mode == 1)) result += 8;

    return result;
  }

  void Install(){
    Debug.Log("button");

    SetupDirs();
    SetupDescriptor();
    RemoveOldParams();
    //CreateAnimatorParams();
  }

  void CheckInstallCondition(){
    string result = "";

    if(!AssetDatabase.IsValidFolder("Assets/VRCSDK")) result = "VRCSDKのフォルダがない";

    if(result != "") throw new System.Exception(result);
  }

  void SetupDirs(){
    string invalid_regex_text = "[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]";
    Regex invalid_regex = new Regex(invalid_regex_text);

    string valid_name = invalid_regex.Replace(target_avatar.name, "_");
    user_asset_path = root + "/User/" + valid_name;

    if(!AssetDatabase.IsValidFolder(root + "/User")) AssetDatabase.CreateFolder(root, "User");
    if(!AssetDatabase.IsValidFolder(user_asset_path)) AssetDatabase.CreateFolder(root + "/User", valid_name);
  }

  // FX/EX Menu/EX Paramがないアバターに対応する。
  void SetupDescriptor(){
    // baseAnimationLayers
    // base, additive, gesture, action, fx
    if(
      target_avatar.baseAnimationLayers[fx_index].isDefault
      || target_avatar.baseAnimationLayers[fx_index].animatorController == null
    ){
      // FXない処理
      target_avatar.customizeAnimationLayers = true;

      string fx_path = user_asset_path + "/FXLayer.controller";

      AssetDatabase.CopyAsset(default_fx_path, fx_path);
      target_avatar.baseAnimationLayers[fx_index].isDefault = false;
      target_avatar.baseAnimationLayers[fx_index].isEnabled = true;
      target_avatar.baseAnimationLayers[fx_index].animatorController = AssetDatabase.LoadAssetAtPath(fx_path, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;

      Debug.Log("FX作った");
    }

    if(!target_avatar.customExpressions) target_avatar.customExpressions = true;

    if(target_avatar.expressionsMenu == null){
      // ExMenuない処理
      string ex_menu_path = user_asset_path + "/ExMenu.asset";

      AssetDatabase.CopyAsset(default_ex_menu_path, ex_menu_path);
      target_avatar.expressionsMenu = AssetDatabase.LoadAssetAtPath(ex_menu_path, typeof(ExpressionsMenu)) as ExpressionsMenu;

      Debug.Log("ExMenu作った");
    }

    if(target_avatar.expressionParameters == null){
      // ExParamない処理
      string ex_param_path = user_asset_path + "/ExParam.asset";

      AssetDatabase.CopyAsset(default_ex_param_path, ex_param_path);
      target_avatar.expressionParameters = AssetDatabase.LoadAssetAtPath(ex_param_path, typeof(ExpressionParameters)) as ExpressionParameters;

      Debug.Log("ExParam作った");
    }
  }

  void RemoveOldParams(){
    AnimatorController fx_layer = target_avatar.baseAnimationLayers[fx_index].animatorController as AnimatorController;

    Debug.Log(fx_layer);
  }

  
}
