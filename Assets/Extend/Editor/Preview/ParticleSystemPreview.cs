﻿using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Rendering;

// Disable it
[Extend.Common.CustomPreview(typeof(ParticleSystem), true)]
// ReSharper disable once UnusedType.Global
public class ParticleSystemPreview : ObjectPreview {
	private class Styles {
		public readonly GUIContent speedScale = IconContent("SpeedScale", "Changes particle preview speed");
		public readonly GUIContent pivot = IconContent("AvatarPivot", "Displays avatar's pivot and mass center");

		public readonly GUIContent[] play = {
			IconContent("preAudioPlayOff", "Play"),
			IconContent("preAudioPlayOn", "Stop")
		};

		public GUIContent lockParticleSystem = IconContent("IN LockButton", "Lock the current particle");
		public readonly GUIContent reload = new GUIContent("Reload", "Reload particle preview");
		public readonly GUIStyle preButton = "preButton";
		public GUIStyle preSlider = "preSlider";
		public readonly GUIStyle preSliderThumb = "preSliderThumb";
		public readonly GUIStyle preLabel = "preLabel";
	}

	/// <summary>
	/// 视图工具
	/// </summary>
	protected enum ViewTool {
		None,

		/// <summary>
		/// 平移
		/// </summary>
		Pan,

		/// <summary>
		/// 缩放
		/// </summary>
		Zoom,

		/// <summary>
		/// 旋转
		/// </summary>
		Orbit
	}

	private PreviewRenderUtility m_PreviewUtility;

	// 预览实例对象
	private GameObject m_PreviewInstance;

	// 原点圆形标识
	private GameObject m_ReferenceInstance;

	// 方向箭头标识
	private GameObject m_DirectionInstance;

	// 轴三箭头标识
	private GameObject m_PivotInstance;

	// 根三箭头标识
	private GameObject m_RootInstance;
	private Vector2 m_PreviewDir = new Vector2(120f, -20f);
	private Mesh m_FloorPlane;
	private Texture2D m_FloorTexture;
	private Material m_FloorMaterial;
	private float m_AvatarScale = 1f;
	private float m_ZoomFactor = 1f;
	private Vector3 m_PivotPositionOffset = Vector3.zero;
	private float m_BoundingVolumeScale;
	protected ViewTool m_ViewTool;
	private bool m_ShowReference;
	private bool m_Loaded;
	private int m_PreviewHint = "Preview".GetHashCode();
	private int m_PreviewSceneHint = "PreviewSene".GetHashCode();

	private bool m_Playing;
	private float m_RunningTime;
	private double m_PreviousTime;
	private float m_PlaybackSpeed = 1f;
	private bool m_LockParticleSystem = true;
	private bool m_HasPreview;
	private Editor m_CacheEditor;
	private const float kDuration = 99f;
	private static int PreviewCullingLayer = 31;
	private static Styles s_Styles;
	private static readonly int Alphas = Shader.PropertyToID("_Alphas");

	private Vector3 bodyPosition => m_PreviewInstance.transform.position;

	protected ViewTool viewTool {
		get {
			var current = Event.current;
			if( m_ViewTool == ViewTool.None ) {
				var flag = current.control && Application.platform == RuntimePlatform.OSXEditor;
				var actionKey = EditorGUI.actionKey;
				var flag2 = !actionKey && !flag && !current.alt;
				if( ( current.button <= 0 && flag2 ) || ( current.button <= 0 && actionKey ) || current.button == 2 ) {
					m_ViewTool = ViewTool.Pan;
				}
				else {
					if( ( current.button <= 0 && flag ) || ( current.button == 1 && current.alt ) ) {
						m_ViewTool = ViewTool.Zoom;
					}
					else {
						if( ( current.button <= 0 && current.alt ) || current.button == 1 ) {
							m_ViewTool = ViewTool.Orbit;
						}
					}
				}
			}

			return m_ViewTool;
		}
	}

	private MouseCursor currentCursor {
		get {
			switch( m_ViewTool ) {
				case ViewTool.Pan:
					return MouseCursor.Pan;
				case ViewTool.Zoom:
					return MouseCursor.Zoom;
				case ViewTool.Orbit:
					return MouseCursor.Orbit;
				default:
					return MouseCursor.Arrow;
			}
		}
	}

	public override void Initialize(UnityEngine.Object[] targets) {
		base.Initialize(targets);

		if( m_CacheEditor == null ) {
			var editors = ActiveEditorTracker.sharedTracker.activeEditors;
			foreach( var editor in editors ) {
				if( editor.target == target ) {
					m_CacheEditor = editor;
					break;
				}
			}

			if( m_CacheEditor == null && editors.Length > 0 ) {
				m_CacheEditor = editors[0];
			}
		}

		m_HasPreview = EditorUtility.IsPersistent(target) && HasStaticPreview();
		if( m_Targets.Length != 1 ) {
			m_HasPreview = false;
		}
	}

	public override bool HasPreviewGUI() {
		return m_HasPreview;
	}

	public void SetEditor(Editor editor) {
		m_CacheEditor = editor;
	}

	public override void OnPreviewGUI(Rect r, GUIStyle background) {
		InitPreview();
		if( m_PreviewUtility == null ) {
			return;
		}

		var rect2 = r;
		var controlID = GUIUtility.GetControlID(m_PreviewHint, FocusType.Passive, rect2);
		var current = Event.current;
		var typeForControl = current.GetTypeForControl(controlID);

		if( typeForControl == EventType.Repaint ) {
			m_PreviewUtility.BeginPreview(rect2, background);
			DoRenderPreview();
			m_PreviewUtility.EndAndDrawPreview(rect2);
		}

		var controlID2 = GUIUtility.GetControlID(m_PreviewSceneHint, FocusType.Passive);
		typeForControl = current.GetTypeForControl(controlID2);
		HandleViewTool(current, typeForControl, controlID2, rect2);
		DoAvatarPreviewFrame(current, typeForControl, rect2);
		EditorGUI.DropShadowLabel(new Rect(r.x, r.yMax - 20f, r.width, 20f),
			( r.width > 140f ? "Playback Time:" : string.Empty ) + $"{m_RunningTime:F}");

		if( current.type == EventType.Repaint ) {
			EditorGUIUtility.AddCursorRect(rect2, currentCursor);
		}
	}

	public override GUIContent GetPreviewTitle() {
		var content = base.GetPreviewTitle();
		content.text = "Particle Preview";
		return content;
	}

	public override void OnPreviewSettings() {
		if( s_Styles == null ) {
			s_Styles = new Styles();
		}

		InitPreview();
		if( m_PreviewUtility == null ) {
			if( GUILayout.Button(s_Styles.reload, s_Styles.preButton) ) {
				m_Loaded = false;
			}

			return;
		}

		EditorGUI.BeginChangeCheck();
		m_ShowReference = GUILayout.Toggle(m_ShowReference, s_Styles.pivot, s_Styles.preButton);
		if( EditorGUI.EndChangeCheck() ) {
			EditorPrefs.SetBool("AvatarpreviewShowReference", m_ShowReference);
		}

		//EditorGUI.BeginChangeCheck();
		//m_LockParticleSystem = GUILayout.Toggle(m_LockParticleSystem, s_Styles.lockParticleSystem, s_Styles.preButton);
		//if (EditorGUI.EndChangeCheck())
		//{
		//    SetSimulateMode();
		//}

		var flag = CycleButton(!m_Playing ? 0 : 1, s_Styles.play, s_Styles.preButton) != 0;
		if( flag != m_Playing ) {
			if( flag ) {
				SimulateEnable();
			}
			else {
				SimulateDisable();
			}
		}

		GUILayout.Box(s_Styles.speedScale, s_Styles.preLabel);
		EditorGUI.BeginChangeCheck();
		m_PlaybackSpeed = PreviewSlider(m_PlaybackSpeed, 0.03f);
		if( EditorGUI.EndChangeCheck() && m_PreviewInstance ) {
			var particleSystems = m_PreviewInstance.GetComponentsInChildren<ParticleSystem>(true);
			foreach( var particleSystem in particleSystems ) {
				var main = particleSystem.main;
				main.simulationSpeed = m_PlaybackSpeed;
			}
		}

		GUILayout.Label(m_PlaybackSpeed.ToString("f2"), s_Styles.preLabel);
	}

	public override void ReloadPreviewInstances() {
		Debug.Log("reload");
		if( m_PreviewUtility == null ) {
			return;
		}

		CreatePreviewInstances();
	}

	public override string GetInfoString() {
		return " ";
	}

	private void InitPreview() {
		if( m_Loaded ) {
			return;
		}

		m_Loaded = true;

		if( m_PreviewUtility == null ) {
			m_PreviewUtility = new PreviewRenderUtility(true) {cameraFieldOfView = 30f};

			GetPreviewCamera().cullingMask = 1 << PreviewCullingLayer;
			GetPreviewCamera().allowHDR = false;
			GetPreviewCamera().allowMSAA = false;
			CreatePreviewInstances();
		}

		if( m_FloorPlane == null ) {
			m_FloorPlane = Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh;
		}

		if( m_FloorTexture == null ) {
			m_FloorTexture = (Texture2D)EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");
		}

		if( m_FloorMaterial == null ) {
			var shader = EditorGUIUtility.LoadRequired("Previews/PreviewPlaneWithShadow.shader") as Shader;
			m_FloorMaterial = new Material(shader) {mainTexture = m_FloorTexture, mainTextureScale = Vector2.one * 5f * 4f};
			m_FloorMaterial.SetVector(Alphas, new Vector4(0.5f, 0.3f, 0f, 0f));
			m_FloorMaterial.hideFlags = HideFlags.HideAndDontSave;
		}

		if( m_ReferenceInstance == null ) {
			var original = (GameObject)EditorGUIUtility.Load("Avatar/dial_flat.prefab");
			m_ReferenceInstance = UnityEngine.Object.Instantiate(original, Vector3.zero, Quaternion.identity);
			InitInstantiatedPreviewRecursive(m_ReferenceInstance);
			AddSingleGO(m_ReferenceInstance);
		}

		if( m_DirectionInstance == null ) {
			var original2 = (GameObject)EditorGUIUtility.Load("Avatar/arrow.fbx");
			m_DirectionInstance = UnityEngine.Object.Instantiate(original2, Vector3.zero, Quaternion.identity);
			InitInstantiatedPreviewRecursive(m_DirectionInstance);
			AddSingleGO(m_DirectionInstance);
		}

		if( m_PivotInstance == null ) {
			var original3 = (GameObject)EditorGUIUtility.Load("Avatar/root.fbx");
			m_PivotInstance = UnityEngine.Object.Instantiate(original3, Vector3.zero, Quaternion.identity);
			InitInstantiatedPreviewRecursive(m_PivotInstance);
			AddSingleGO(m_PivotInstance);
		}

		if( m_RootInstance == null ) {
			var original4 = (GameObject)EditorGUIUtility.Load("Avatar/root.fbx");
			m_RootInstance = UnityEngine.Object.Instantiate(original4, Vector3.zero, Quaternion.identity);
			InitInstantiatedPreviewRecursive(m_RootInstance);
			AddSingleGO(m_RootInstance);
		}

		m_ShowReference = EditorPrefs.GetBool("AvatarpreviewShowReference", true);
		SetPreviewCharacterEnabled(false, false);
	}

	private bool HasStaticPreview() {
		if( target == null ) {
			return false;
		}

		var gameObject = target as GameObject;
		return gameObject.GetComponentInChildren<ParticleSystem>(true);
	}

	private void DoRenderPreview() {
		var pos = this.bodyPosition;
		var quaternion = Quaternion.identity;
		var vector = Vector3.zero;
		var quaternion2 = Quaternion.identity;
		var pivotPos = Vector3.zero;

		var oldFog = SetupPreviewLightingAndFx();
		var forward = quaternion2 * Vector3.forward;
		forward[1] = 0f;
		var directionRot = Quaternion.LookRotation(forward);
		var directionPos = vector;
		var pivotRot = quaternion;
		PositionPreviewObjects(pivotRot, pivotPos, quaternion2, pos, directionRot, quaternion, vector, directionPos, m_AvatarScale);

		GetPreviewCamera().nearClipPlane = 0.5f * m_ZoomFactor;
		GetPreviewCamera().farClipPlane = 100f * m_AvatarScale;
		var rotation = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0f);
		var position2 = rotation * ( Vector3.forward * -5.5f * m_ZoomFactor ) + pos + m_PivotPositionOffset;
		GetPreviewCamera().transform.position = position2;
		GetPreviewCamera().transform.rotation = rotation;

		var identity = Quaternion.identity;
		var position = m_ReferenceInstance.transform.position;
		var floorMaterial = m_FloorMaterial;
		var matrix2 = Matrix4x4.TRS(position, identity, Vector3.one * 5f * m_AvatarScale);
		floorMaterial.mainTextureOffset = -new Vector2(position.x, position.z) * 5f * 0.08f * ( 1f / m_AvatarScale );
		floorMaterial.SetVector(Alphas, new Vector4(0.5f * 1f, 0.3f * 1f, 0f, 0f));

		var pipeline = GraphicsSettings.renderPipelineAsset;
		GraphicsSettings.renderPipelineAsset = null;
		Graphics.DrawMesh(m_FloorPlane, matrix2, floorMaterial, PreviewCullingLayer, GetPreviewCamera(), 0);
		SetPreviewCharacterEnabled(true, m_ShowReference);
		GetPreviewCamera().Render();
		SetPreviewCharacterEnabled(false, false);
		TeardownPreviewLightingAndFx(oldFog);
		GraphicsSettings.renderPipelineAsset = pipeline;
	}

	private void CreatePreviewInstances() {
		DestroyPreviewInstances();
		var gameObject = UnityEngine.Object.Instantiate(target) as GameObject;
		InitInstantiatedPreviewRecursive(gameObject);
		AddSingleGO(gameObject);
		var component = gameObject.GetComponent<Animator>();
		if( component ) {
			component.enabled = false;
			component.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			component.logWarnings = false;
			component.fireEvents = false;
		}

		m_PreviewInstance = gameObject;
		//Debug.Log("OnCreate");

		var bounds = new Bounds(m_PreviewInstance.transform.position, Vector3.zero);
		GetRenderableBoundsRecurse(ref bounds, m_PreviewInstance);
		if( bounds.size == Vector3.zero ) {
			bounds.size = Vector3.one;
		}

		m_BoundingVolumeScale = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
		m_AvatarScale = ( m_ZoomFactor = m_BoundingVolumeScale / 2f );
	}

	private void DestroyPreviewInstances() {
		if( m_PreviewInstance == null ) {
			return;
		}

		UnityEngine.Object.DestroyImmediate(m_PreviewInstance);
		UnityEngine.Object.DestroyImmediate(m_FloorMaterial);
		UnityEngine.Object.DestroyImmediate(m_ReferenceInstance);
		UnityEngine.Object.DestroyImmediate(m_RootInstance);
		UnityEngine.Object.DestroyImmediate(m_PivotInstance);
		UnityEngine.Object.DestroyImmediate(m_DirectionInstance);
	}

	private void AddSingleGO(GameObject go) {
		m_PreviewUtility.AddSingleGO(go);
	}

	private Camera GetPreviewCamera() {
		return m_PreviewUtility.camera;
	}

	private bool SetupPreviewLightingAndFx() {
		var lights = m_PreviewUtility.lights;
		lights[0].intensity = 1.4f;
		lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
		lights[1].intensity = 1.4f;
		var ambient = new Color(0.1f, 0.1f, 0.1f, 0f);
		InternalEditorUtility.SetCustomLighting(lights, ambient);
		var fog = RenderSettings.fog;
		Unsupported.SetRenderSettingsUseFogNoDirty(false);
		return fog;
	}

	private static void TeardownPreviewLightingAndFx(bool oldFog) {
		Unsupported.SetRenderSettingsUseFogNoDirty(oldFog);
		InternalEditorUtility.RemoveCustomLighting();
	}

	private void SetPreviewCharacterEnabled(bool enabled, bool showReference) {
		if( m_PreviewInstance != null ) {
			SetEnabledRecursive(m_PreviewInstance, enabled);
		}

		SetEnabledRecursive(m_ReferenceInstance, showReference && enabled);
		SetEnabledRecursive(m_DirectionInstance, showReference && enabled);
		SetEnabledRecursive(m_PivotInstance, showReference && enabled);
		SetEnabledRecursive(m_RootInstance, showReference && enabled);
	}

	private void PositionPreviewObjects(Quaternion pivotRot, Vector3 pivotPos, Quaternion bodyRot, Vector3 bodyPos,
		Quaternion directionRot, Quaternion rootRot, Vector3 rootPos, Vector3 directionPos, float scale) {
		m_ReferenceInstance.transform.position = rootPos;
		m_ReferenceInstance.transform.rotation = rootRot;
		m_ReferenceInstance.transform.localScale = Vector3.one * scale * 1.25f;
		m_DirectionInstance.transform.position = directionPos;
		m_DirectionInstance.transform.rotation = directionRot;
		m_DirectionInstance.transform.localScale = Vector3.one * scale * 2f;
		m_PivotInstance.transform.position = pivotPos;
		m_PivotInstance.transform.rotation = pivotRot;
		m_PivotInstance.transform.localScale = Vector3.one * scale * 0.1f;
		m_RootInstance.transform.position = bodyPos;
		m_RootInstance.transform.rotation = bodyRot;
		m_RootInstance.transform.localScale = Vector3.one * scale * 0.25f;
	}

	/// <summary>
	/// 最后的销毁方法
	/// 不能被Unity自动调用，目前只能在点下一个对象时，调用销毁
	/// </summary>
	public void OnDestroy() {
		ClearLockedParticle();
		SimulateDisable();
		DestroyPreviewInstances();
		if( m_PreviewUtility != null ) {
			//Debug.Log("OnDestroy");
			m_PreviewUtility.Cleanup();
			m_PreviewUtility = null;
		}
	}

	/// <summary>
	/// 模拟-开启
	/// </summary>
	private void SimulateEnable() {
		SimulateDisable();
		SetSimulateMode();
		if( m_LockParticleSystem ) {
			var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
			if( particleSystem ) {
				particleSystem.Play();
				ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
			}
		}

		m_PreviousTime = EditorApplication.timeSinceStartup;

		EditorApplication.update -= InspectorUpdate;
		EditorApplication.update += InspectorUpdate;
		m_RunningTime = 0f;
		m_Playing = true;
	}

	/// <summary>
	/// 模拟-停止
	/// </summary>
	private void SimulateDisable() {
		if( m_LockParticleSystem ) {
			ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
			ParticleSystemEditorUtilsReflect.editorPlaybackTime = 0f;
			ParticleSystemEditorUtilsReflect.StopEffect();
		}

		EditorApplication.update -= InspectorUpdate;
		m_RunningTime = 0f;
		m_Playing = false;
	}

	/// <summary>
	/// 模拟-更新方法
	/// 锁定粒子方式的话，只需要刷新编辑器即可
	/// </summary>
	private void SimulateUpdate() {
		if( m_LockParticleSystem ) {
			Repaint();
			return;
		}

		var gameObject = m_PreviewInstance;
		var particleSystem = gameObject.GetComponentInChildren<ParticleSystem>(true);
		if( particleSystem ) {
			particleSystem.Simulate(m_RunningTime, true);
			Repaint();
		}
	}

	private void InspectorUpdate() {
		var delta = EditorApplication.timeSinceStartup - m_PreviousTime;
		m_PreviousTime = EditorApplication.timeSinceStartup;

		if( m_Playing ) {
			m_RunningTime = Mathf.Clamp(m_RunningTime + (float)delta, 0f, kDuration);
			SimulateUpdate();
		}
	}

	/// <summary>
	/// 设置模拟方式
	/// 一种直接调用Simulate方法，效果跟直接运行播放不一定一样
	/// 一种调用锁定粒子，再调用play方法，效果跟直接运行播放一样
	/// </summary>
	private void SetSimulateMode() {
		SimulateDisable();
		if( m_PreviewInstance ) {
			var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
			if( !particleSystem )
				return;
			if( m_LockParticleSystem ) {
				if( ParticleSystemEditorUtilsReflect.lockedParticleSystem != particleSystem ) {
					ParticleSystemEditorUtilsReflect.lockedParticleSystem = particleSystem;
				}
			}
			else {
				ParticleSystemEditorUtilsReflect.lockedParticleSystem = null;
			}
		}
	}

	private void Repaint() {
		/* 如果有使用 https://github.com/akof1314/ObjectPickerAdvanced
		EditorWindow ew = EditorWindow.focusedWindow;
		if (ew && ew as ObjectSelectorWindow)
		{
		    ew.Repaint();
		    return;
		}*/
		if( m_CacheEditor ) {
			m_CacheEditor.Repaint();
		}
	}

	/// <summary>
	/// 解锁粒子
	/// </summary>
	private void ClearLockedParticle() {
		if( m_PreviewInstance ) {
			var particleSystem = m_PreviewInstance.GetComponentInChildren<ParticleSystem>(true);
			if( particleSystem ) {
				if( m_LockParticleSystem && ParticleSystemEditorUtilsReflect.lockedParticleSystem == particleSystem ) {
					ParticleSystemEditorUtilsReflect.lockedParticleSystem = null;
				}
			}
		}
	}

	private void HandleMouseDown(Event evt, int id, Rect previewRect) {
		if( viewTool != ViewTool.None && previewRect.Contains(evt.mousePosition) ) {
			EditorGUIUtility.SetWantsMouseJumping(1);
			evt.Use();
			GUIUtility.hotControl = id;
		}
	}

	private void HandleMouseUp(Event evt, int id) {
		if( GUIUtility.hotControl == id ) {
			m_ViewTool = ViewTool.None;
			GUIUtility.hotControl = 0;
			EditorGUIUtility.SetWantsMouseJumping(0);
			evt.Use();
		}
	}

	private void HandleMouseDrag(Event evt, int id, Rect previewRect) {
		if( m_PreviewInstance == null ) {
			return;
		}

		if( GUIUtility.hotControl == id ) {
			switch( m_ViewTool ) {
				case ViewTool.Pan:
					DoAvatarPreviewPan(evt);
					break;
				case ViewTool.Zoom:
					DoAvatarPreviewZoom(evt, -HandleUtility.niceMouseDeltaZoom * ( ( !evt.shift ) ? 0.5f : 2f ));
					break;
				case ViewTool.Orbit:
					DoAvatarPreviewOrbit(evt, previewRect);
					break;
				default:
					Debug.Log("Enum value not handled");
					break;
			}
		}
	}

	protected void HandleViewTool(Event evt, EventType eventType, int id, Rect previewRect) {
		switch( eventType ) {
			case EventType.MouseDown:
				HandleMouseDown(evt, id, previewRect);
				break;
			case EventType.MouseUp:
				HandleMouseUp(evt, id);
				break;
			case EventType.MouseDrag:
				HandleMouseDrag(evt, id, previewRect);
				break;
			case EventType.ScrollWheel:
				DoAvatarPreviewZoom(evt, HandleUtility.niceMouseDeltaZoom * ( ( !evt.shift ) ? 0.5f : 2f ));
				break;
		}
	}

	private void DoAvatarPreviewOrbit(Event evt, Rect previewRect) {
		m_PreviewDir -= evt.delta * (!evt.shift ? 1 : 3) / Mathf.Min(previewRect.width, previewRect.height) * 140f;
		m_PreviewDir.y = Mathf.Clamp(m_PreviewDir.y, -90f, 90f);
		evt.Use();
	}

	private void DoAvatarPreviewPan(Event evt) {
		var camera = GetPreviewCamera();
		var vector = camera.WorldToScreenPoint(bodyPosition + m_PivotPositionOffset);
		var a = new Vector3(-evt.delta.x, evt.delta.y, 0f);
		vector += a * Mathf.Lerp(0.25f, 2f, m_ZoomFactor * 0.5f);
		var b = camera.ScreenToWorldPoint(vector) - ( bodyPosition + m_PivotPositionOffset );
		m_PivotPositionOffset += b;
		evt.Use();
	}

	/// <summary>
	/// 定位事件
	/// 按F近距离查看对象
	/// 按G视图平移到鼠标位置
	/// </summary>
	/// <param name="evt"></param>
	/// <param name="type"></param>
	/// <param name="previewRect"></param>
	private void DoAvatarPreviewFrame(Event evt, EventType type, Rect previewRect) {
		if( type == EventType.KeyDown && evt.keyCode == KeyCode.F ) {
			m_PivotPositionOffset = Vector3.zero;
			m_ZoomFactor = m_AvatarScale;
			evt.Use();
		}

		if( type == EventType.KeyDown && Event.current.keyCode == KeyCode.G ) {
			m_PivotPositionOffset = GetCurrentMouseWorldPosition(evt, previewRect) - bodyPosition;
			evt.Use();
		}
	}

	private Vector3 GetCurrentMouseWorldPosition(Event evt, Rect previewRect) {
		var camera = GetPreviewCamera();
		var scaleFactor = m_PreviewUtility.GetScaleFactor(previewRect.width, previewRect.height);
		return camera.ScreenToWorldPoint(new Vector3(( evt.mousePosition.x - previewRect.x ) * scaleFactor,
			( previewRect.height - ( evt.mousePosition.y - previewRect.y ) ) * scaleFactor, 0f) {
			z = Vector3.Distance(bodyPosition, camera.transform.position)
		});
	}

	private void DoAvatarPreviewZoom(Event evt, float delta) {
		var num = -delta * 0.05f;
		m_ZoomFactor += m_ZoomFactor * num;
		m_ZoomFactor = Mathf.Max(m_ZoomFactor, m_AvatarScale / 10f);
		evt.Use();
	}

	private float PreviewSlider(float val, float snapThreshold) {
		val = GUILayout.HorizontalSlider(val, 0.1f, 3f, s_Styles.preSlider, s_Styles.preSliderThumb, GUILayout.MaxWidth(64f));
		if( val > 0.25f - snapThreshold && val < 0.25f + snapThreshold ) {
			val = 0.25f;
		}
		else {
			if( val > 0.5f - snapThreshold && val < 0.5f + snapThreshold ) {
				val = 0.5f;
			}
			else {
				if( val > 0.75f - snapThreshold && val < 0.75f + snapThreshold ) {
					val = 0.75f;
				}
				else {
					if( val > 1f - snapThreshold && val < 1f + snapThreshold ) {
						val = 1f;
					}
					else {
						if( val > 1.25f - snapThreshold && val < 1.25f + snapThreshold ) {
							val = 1.25f;
						}
						else {
							if( val > 1.5f - snapThreshold && val < 1.5f + snapThreshold ) {
								val = 1.5f;
							}
							else {
								if( val > 1.75f - snapThreshold && val < 1.75f + snapThreshold ) {
									val = 1.75f;
								}
							}
						}
					}
				}
			}
		}

		return val;
	}

	private static void SetEnabledRecursive(GameObject go, bool enabled) {
		var componentsInChildren = go.GetComponentsInChildren<Renderer>();
		foreach( var renderer in componentsInChildren ) {
			renderer.enabled = enabled;
		}
	}

	private static void GetRenderableBoundsRecurse(ref Bounds bounds, GameObject go) {
		var meshRenderer = go.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
		var meshFilter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
		if( meshRenderer && meshFilter && meshFilter.sharedMesh ) {
			if( bounds.extents == Vector3.zero ) {
				bounds = meshRenderer.bounds;
			}
			else {
				bounds.Encapsulate(meshRenderer.bounds);
			}
		}

		var skinnedMeshRenderer = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
		if( skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh ) {
			if( bounds.extents == Vector3.zero ) {
				bounds = skinnedMeshRenderer.bounds;
			}
			else {
				bounds.Encapsulate(skinnedMeshRenderer.bounds);
			}
		}

		var spriteRenderer = go.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
		if( spriteRenderer && spriteRenderer.sprite ) {
			if( bounds.extents == Vector3.zero ) {
				bounds = spriteRenderer.bounds;
			}
			else {
				bounds.Encapsulate(spriteRenderer.bounds);
			}
		}

		foreach( Transform transform in go.transform ) {
			GetRenderableBoundsRecurse(ref bounds, transform.gameObject);
		}
	}

	private static void InitInstantiatedPreviewRecursive(GameObject go) {
		go.hideFlags = HideFlags.HideAndDontSave;
		go.layer = PreviewCullingLayer;
		foreach( Transform transform in go.transform ) {
			InitInstantiatedPreviewRecursive(transform.gameObject);
		}
	}

	private static int CycleButton(int selected, GUIContent[] contents, GUIStyle style) {
		var flag = GUILayout.Button(contents[selected], style);
		if( flag ) {
			var num = selected;
			selected = num + 1;
			var flag2 = selected >= contents.Length;
			if( flag2 ) {
				selected = 0;
			}
		}

		return selected;
	}

	private static GUIContent IconContent(string name, string tooltip) {
		var content = EditorGUIUtility.IconContent(name, tooltip);
		content.tooltip = tooltip;
		return content;
	}
}