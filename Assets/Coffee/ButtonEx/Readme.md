ButtonEx
===

## Overview

Button with extended feature for uGUI.

![image](https://user-images.githubusercontent.com/12690315/32772216-8a96c922-c968-11e7-9e50-edaf44f21bd9.png)

* 再フォーカス時のクリック判定を無効できます.
    * スクリプト定義シンボルに `DISALLOW_REFOCUS` を追加してください.
* エディタにて、スペースキーやエンターキーを押した時に、ボタンがクリック不可能な状態にもかかわらずクリックされる問題を修正します.
    * 実際にレイキャストを飛ばして、本当に押せるかどうかを判定します.
* ESCキーでClickイベントを発火できます(Androidバックキー対応).
    * `Invoke On Esc Key` を有効化してください.
    * 実際にレイキャストを飛ばして、本当に押せるかどうかを判定します.
    * クリック可能かつ最前面にあるボタンのみEscキーが反応します.
    * シーンビューより、ESCキー対応ボタンをギズモで確認できます.  
    ![image](https://user-images.githubusercontent.com/12690315/32772546-86e65b2a-c969-11e7-899b-f8b4d8315c77.png)
* 以下のイベントタイプを追加します.
    * Press : ボタンを押したときのイベントです.
        * Press-Repeat( ![Repeat](https://user-images.githubusercontent.com/12690315/32772850-9765fcac-c96a-11e7-88cb-95c3e265f200.png) )を有効にすると、ボタンを押し続けた時に繰り返しPressイベントを発火します.
    * Hold : ボタンを一定時間押し続けたとき(長押し)のイベントです
        * Holdイベントを発火すると、Press-Repeatは停止します.
* コンテキストメニューより、既存のButtonコンポーネントをButtonExへ変換できます.
    * ButtonEx > Button の変換もできます.  
    ![image](https://user-images.githubusercontent.com/12690315/32772668-e60e43ba-c969-11e7-9fef-cc7233f54c7a.png)


## Requirement

* Unity5.3+ *(included Unity 2017.x)*
* No other SDK is required.




## Usage

1. Download [ButtonEx.unitypackage](https://github.com/mob-sakai/ButtonEx/raw/develop/ButtonEx.unitypackage) and install on your unity project.
1. AddComponent `ButtonEx` to the GameObject.
1. Enjoy!




## Demo

WebGL: https://developer.cloud.unity3d.com/share/WkmVXpqkkm/




## Release Notes

### ver.1.1.0:

* Fixed: デモシーンのアスペクト比を修正
* Changed: 再フォーカス時クリック判定について、スクリプト定義シンボル `DISALLOW_REFOCUS` で切り替えられるように変更しました.

### ver.1.0.0:
* 再フォーカス時のクリック判定を無効にします.
* エディタにて、スペースキーやエンターキーを押した時に、ボタンがクリック不可能な状態にもかかわらずクリックされる問題を修正します.
* ESCキーでClickイベントを発火できます(Androidバックキー対応).
* 以下のイベントタイプを追加します.
    * Press : ボタンを押したときのイベントです.
    * Hold : ボタンを一定時間押し続けたとき(長押し)のイベントです
* コンテキストメニューより、既存のButtonコンポーネントをButtonExへ変換できます.




## See Also

* GitHub Page : https://github.com/mob-sakai/ButtonEx
* Issue tracker : https://github.com/mob-sakai/ButtonEx/issues