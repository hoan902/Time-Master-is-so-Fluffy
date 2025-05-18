using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;
using Event = UnityEngine.Event;

#if UNITY_EDITOR

namespace SpineTool.Editor
{
    public class SpineToolEditor : EditorWindow
    {
        public Skeleton curSkeleton;
        public SkeletonAnimation spine;
        public SkeletonGraphic uiSpine;
        public AnimationState currentState;
        public bool isAutoRun;
        public bool isNeedReload;
        public float autoRunSpeed = 1f;
        float animationLastTime;
        public Animation[] animations;
        public Skin[] skins;

        static float CurrentTime => (float)EditorApplication.timeSinceStartup;

        [MenuItem("Toos/Spine Tool")]
        static void ShowWindow()
        {
            var editor = GetWindow(typeof(SpineToolEditor));
            editor.titleContent.text = "Spine Tool";
            editor.Show();
        }

        private Vector2 scrollPosition;
        private Vector2 skinPosition;
        private Animation currentAnim;
        private List<Skin> currentSkin = new();
        public GameObject obj;
        private float playTime;
        private string searchAnimation = string.Empty;
        private string searchAnimationOld = string.Empty;
        private string searchSkin = string.Empty;
        private string searchSkinOld = string.Empty;
        private float animPanelWidth = 300;
        private Rect _cursorChangeRect;
        private bool _isResize;
        private SkeletonDataAsset _currentSkeletonDataAsset;
        private const string toolTip1 = "Hole Ctrl + click to copy animation name";
        private const string toolTip2 = "Hole Ctrl + click to copy skin name";

        private void OnEnable()
        {
            EditorApplication.update -= HandleEditorUpdate;
            EditorApplication.update += HandleEditorUpdate;
            Selection.selectionChanged += SelectionChanged;
            // EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            _cursorChangeRect = new Rect(animPanelWidth, 0, 5, position.size.y);
            spine = null;
            uiSpine = null;
        }

        private void SelectionChanged()
        {
            UpdateSpine();
            Repaint();
        }

        private void HierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
        {
            UpdateSpine();
            Repaint();
        }

        private void OnDisable()
        {
            EditorApplication.update -= HandleEditorUpdate;
            Selection.selectionChanged -= SelectionChanged;
            // EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
        }

        void HandleEditorUpdate()
        {
            if (currentAnim != null && currentState != null && currentState.GetCurrent(0) != null)
            {
                if (spine != null && _currentSkeletonDataAsset != spine.skeletonDataAsset)
                {
                    _currentSkeletonDataAsset = spine.skeletonDataAsset;
                    spine.initialSkinName = "default";
                    spine.Initialize(true);
                    uiSpine = null;
                    UpdateSpine();
                }
                else if (uiSpine != null && _currentSkeletonDataAsset != uiSpine.skeletonDataAsset)
                {
                    _currentSkeletonDataAsset = uiSpine.skeletonDataAsset;
                    uiSpine.initialSkinName = "default";
                    uiSpine.Initialize(true);
                    uiSpine = null;
                    UpdateSpine();
                }

                if (isAutoRun)
                {
                    float deltaTime = CurrentTime - animationLastTime;
                    currentState.Update(deltaTime);
                    animationLastTime = CurrentTime;
                    currentState.TimeScale = autoRunSpeed;
                    var currentTrack = currentState.GetCurrent(0);
                    if (currentTrack != null && currentTrack.TrackTime >= currentAnim.Duration)
                    {
                        currentTrack.TrackTime = 0;
                    }

                    if (spine != null)
                    {
                        spine.LateUpdate();
                        spine.Skeleton.UpdateWorldTransform();
                    }
                    else if (uiSpine != null)
                    {
                        uiSpine.LateUpdate();
                        uiSpine.Skeleton.UpdateWorldTransform();
                        uiSpine.UpdateMesh();
                    }

                    if (currentTrack != null)
                        playTime = currentTrack.TrackTime;
                    Repaint();
                }
                else
                {
                    animationLastTime = CurrentTime;
                    currentState.TimeScale = autoRunSpeed;
                    var currentTrack = currentState.GetCurrent(0);
                    if (currentTrack != null)
                        currentTrack.TrackTime = playTime;
                    currentState.Update(0);

                    if (spine != null)
                    {
                        spine.LateUpdate();
                        spine.Skeleton.UpdateWorldTransform();
                    }
                    else if (uiSpine != null)
                    {
                        uiSpine.LateUpdate();
                        uiSpine.Skeleton.UpdateWorldTransform();
                        uiSpine.UpdateMesh();
                    }
                }
            }
        }

        private bool check;

        void OnGUI()
        {
            if (Application.isPlaying) return;

            // GUILayout.BeginHorizontal(SpineToolStyle.BgPopup2);
            //
            // GUILayout.EndHorizontal();
            if (curSkeleton != null)
            {
                GUILayout.BeginVertical(SpineToolStyle.BgPopup);
                {
                    GUILayout.BeginHorizontal();
                    isAutoRun = GUILayout.Toggle(isAutoRun, "Auto Run", GUILayout.ExpandWidth(false));
                    SpineToolStyle.focusOnScene.text = spine == null ? uiSpine.name : spine.name;
                    if (GUILayout.Button(SpineToolStyle.focusOnScene, GUILayout.ExpandWidth(false),
                            GUILayout.ExpandHeight(false)))
                    {
                        Selection.activeGameObject = obj;
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }

                    if (spine!=null)
                    {
                        var skeleton =
                            (SkeletonDataAsset)EditorGUILayout.ObjectField("", spine.skeletonDataAsset,
                                typeof(SkeletonDataAsset), true);
                        if (skeleton != spine.skeletonDataAsset)
                        {
                            spine.skeletonDataAsset = skeleton;
                        }    
                    }
                    else
                    {
                        var skeleton =
                            (SkeletonDataAsset)EditorGUILayout.ObjectField("", uiSpine.skeletonDataAsset,
                                typeof(SkeletonDataAsset), true);
                        if (skeleton != uiSpine.skeletonDataAsset)
                        {
                            uiSpine.skeletonDataAsset = skeleton;
                        }    
                    }
                    

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Frame: " + (int)(playTime * 30), GUILayout.ExpandWidth(false));
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            if (isAutoRun)
                            {
                                GUILayout.Label("Speed");
                                autoRunSpeed = EditorGUILayout.Slider(autoRunSpeed, 0, 1.5f);
                                currentState.TimeScale = autoRunSpeed;
                            }

                            if (currentAnim != null)
                            {
                                playTime = EditorGUILayout.Slider(playTime, 0, currentAnim.Duration);
                            }
                            else
                            {
                                EditorGUILayout.Slider(playTime, 0, 0);
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                ViewAnimations();
                ViewSkins();
                GUILayout.EndHorizontal();

                if (isNeedReload)
                {
                    isNeedReload = false;
                    if (spine != null)
                    {
                        spine.ClearState();
                        if (currentAnim != null)
                            spine.AnimationName = currentAnim.Name;
                        spine.initialSkinName = spine.skeleton.Data.Skins.Items[0].Name;
                        spine.LateUpdate();
                        spine.Initialize(true);
                        currentState = spine.state;
                        currentState.TimeScale = autoRunSpeed;
                        spine.gameObject.SetActive(false);
                        spine.gameObject.SetActive(true);
                        ChangeSkin();
                    }
                    else if (uiSpine != null)
                    {
                        uiSpine.Clear();
                        if (currentAnim != null)
                            uiSpine.startingAnimation = currentAnim.Name;
                        uiSpine.AnimationState.SetAnimation(0, currentAnim.Name, false);
                        uiSpine.LateUpdate();
                        uiSpine.Initialize(true);
                        currentState.TimeScale = autoRunSpeed;
                        uiSpine.gameObject.SetActive(false);
                        uiSpine.gameObject.SetActive(true);
                        currentState = uiSpine.AnimationState;
                        ChangeSkin();
                    }

                    Repaint();
                }
            }
            else
            {
                GUILayout.Label("Please select a skeleton!", EditorStyles.helpBox);
            }
        }

        private void ViewAnimations()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(animPanelWidth));
            searchAnimation = GUILayout.TextField(searchAnimation, SpineToolStyle.SearchBoxStyle);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                int count = 0;

                if (searchAnimation != searchAnimationOld)
                {
                    searchAnimation = searchAnimationOld;
                    animations = curSkeleton.Data.Animations.Items
                        .Where(s => s.Name.ToLower().Contains(searchAnimation)).ToArray();
                }

                foreach (var animation in animations)
                {
                    if (animation == currentAnim)
                    {
                        GUILayout.BeginHorizontal(SpineToolStyle.AnimSelected);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                    }

                    GUILayout.Label(new GUIContent(SpineEditorUtilities.Icons.animation, toolTip1),
                        GUILayout.ExpandWidth(false));
                    GUILayout.Label($"{animation.Name}", SpineToolStyle.AnimName, GUILayout.Height(24));
                    GUILayout.Label($"{animation.Duration}", SpineToolStyle.AnimTime, GUILayout.ExpandWidth(false),
                        GUILayout.Height(24));
                    GUILayout.EndHorizontal();
                    var lastRect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                    {
                        currentAnim = animation;
                        isNeedReload = true;
                        if (Event.current.control)
                        {
                            EditorGUIUtility.systemCopyBuffer = animation.Name;
                        }
                    }

                    Repaint();
                    count++;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            ResizeScrollView();
        }

        private void ViewSkins()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            searchSkin = GUILayout.TextField(searchSkin, SpineToolStyle.SearchBoxStyle);
            skinPosition = GUILayout.BeginScrollView(skinPosition);
            {
                int count = 0;

                if (searchSkin != searchSkinOld)
                {
                    searchSkinOld = searchSkin;
                    skins = curSkeleton.Data.Skins.Items.Where(s => s.Name.ToLower().Contains(searchSkin)).ToArray();
                }

                foreach (var skin in skins)
                {
                    if (currentSkin.Contains(skin))
                    {
                        GUILayout.BeginHorizontal(SpineToolStyle.AnimSelected);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                    }

                    GUILayout.Label(new GUIContent(SpineEditorUtilities.Icons.skin, toolTip2),
                        GUILayout.ExpandWidth(false));
                    GUILayout.Label($"{skin.Name}", SpineToolStyle.AnimName, GUILayout.Height(24));
                    GUILayout.EndHorizontal();
                    var lastRect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                    {
                        if (!currentSkin.Contains(skin))
                        {
                            currentSkin.Add(skin);
                        }
                        else
                        {
                            currentSkin.Remove(skin);
                        }

                        ChangeSkin();
                        if (Event.current.control)
                        {
                            EditorGUIUtility.systemCopyBuffer = skin.Name;
                        }

                        Repaint();
                    }

                    count++;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void UpdateSpine()
        {
            if (Selection.activeGameObject != null)
            {
                var tempSpine = Selection.activeGameObject.GetComponent<SkeletonAnimation>();
                if (tempSpine != null)
                {
                    currentSkin.Clear();
                    spine = tempSpine;
                    uiSpine = null;
                    curSkeleton = spine.Skeleton;
                    obj = spine.gameObject;
                    UpdateViewList();
                    isNeedReload = true;
                    currentAnim = spine.skeleton.Data.Animations.Items[0];
                    spine.initialSkinName = spine.skeletonDataAsset.GetSkeletonData(true).Skins.Items[0].Name;
                    spine.skeleton.SetSkin(spine.initialSkinName);
                    currentState = spine.state;
                }
                else
                {
                    var tempUiSpine = Selection.activeGameObject.GetComponent<SkeletonGraphic>();
                    if (tempUiSpine != null && uiSpine != tempUiSpine)
                    {
                        currentSkin.Clear();
                        uiSpine = tempUiSpine;
                        spine = null;
                        curSkeleton = uiSpine.Skeleton;
                        obj = uiSpine.gameObject;
                        UpdateViewList();
                        isNeedReload = true;
                        currentAnim = uiSpine.Skeleton.Data.Animations.Items[0];
                        uiSpine.initialSkinName = uiSpine.skeletonDataAsset.GetSkeletonData(true).Skins.Items[0].Name;
                        uiSpine.Skeleton.SetSkin(uiSpine.initialSkinName);
                        currentState = uiSpine.AnimationState;
                    }
                }
            }
        }

        private void ChangeSkin()
        {
            if (currentSkin.Count <= 0)
            {
                return;
            }

            Skin skin = new Skin("run");
            foreach (var skin1 in currentSkin)
            {
                skin.AddSkin(skin1);
            }

            if (spine != null)
            {
                spine.initialSkinName = "run";
                spine.skeleton.SetSkin(skin);
                spine.skeleton.SetSlotsToSetupPose();
                spine.LateUpdate();
                spine.AnimationState.Apply(spine.Skeleton);
            }
            else if (uiSpine != null)
            {
                uiSpine.Skeleton.SetSkin(skin);
                uiSpine.Skeleton.SetSlotsToSetupPose();
                uiSpine.LateUpdate();
            }
        }

        private void UpdateViewList()
        {
            animations = curSkeleton.Data.Animations.Items;
            skins = curSkeleton.Data.Skins.Items;
        }

        private void ResizeScrollView()
        {
            EditorGUIUtility.AddCursorRect(_cursorChangeRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && _cursorChangeRect.Contains(Event.current.mousePosition))
            {
                _isResize = true;
            }

            if (_isResize)
            {
                animPanelWidth = Event.current.mousePosition.x;
                animPanelWidth = Mathf.Clamp(animPanelWidth, 100, position.width - 100);
                _cursorChangeRect.Set(animPanelWidth, _cursorChangeRect.y, _cursorChangeRect.width,
                    _cursorChangeRect.height);
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
                _isResize = false;
        }
    }
}

#endif