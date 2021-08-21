using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Animations;

using AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using ExpressionsMenu = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
using ExpressionsControl = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using RuntimeAnimatorController = UnityEngine.RuntimeAnimatorController;

public class SimpleLightGUI : EditorWindow
{
  // 設定用情報
  private AvatarDescriptor target_avatar;
  private Light target_light_spot;
  private Light target_light_point;
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

      // spot
      if(light_mode == 0 || light_mode == 2){
        target_light_spot = EditorGUILayout.ObjectField("Light(Spot)", target_light_spot, typeof(Light), true) as Light;
      }

      // point
      if(light_mode == 1 || light_mode == 2){
        target_light_point = EditorGUILayout.ObjectField("Light(Point)", target_light_point, typeof(Light), true) as Light;
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
    if(target_avatar == null) return false;

    if(light_mode == 0 && target_light_spot == null) return false;
    if(light_mode == 1 && target_light_point == null) return false;

    if(light_mode == 2 && (target_light_point == null || target_light_spot == null)) return false;

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
    // ここから削除処理
    RemoveOldParams();
    RemoveOldLeyers();
    RemoveOldExParam();
    RemoveOldExMenu();
    // ここまで削除処理
    // ここから追加処理
    CreateAnimatorParams();
    CreateAnimatorLayer();
    FixLightsSetting();
    CreateAndBuildAnimation();
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

    AnimatorControllerParameter[] orig_params = fx_layer.parameters;
    AnimatorControllerParameter[] removed_params = new AnimatorControllerParameter[orig_params.Length];

    int count = 0;

    for(int i = 0; i < orig_params.Length; i++){
      AnimatorControllerParameter param = orig_params[i];

      if(!param.name.StartsWith(prefix)){
        removed_params[i] = param;
        count++;
      }else{
        Debug.Log("Removed: " + param.name);
      }
    }

    System.Array.Resize(ref removed_params, count);

    fx_layer.parameters = removed_params;
  }

  void RemoveOldLeyers(){
    AnimatorController fx_layer = target_avatar.baseAnimationLayers[fx_index].animatorController as AnimatorController;

    AnimatorControllerLayer[] orig_layers = fx_layer.layers;
    AnimatorControllerLayer[] removed_layers = new AnimatorControllerLayer[orig_layers.Length];

    int count = 0;

    for(int i = 0; i < orig_layers.Length; i++){
      AnimatorControllerLayer layer = orig_layers[i];

      if(!layer.name.StartsWith(prefix)){
        removed_layers[i] = layer;
        count++;
      }else{
        Debug.Log("Removed: " + layer.name);
      }
    }

    System.Array.Resize(ref removed_layers, count);

    fx_layer.layers = removed_layers;
  }

  void RemoveOldExParam(){
    ExpressionParameters ex_param = target_avatar.expressionParameters;

    ExpressionParameter[] orig_ex_params = ex_param.parameters;
    ExpressionParameter[] removed_ex_params = new ExpressionParameter[orig_ex_params.Length];

    for(int i = 0; i < orig_ex_params.Length; i++){
      ExpressionParameter param = orig_ex_params[i];

      // 空のパラメーター消せるけどなんとなく無視する
      if(!param.name.StartsWith(prefix)){
        removed_ex_params[i] = param;
      }else{
        Debug.Log("Removed: " + param.name);
      }

      ex_param.parameters = removed_ex_params;
    }
  }

  void RemoveOldExMenu(){
    ExpressionsMenu ex_menu = target_avatar.expressionsMenu;

    int i = 0;
    while(i < ex_menu.controls.Count){
      if(ex_menu.controls[i].name.StartsWith(prefix)){
        Debug.Log("Removed: " + ex_menu.controls[i].name);
        ex_menu.controls.RemoveAt(i);
        continue;
      }else{
        i++;
      }
    }
  }

  void CreateAnimatorParams(){
    AnimatorController fx_layer = target_avatar.baseAnimationLayers[fx_index].animatorController as AnimatorController;

    // bool on/off
    fx_layer.AddParameter(prefix + "Enable", AnimatorControllerParameterType.Bool);

    // bool spot/point
    if(light_mode == 2) fx_layer.AddParameter(prefix + "Mode", AnimatorControllerParameterType.Bool);

    if(color_mode == 0){
      // float color(r, g, b)
      fx_layer.AddParameter(prefix + "ColorR", AnimatorControllerParameterType.Float);
      fx_layer.AddParameter(prefix + "ColorG", AnimatorControllerParameterType.Float);
      fx_layer.AddParameter(prefix + "ColorB", AnimatorControllerParameterType.Float);
    }else if(color_mode == 1){
      // int color template
      fx_layer.AddParameter(prefix + "Color", AnimatorControllerParameterType.Int);
    }

    // float strength / int strength
    if(strength_mode == 0 || strength_mode == 1){
      fx_layer.AddParameter(prefix + "Strength", strength_mode == 0 ? AnimatorControllerParameterType.Float : AnimatorControllerParameterType.Int);
    }

    // float range / int range
    if(range_mode == 0 || range_mode == 1){
      fx_layer.AddParameter(prefix + "Range", range_mode == 0 ? AnimatorControllerParameterType.Float : AnimatorControllerParameterType.Int);
    }

    // spotのみの設定値
    // float angle / int angle
    if((light_mode == 0 || light_mode == 2) && (angle_mode == 0 || angle_mode == 1)){
      fx_layer.AddParameter(prefix + "Angle", angle_mode == 0 ? AnimatorControllerParameterType.Float : AnimatorControllerParameterType.Int);
    }
  }

  void CreateAnimatorLayer(){
    AnimatorController fx_layer = target_avatar.baseAnimationLayers[fx_index].animatorController as AnimatorController;

    fx_layer.AddLayer(prefix + "Enable");

    if(color_mode == 0){
      fx_layer.AddLayer(prefix + "ColorR");
      fx_layer.AddLayer(prefix + "ColorG");
      fx_layer.AddLayer(prefix + "ColorB");
    }else if(color_mode == 1){
      fx_layer.AddLayer(prefix + "Color");
    }

    if(strength_mode == 0 || strength_mode == 1){
      fx_layer.AddLayer(prefix + "Strength");
    }

    if(range_mode == 0 || range_mode == 1){
      fx_layer.AddLayer(prefix + "Range");
    }

    // spotのみの設定値
    if((light_mode == 0 || light_mode == 2) && (angle_mode == 0 || angle_mode == 1)){
      fx_layer.AddLayer(prefix + "Angle");
    }

    FixLayerWeight(fx_layer);
  }

  void FixLayerWeight(AnimatorController anim_con){
    AnimatorControllerLayer[] orig_layers = anim_con.layers;
    AnimatorControllerLayer[] fixed_layers = new AnimatorControllerLayer[orig_layers.Length];

    for(int i = 0; i < orig_layers.Length; i++){
      AnimatorControllerLayer layer = orig_layers[i];

      if(layer.name.StartsWith(prefix)){
        layer.defaultWeight = 1.0f;
        Debug.Log("Weight fix: " + layer.name);
      }

      fixed_layers[i] = layer;
    }

    anim_con.layers = fixed_layers;
  }

  void FixLightsSetting(){
    if(target_light_spot != null){
      target_light_spot.type = LightType.Spot;
      target_light_spot.lightmapBakeType = LightmapBakeType.Realtime;
      target_light_spot.renderMode = LightRenderMode.ForcePixel;
    }

    if(target_light_point != null){
      target_light_point.type = LightType.Point;
      target_light_point.lightmapBakeType = LightmapBakeType.Realtime;
      target_light_point.renderMode = LightRenderMode.ForcePixel;
    }
  }

  void CreateAndBuildAnimation(){
    AnimatorController fx_layer = target_avatar.baseAnimationLayers[fx_index].animatorController as AnimatorController;

    AnimationClip off_anim = new AnimationClip();

    // 単体か
    if(light_mode == 2){
      AddCurve(off_anim, target_light_point.gameObject.transform, typeof(GameObject), "isActive", 0);
      AddCurve(off_anim, target_light_spot.gameObject.transform, typeof(GameObject), "isActive", 0);
    }else{
      Transform target = (light_mode == 0 ? target_light_spot : target_light_point).gameObject.transform;

      AddCurve(off_anim, target, typeof(GameObject), "isActive", 0);
    }

    AssetDatabase.CreateAsset(off_anim, user_asset_path + "/off.anim");

    AnimatorState off_state = AddStateClip(fx_layer, prefix + "Enable", off_anim);
    AnimatorStateTransition off_transition = CreateAnyStateTransition(fx_layer, prefix + "Enable", off_state);

    off_transition.AddCondition(AnimatorConditionMode.IfNot, 0, prefix + "Enable");

    // NOTE: 何故かFXをSetDirtyしなくてもちゃんと反映される
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();

//    // bool spot/point
//    if(light_mode == 2) fx_layer.AddParameter(prefix + "Mode", AnimatorControllerParameterType.Bool);
//
//    if(color_mode == 0){
//      // float color(r, g, b)
//      fx_layer.AddParameter(prefix + "ColorR", AnimatorControllerParameterType.Float);
//      fx_layer.AddParameter(prefix + "ColorG", AnimatorControllerParameterType.Float);
//      fx_layer.AddParameter(prefix + "ColorB", AnimatorControllerParameterType.Float);
//    }else if(color_mode == 1){
//      // int color template
//      fx_layer.AddParameter(prefix + "Color", AnimatorControllerParameterType.Int);
//    }
//
//    // float strength / int strength
//    if(strength_mode == 0 || strength_mode == 1){
//      fx_layer.AddParameter(prefix + "Strength", strength_mode == 0 ? AnimatorControllerParameterType.Float : AnimatorControllerParameterType.Int);
//    }
//
//    // float range / int range
//    if(range_mode == 0 || range_mode == 1){
//      fx_layer.AddParameter(prefix + "Range", range_mode == 0 ? AnimatorControllerParameterType.Float : AnimatorControllerParameterType.Int);
//    }
//
//    // spotのみの設定値
//    // float angle / int angle
//    if((light_mode == 0 || light_mode == 2) && (angle_mode == 0 || angle_mode == 1)){
//      fx_layer.AddParameter(prefix + "Angle", angle_mode == 0 ? AnimatorControllerParameterType.Float : AnimatorControllerParameterType.Int);
//    }
  }

  void AddCurve(AnimationClip clip, Transform target, System.Type target_type, string key, float value){
    AnimationCurve curve = new AnimationCurve();

    // 親はavatarで固定
    string path = AnimationUtility.CalculateTransformPath(target, target_avatar.gameObject.transform);

    curve.AddKey(0, value);

    clip.SetCurve(path, target_type, key, curve);
  }

  AnimatorState AddStateClip(AnimatorController anim, string layer_name, AnimationClip clip){
    AnimatorControllerLayer[] layers = anim.layers;

    AnimatorStateMachine state_machine = layers[GetLayerIndex(anim, layer_name)].stateMachine;

    AnimatorState state = state_machine.AddState(clip.name);

    state.motion = clip;

    // NOTE: UnityのドキュメントにはLayersはコピーだから変更したら戻せよって書いてあるんだけど何故か上書きしなくても反映されてしかもディスクへの書き出しまでされる。怖いから一応やる。
    anim.layers = layers;

    return state;
  }

  AnimatorStateTransition CreateAnyStateTransition(AnimatorController anim, string layer_name, AnimatorState state){
    AnimatorControllerLayer[] layers = anim.layers;

    AnimatorStateMachine machine = layers[GetLayerIndex(anim, layer_name)].stateMachine;

    AnimatorStateTransition transition = machine.AddAnyStateTransition(state);

    transition.duration = 0;
    transition.hasExitTime = false;

    anim.layers = layers;

    return transition;
  }

  // ない場合は想定しない(実装的にない場合は例外なので)
  int GetLayerIndex(AnimatorController anim, string name){
    AnimatorControllerLayer[] layers = anim.layers;

    for(int i = 0; i < layers.Length; i++){
      AnimatorControllerLayer layer = layers[i];

      if(layer.name == name){
        return i;
      }
    }

    throw new System.Exception("そんなやつない");
  }
}
