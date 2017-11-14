using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections.Generic;
using Mobcast.Coffee.UI;

using EventType = Mobcast.Coffee.UI.ButtonEx.EventType;
using System.Linq;

namespace Mobcast.CoffeeEditor.UI
{
	/// <summary>
	/// ButtonExエディタ.
	/// </summary>
	[CustomEditor(typeof(ButtonEx), true)]
	public class ButtonExEditor : SelectableEditor
	{
		/// <summary>
		/// コールバック描画ディクショナリ.コールバックカリングが有効なときに実行するメソッドを定義します.
		/// </summary>
		Dictionary<EventType, System.Action> callbackDrawers;

		/// <summary>
		/// イベントタイプに対応するプロパティリスト.
		/// </summary>
		Dictionary<EventType, SerializedProperty> eventProperties;

		static GUIContent contentLoopOn;
		static GUIContent contentLoopOff;
		static GUIContent contentPressInterval;
		static GUIContent contentHoldThreshold;
		static Color COLOR_ENABLE = new Color(1, 1, 1);
		static Color COLOR_DISABLE = new Color(1, 1, 1, 0.6f);

		//==== v Editor callback v ====
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();

			contentLoopOn = new GUIContent(EditorGUIUtility.FindTexture("playloopon"), "Press Repeat");
			contentLoopOff = new GUIContent(EditorGUIUtility.FindTexture("playloopoff"), "Press Repeat");
			contentPressInterval = new GUIContent("Interval", "Interval time to invoke press callback");
			contentHoldThreshold = new GUIContent("Threshold", "Threshold time to invoke hold callback");

			eventProperties = new Dictionary<EventType, SerializedProperty>()
			{
				{ EventType.Click,serializedObject.FindProperty("m_OnClick") },
				{ EventType.Press,serializedObject.FindProperty("m_OnPress") },
				{ EventType.Hold,serializedObject.FindProperty("m_OnHold") },
			};

			callbackDrawers = new Dictionary<EventType, System.Action>()
			{
				//クリック.
				{
					EventType.Click,
					() =>
					{
						EditorGUILayout.LabelField("Click", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(eventProperties[EventType.Click]);

						var last = GUILayoutUtility.GetLastRect();
						var sp = serializedObject.FindProperty("m_ClickOnEscape");
						sp.boolValue = EditorGUI.ToggleLeft(new Rect(last.xMax - 130, last.y + 1, 130, 18), "Invoke On Esc Key", sp.boolValue);
					}
				},
				//プレス.
				{
					EventType.Press,
					() =>
					{
						EditorGUILayout.LabelField("Press", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(eventProperties[EventType.Press]);

						var last = GUILayoutUtility.GetLastRect();
						var spRepeat = serializedObject.FindProperty("m_PressRepeat");
						var spInterval = serializedObject.FindProperty("m_PressRepeatInterval");

						var color = GUI.color;
						GUI.color = spRepeat.boolValue ? COLOR_ENABLE : COLOR_DISABLE;
						var content = spRepeat.boolValue ? contentLoopOn : contentLoopOff;
						spRepeat.boolValue = GUI.Toggle(new Rect(last.xMax - 130, last.y + 1, 20, 20), spRepeat.boolValue, content, EditorStyles.label);
						EditorGUI.LabelField(new Rect(last.xMax - 110, last.y + 1, 50, 14), contentPressInterval);
						spInterval.floatValue = EditorGUI.FloatField(new Rect(last.xMax - 60, last.y + 2, 55, 14), spInterval.floatValue, EditorStyles.miniTextField);
						GUI.color = color;
					}
				},
				//ホールド.
				{
					EventType.Hold,
					() =>
					{
						EditorGUILayout.LabelField("Hold", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(eventProperties[EventType.Hold]);

						var last = GUILayoutUtility.GetLastRect();
						var spThreshold = serializedObject.FindProperty("m_HoldThreshold");

						EditorGUI.LabelField(new Rect(last.xMax - 130, last.y + 1, 70, 14), contentHoldThreshold);
						spThreshold.floatValue = EditorGUI.FloatField(new Rect(last.xMax - 60, last.y + 2, 55, 14), spThreshold.floatValue, EditorStyles.miniTextField);
					}
				},
			};
		}


		/// <summary>
		/// Raises the inspector GU event.
		/// </summary>
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();
			
			// 継承先プロパティの描画.
			DrawCustomInspector();
			GUILayout.Space(10);

			// イベントタイプ.
			var spType = serializedObject.FindProperty("m_EventType");
			int oldType = spType.intValue;
			spType.intValue = (int)((EventType)EditorGUILayout.EnumMaskField(new GUIContent("Event Type"), (EventType)spType.intValue));

			// イベントタイプに合わせて、必要ななイベントのみ詳細を描画.
			foreach (EventType e in System.Enum.GetValues(typeof(EventType)))
			{
				// イベントが有効化されているとき、イベント詳細を描画.
				if (0 < (spType.intValue & (int)e))
				{
					callbackDrawers[e]();
				}
				// イベントが無効化された場合、設定されているコールバックをリセット.
				else if (0 < (oldType & (int)e))
				{
					eventProperties[e].FindPropertyRelative("m_PersistentCalls").FindPropertyRelative("m_Calls").ClearArray();
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
		//==== ^ Editor callback ^ ====


		/// <summary>
		/// Draw custom inspector.
		/// 継承先クラスで定義されたプロパティの描画.
		/// </summary>
		protected virtual void DrawCustomInspector()
		{
			// Skip properties declared in ButtonEx.
			var itr = serializedObject.GetIterator();
			itr.NextVisible(true);
			while (itr.NextVisible(false) && itr.name != "m_OnHold")
				;

			// Draw properties declared in Custom-ButtonEx.
			while (itr.NextVisible(false))
			{
				EditorGUILayout.PropertyField(itr, true);
			}
		}
	}
}