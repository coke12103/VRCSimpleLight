# SimpleLight
**この拡張はまだかなり不安定です。利用する場合は必ずプロジェクトのバックアップを取ってください。**

VRChatのアバターにライトを仕込むことができるUnityエディタ拡張です。

構成をかなり自由に設定できるので、ON/OFFのみの単純なライトから、「色(RGB)」「強さ」「範囲」「角度」が無段階調整できるよくわからない性能のライトまで自由自在。

## 特徴
- 選べる調整の種類
  - 無段階調整
    - 色(RGB) 
    - 光の強さ、範囲、角度(あらかじめMIN/MAXを指定、角度はSpotのみ)
  - 8段階
    - 色(あらかじめ8色指定)
    - 光の強さ、範囲、角度(あらかじめ8段階指定、角度はSpotのみ)
  - 調整なし
    - 色、光の強さ、範囲、角度(あらかじめ指定、角度はSpotのみ)
- 組み込まれるものはすべて動的生成

## 動作テスト環境
- Unity 2019.4.29f1
- VRCSDK3 2021.08.04.11.23

## 使い方
1. パッケージをインポート
2. ライトを新規作成してアバター配下の好きな位置に配置する。    
   ※基本的な設定は自動で調整されるので、スポットライトまたはポイントライトを新規作成して位置と角度だけ調整すればOKです。
3. 「メニュー/SimpleLight/Editor」から拡張のウインドウを表示する。
4. Avatarにライトを仕込みたいアバターを指定する。
5. Light typeを指定する。
6. Lightに2で配置したライトを指定する。
7. 好きなように調整する。
8. Installを押す。
9.  ちょっと待つ。

## 注意事項
- `SimpleLight`から始まるEXパラメーター/EXメニュー/アニメーターレイヤー/アニメーターパラメーターは削除されます。
- VRCSDK 2.0には対応してません。
- Write Defaults使ってます。(改善予定)
- 事前にアバターに指定されていた、アニメーターコントローラー、EXメニュー、EXパラメーターを除く、生成されたファイルはすべて上書きされます。
- ライトをアバターに仕込むとパフォーマンスランクがすごく悪化します。(1個でPoor、2個でVery Poor)

## ライセンス
[MIT No Attribution](https://github.com/coke12103/VRCSimpleLight/blob/master/LICENSE)