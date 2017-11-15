using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mobcast.Coffee.UI
{
	/// <summary>
	/// 高機能ボタン.
	/// 標準のボタンと比べ、以下の点が改善されています.
	/// 1. 再フォーカス時のクリック判定を無効.
	/// 2. Submit時、レイキャストを飛ばして、本当に押せるかどうかを判定.
	/// 3. Click On Escape - ESCキーでクリックできるように設定可能(Androidバックキー対応).
	/// 4. Callback Culling - Pressコールバックの追加.
	/// 5. Callback Culling - RepeatPress(Curveによるリピートウェイト曲線対応)コールバックの追加.
	/// 6. Callback Culling - LongPress(押下時間設定可能)コールバックの追加.
	/// </summary>

	[AddComponentMenu("UI/ButtonEx", 30)]
	public class ButtonEx : Button
	{
		/// <summary>クリック連打制限時間.</summary>
		const float kTimeIgnoreRapidClick = 0.2f;

		/// <summary>コールバック選択.</summary>
		public enum EventType
		{
			/// <summary>クリック.</summary>
			Click = 1 << 0,
			/// <summary>押下.</summary>
			Press = 1 << 1,
			/// <summary>長押し.</summary>
			Hold = 1 << 2,
		}

		//---- ▼ シリアライズ項目 ▼ ----
		/// <summary>コールバック選択.</summary>
		[SerializeField]
		EventType m_EventType = EventType.Click;
		
		/// <summary>Escキー押下時、クリック可能ならクリックを実行するか.</summary>
		[SerializeField]
		bool m_ClickOnEscape = false;

		/// <summary>押下リピートを有効にするか.</summary>
		public bool m_PressRepeat = false;

		/// <summary>リピート間隔時間.</summary>
		[Range(0.1f, 5.0f)]
		public float m_PressRepeatInterval = 1f;

		/// <summary>押下コールバック.</summary>
		public UnityEvent onPress { get { return this.m_OnPress; } }

		/// <summary>押下コールバック.</summary>
		[SerializeField]
		UnityEvent m_OnPress = new Button.ButtonClickedEvent();
		
		/// <summary>長押し時間のしきい値.</summary>
		[Range(0.1f, 5.0f)]
		[SerializeField]
		float m_HoldThreshold = 1;

		/// <summary>長押しコールバック.</summary>
		public UnityEvent onHold { get { return this.m_OnHold; } }

		/// <summary>長押しコールバック.</summary>
		[SerializeField]
		UnityEvent m_OnHold = new Button.ButtonClickedEvent();


		//---- ▲ シリアライズ項目 ▲ ----


		/// <summary>ポインタがボタン内にあるか.</summary>
		protected bool isInside = false;

		/// <summary>ポインタがボタンを押下しているか.</summary>
		protected bool isPress = false;

		/// <summary>押下継続時間.</summary>
		protected float timePressing = 0;

		/// <summary>リピート押下待機時間.</summary>
		float timeNextPress = 0;

		/// <summary>現在のESCキー押下状態.</summary>
		static bool isEscapeKeyPress;

		/// <summary>ESCキーチェックを最後に行ったフレーム.</summary>
		static int lastFrameTrigger;

		/// <summary>ESCキー対応ボタンリスト.</summary>
		static List<ButtonEx> buttonForEscapeKeyList = new List<ButtonEx>();

		/// <summary>レイキャスト結果.</summary>
		static List<RaycastResult> s_RaycastResult = new List<RaycastResult>();

		static Vector3[] s_WorldCorners = new Vector3[4];

		/// <summary>キャッシュ済みRectTransform.</summary>
		public RectTransform cachedTransform
		{
			get
			{
				if (!m_CachedTransform)
					m_CachedTransform = (transform as RectTransform);
				return m_CachedTransform;
			}
		}

		RectTransform m_CachedTransform;

		Graphic m_Graphic;

		Camera eventCamera { get { return (!m_Graphic || m_Graphic.canvas.renderMode == RenderMode.ScreenSpaceCamera || !m_Graphic.canvas.worldCamera) ? Camera.main : m_Graphic.canvas.worldCamera; } }

		Canvas canvas { get { return m_Graphic ? m_Graphic.canvas : null; } }

		/// <summary>
		/// クリック可能かどうかを返します.
		/// </summary>
		protected bool enableClick { get { return isActiveAndEnabled && interactable && (0 != (m_EventType & EventType.Click)) && (kTimeIgnoreRapidClick * Application.targetFrameRate) < (Time.frameCount - lastFrameTrigger); } }

		//==== v MonoBehavior Callbacks v ====
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();

			m_Graphic = GetComponent<Graphic>();

			if (m_ClickOnEscape && !buttonForEscapeKeyList.Contains(this))
				buttonForEscapeKeyList.Add(this);
		}

		/// <summary>
		/// Raises the disable event.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();

			if (buttonForEscapeKeyList.Contains(this))
				buttonForEscapeKeyList.Remove(this);
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		protected virtual void Update()
		{
			if (interactable && isPress && isInside)
			{
				// ボタン長押しのチェックを行う.
				CheckPressHold();

				// ボタン押下リピートのチェックを行う.
				CheckPressRepeat();
			}

			// エスケープボタンのチェックを行う.
			CheckEscapeButton();

			//
			if (isPress && isInside)
				timePressing += Time.unscaledDeltaTime;
			else
				timePressing = 0;
		}


		/// <summary>
		/// Raises the pointer enter event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			if (!isPress)
				isInside = true;
		}

		/// <summary>
		/// Raises the pointer exit event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			isInside = false;
		}

		/// <summary>
		/// Raises the pointer down event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			if (isActiveAndEnabled && interactable)
			{
				isPress = true;
				isInside = true;
				timePressing = 0;
				timeNextPress = timePressing + m_PressRepeatInterval;
				
				if (0 != (m_EventType & EventType.Press))
					onPress.Invoke();
			}
		}

		/// <summary>
		/// Raises the pointer up event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);
			isPress = false;
			timePressing = 0;

#if DISALLOW_REFOCUS
			// [再フォーカスを禁止している場合]
			// PointerUp時にクリック可能かどうかチェックします.
			//   * EventData がクリックと判定
			//   * PointerEnter 後、 PointerExit されていない
			//   * クリック連打に引っかかっておらず、クリックイベントが有効化されている
			if (eventData.eligibleForClick && isInside && enableClick)
			{
				ExecuteClick();
			}
#endif
		}

		/// <summary>
		/// Raises the pointer click event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerClick(PointerEventData eventData)
		{
#if !DISALLOW_REFOCUS
			// [再フォーカスを許可している場合(Unityデフォルト)]
			// PointerClick時にクリックを実行します.
			if (enableClick)
			{
				ExecuteClick();
			}
#endif
		}

		/// <summary>
		/// Raises the submit event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnSubmit(BaseEventData eventData)
		{
			if (enableClick && IsClickable())
			{
				ExecuteClick();
			}
		}
		//==== ^ MonoBehavior Callbacks ^ ====


		/// <summary>
		/// Submitを実行します.
		/// </summary>
		protected virtual void ExecuteClick()
		{
			try
			{
				lastFrameTrigger = Time.frameCount;
				base.OnPointerClick(new PointerEventData(EventSystem.current));
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex);
			}
		}

		/// <summary>
		/// ボタン押下リピートのチェックを行います.
		/// </summary>
		void CheckPressRepeat()
		{
			if (m_PressRepeat && timeNextPress < timePressing && 0 != (m_EventType & EventType.Press))
			{
				timeNextPress = timePressing + m_PressRepeatInterval;
				onPress.Invoke();
			}
		}

		/// <summary>
		/// ボタン長押しのチェックを行います.
		/// </summary>
		void CheckPressHold()
		{
			if (m_HoldThreshold < timePressing && 0 != (m_EventType & EventType.Hold))
			{
				isInside = false;
				isPress = false;
				timePressing = 0;

				onHold.Invoke();
			}
		}

		/// <summary>
		/// エスケープボタンのチェックを行います.
		/// </summary>
		void CheckEscapeButton()
		{
			//連打防止.
			if ((Time.frameCount - lastFrameTrigger) < (kTimeIgnoreRapidClick * Application.targetFrameRate))
				return;
			lastFrameTrigger = Time.frameCount;

#if UNITY_EDITOR
			s_ActiveButtonForEscapeKey = GetActiveButtonForEscapeKey();
#endif

			// Escキー押下トリガ.
			bool oldEsc = isEscapeKeyPress;
			isEscapeKeyPress = Input.GetKeyUp(KeyCode.Escape);
			if (!oldEsc && isEscapeKeyPress)
			{
				// 現在アクティブなエスケープキー対応ボタンを取得します.
				//ボタン押下可能な場合のみ、クリックを実行する.
				ButtonEx foregroundButton = GetActiveButtonForEscapeKey();
				if (foregroundButton)
				{
					try
					{
						lastFrameTrigger = Time.frameCount;
						foregroundButton.ExecuteClick();
					}
					catch (System.Exception ex)
					{
						Debug.LogError(ex);
					}
				}
			}
		}

		/// <summary>
		/// 現在アクティブなエスケープキー対応ボタンを取得します.
		/// 最も手前でレイキャストがヒットしたボタンを返します.
		/// </summary>
		static ButtonEx GetActiveButtonForEscapeKey()
		{
			return buttonForEscapeKeyList
				.Where(x => x.m_Graphic != null)
				.OrderByDescending(x => x.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? x.canvas.renderOrder : -1)
				.ThenByDescending(x => x.eventCamera.depth)
				.ThenByDescending(x => x.canvas.cachedSortingLayerValue)
				.ThenByDescending(x => x.canvas.sortingOrder)
				.ThenBy(x => x.canvas.planeDistance)
				.ThenByDescending(x => x.m_Graphic.depth)
				.FirstOrDefault(x => x.IsClickable());
		}

		/// <summary>
		/// クリック可能かどうか、レイキャストを行って判定します.
		/// </summary>
		bool IsClickable()
		{
			//ボタン押下不可能な状態だったらスキップします.
			if (!isActiveAndEnabled || !interactable || !m_Graphic || !EventSystem.current)
				return false;

			PointerEventData evData = new PointerEventData(EventSystem.current) { position = RectTransformToScreenPoint(canvas, cachedTransform) };
			s_RaycastResult.Clear();
			EventSystem.current.RaycastAll(evData, s_RaycastResult);

			//ヒットが無いならfalseを返します.
			if (0 == s_RaycastResult.Count)
			{
				s_RaycastResult.Clear();
				return false;
			}

			//クリック可能かチェックします.
			GameObject go = s_RaycastResult[0].gameObject;
			s_RaycastResult.Clear();
			return (go == gameObject) || (go.GetComponent<Graphic>() && go.transform.IsChildOf(transform) && (go.GetComponentInParent<Selectable>() == this));
		}

		/// <summary>
		/// ScreenSpaceに変換します.
		/// </summary>
		static Vector2 RectTransformToScreenPoint(Canvas canvas, RectTransform transform)
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
				Rect rect = new Rect(transform.position.x, transform.position.y, size.x, size.y);
				rect.x -= (transform.pivot.x * size.x);
				rect.y = rect.y - ((1.0f - transform.pivot.y) * size.y) + rect.height;
				return rect.center;
			}
			else
			{
				var camera = canvas.worldCamera ?? Camera.main;
				if (!camera)
					return Vector2.zero;
				
				transform.GetWorldCorners(s_WorldCorners);
				return RectTransformUtility.WorldToScreenPoint(camera, (s_WorldCorners[0] + s_WorldCorners[2]) / 2);
			}
		}


#if UNITY_EDITOR
		void OnValidate()
		{
			bool isBackButton = 0 < (m_EventType & EventType.Click) && m_ClickOnEscape && enabled;
			if (!isBackButton)
				buttonForEscapeKeyList.Remove(this);
			else if (!buttonForEscapeKeyList.Contains(this))
				buttonForEscapeKeyList.Add(this);
		}

		//#### v Gizmo v ####
		static ButtonEx s_ActiveButtonForEscapeKey;
		static GUIStyle s_Style;
		static GUIContent s_Content = new GUIContent("[Esc]");
		static readonly Color s_DisableColor = new Color(0.3f, 0.3f, 0.3f);
		static readonly Color s_EnableColor = Color.white;

		void OnDrawGizmos()
		{
			// This button is not for Esc key.
			if (!buttonForEscapeKeyList.Contains(this))
				return;

			cachedTransform.GetWorldCorners(s_WorldCorners);
			var position = (s_WorldCorners[0] + s_WorldCorners[2]) / 2;
			var cameraDistance = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, position);

			// Too far to display icon.
			if ((512 / cameraDistance - 0.5f) <= 0)
				return;

			Handles.BeginGUI();
			{
				if (s_Style == null)
					s_Style = new GUIStyle("sv_label_5");
				
				bool isActive = (s_ActiveButtonForEscapeKey == this);
				GUIContent content = new GUIContent("[Esc] " + name);
				s_Style.fixedWidth = s_Style.CalcSize(content).x;

				// Draw icon.
				GUI.backgroundColor = isActive ? s_EnableColor : s_DisableColor;
				{
					GUI.Toggle(HandleUtility.WorldPointToSizedRect(position, content, s_Style), !isActive, content, s_Style);
				}
				GUI.backgroundColor = Color.white;
			}
			Handles.EndGUI();
		}
		//#### ^ Gizmo ^ ####

		//==== v Context menu for editor v ====
		[MenuItem("CONTEXT/Button/Convert To ButtonEx", true)]
		static bool _ConvertToButtonEx(MenuCommand command)
		{
			return CanConvertTo<ButtonEx>(command.context);
		}

		[MenuItem("CONTEXT/Button/Convert To ButtonEx", false)]
		static void ConvertToButtonEx(MenuCommand command)
		{
			ConvertTo<ButtonEx>(command.context);
		}

		[MenuItem("CONTEXT/Button/Convert To Button", true)]
		static bool _ConvertToButton(MenuCommand command)
		{
			return CanConvertTo<Button>(command.context);
		}

		[MenuItem("CONTEXT/Button/Convert To Button", false)]
		static void ConvertToButton(MenuCommand command)
		{
			ConvertTo<Button>(command.context);
		}

		/// <summary>
		/// Verify whether it can be converted to the specified component.
		/// </summary>
		protected static bool CanConvertTo<T>(Object context)
			where T : MonoBehaviour
		{
			return context && context.GetType() != typeof(T);
		}

		/// <summary>
		/// Convert to the specified component.
		/// </summary>
		protected static void ConvertTo<T>(Object context) where T : MonoBehaviour
		{
			var target = context as MonoBehaviour;
			var so = new SerializedObject(target);
			so.Update();

			bool oldEnable = target.enabled;
			target.enabled = false;

			// Find MonoScript of the specified component.
			foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
			{
				if (script.GetClass() != typeof(T))
					continue;

				// Set 'm_Script' to convert.
				so.FindProperty("m_Script").objectReferenceValue = script;
				so.ApplyModifiedProperties();
				break;
			}

			(so.targetObject as MonoBehaviour).enabled = oldEnable;
		}
		//==== ^ Context menu for editor ^ ====
#endif
	}
}