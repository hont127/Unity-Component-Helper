using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Hont
{
    public class ComponentHelperView : EditorWindow
    {
        static EditorWindow mWindowHandle;
        ComponentHelperCore mComponentTool = new ComponentHelperCore();
        Vector2 mScrollSign;


        [MenuItem("Tools/Hont/Utility/Component Helper %#o")]
        public static void ComponentEditorToolInit()
        {
            if (mWindowHandle != null)
            {
                mWindowHandle.Close();
                return;
            }
            mWindowHandle = EditorWindow.GetWindow(typeof(ComponentHelperView));
            mWindowHandle.minSize = new Vector2(480, 540);
        }

        void OnGUI()
        {
            ModeSelectDetection();

            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) return;

            EditorGUILayout.HelpBox("-Hold 'ctrl' you can change to Mode1'\t\n-Hold 'ctrl+alt' you can change to 'Mode2'", MessageType.Info);

            RefreshSelectList();
            FindParentModeOption();
            RefreshCopyList();
            RefreshOpComponentList();
        }

        void Update()
        {
            Repaint();
        }

        void ModeSelectDetection()
        {
            var curEvt = Event.current;
            if (curEvt.keyCode == KeyCode.LeftControl && curEvt.type == EventType.KeyDown)
            {
                mComponentTool.IsOrderMode = true;
                Repaint();
            }

            else if (curEvt.keyCode == KeyCode.LeftControl && curEvt.type == EventType.KeyUp)
            {
                mComponentTool.IsOrderMode = false;
                Repaint();
            }

            else if (curEvt.keyCode == KeyCode.LeftAlt && curEvt.type == EventType.KeyDown)
            {
                mComponentTool.IsAttachMode = true;
                Repaint();
            }

            else if (curEvt.keyCode == KeyCode.LeftAlt && curEvt.type == EventType.KeyUp)
            {
                mComponentTool.IsAttachMode = false;
                Repaint();
            }
        }

        void FindParentModeOption()
        {
            EditorGUILayout.BeginHorizontal();
            mComponentTool.IsIntelligentFindParent
                = GUILayout.Toggle(mComponentTool.IsIntelligentFindParent, "IntelligentFindParent");
            mComponentTool.IsIntelligentFindParentOnlyType
                = GUILayout.Toggle(mComponentTool.IsIntelligentFindParentOnlyType, "IntelligentFindParent(Only Type)");
            EditorGUILayout.EndHorizontal();
        }

        void RefreshOpComponentList()
        {
            var defaultGUIColor = GUI.color;
            mScrollSign = EditorGUILayout.BeginScrollView(mScrollSign);/*BeginScrollView*/

            #region ---SRC Object Components---
            if (mComponentTool.SrcObj != null)
            {
                GUILayout.Label(mComponentTool.SrcObjInfo, EditorStyles.boldLabel);

                var comps = mComponentTool.SrcObjComps;
                for (int i = 0; i < comps.Length; i++)
                {
                    var item = comps[i];
                    GUI.color = defaultGUIColor;

                    if (item == null)
                    {
                        ColorBlock(col: Color.red, blockContent: () => GUILayout.Box("Null(Script) Component"));
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();/*BeginHorizontal*/

                    if (item == mComponentTool.LastOpComponent) GUI.color = Color.Lerp(GUI.color, Color.red, 0.2f);

                    var isWillRemove = mComponentTool.WillRemoveList.Contains(item);

                    ColorBlock(
                        col: isWillRemove ? Color.red : GUI.color
                        , blockContent: () => ComponentNameBoxRender(item, isWillRemove ? "(WillRemove)" : ""));

                    HightLightRectRender(defaultGUIColor);

                    if (mComponentTool.IsOrderMode)
                    {
                        var delResult = OrderModeBtnGroupRender(item);
                        if (delResult) break;
                    }
                    else
                    {
                        if (GUILayout.Button("Cut", GUILayout.MaxWidth(100)))
                        {
                            if (item is Transform)
                            {
                                EditorUtility.DisplayDialog("Tip", "You Don't Destroy Transform!", "OK");
                                return;
                            }

                            mComponentTool.AddCutComponentToCopyList(item);
                        }

                        if (GUILayout.Button("To Copy List", GUILayout.MaxWidth(100)))
                        {
                            mComponentTool.AddComponentToCopyList(item);
                        }

                        if (GUILayout.Button("Duplicate", GUILayout.MaxWidth(100)))
                        {
                            if (item is Transform)
                            {
                                EditorUtility.DisplayDialog("Tip", "You Don't Destroy Transform!", "OK");
                                return;
                            }

                            if (mComponentTool.DstObj == null)
                            {
                                EditorUtility.DisplayDialog("Tip", "Please Select A Target!", "OK");
                                return;
                            }

                            mComponentTool.ComponentDuplicate(item);
                        }
                    }
                    EditorGUILayout.EndHorizontal();/*EndHorizontal*/
                }
                GUI.color = defaultGUIColor;
            }
            #endregion

            #region ---DST Object Components---
            if (mComponentTool.IsMultiDst)
            {
                GUILayout.Label("Dst(ID:--)", EditorStyles.boldLabel);
                GUILayout.Box("Multi Dst Mode");
            }
            else if (mComponentTool.DstObj != null)
            {
                GUILayout.Label(mComponentTool.DstObjInfo, EditorStyles.boldLabel);
                var comps = mComponentTool.DstObjComps;
                for (int i = 0; i < comps.Length; i++)
                {
                    var item = comps[i];

                    if (item == null)
                    {
                        ColorBlock(col: Color.red, blockContent: () => GUILayout.Box("Null(Script) Component"));
                        continue;
                    }

                    GUILayout.BeginHorizontal();/*BeginHorizontal*/

                    GUI.color = defaultGUIColor;
                    if (item == mComponentTool.LastOpComponent) GUI.color = Color.Lerp(GUI.color, Color.red, 0.2f);

                    ComponentNameBoxRender(item);

                    HightLightRectRender(defaultGUIColor);

                    if (mComponentTool.IsOrderMode)
                    {
                        var delResult = OrderModeBtnGroupRender(item);
                        if (delResult) return;
                    }
                    else
                    {
                        if (GUILayout.Button("Paste Value", GUILayout.MaxWidth(200)))
                        {
                            if (mComponentTool.CopySelList.Count > 1)
                            {
                                EditorUtility.DisplayDialog("Tip", "Please Change To Single Target!", "OK");
                                return;
                            }
                            else if (mComponentTool.CopySelList.Count < 1)
                            {
                                EditorUtility.DisplayDialog("Tip", "Please Select A Target!", "OK");
                                return;
                            }
                            else
                            {
                                mComponentTool.PasteValue(item);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();/*EndHorizontal*/
                }
                GUI.color = defaultGUIColor;
            }
            #endregion

            if (mComponentTool.CanPaste)
            {
                if (GUILayout.Button("\nPaste\n"))
                    ExecutePaste();
            }

            EditorGUILayout.EndScrollView();/*EndScrollView*/
        }

        void RefreshSelectList()
        {
            mComponentTool.RefreshSelectList();

            #region ---SRC---
            EditorGUILayout.BeginHorizontal();/*BeginHorizontal*/
            EditorGUILayout.LabelField("Source GameObject");

            CursorModeRender(targetModeDetectionValue: true);

            mComponentTool.SrcObj = EditorGUILayout.ObjectField(mComponentTool.SrcObj, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndHorizontal();/*EndHorizontal*/
            #endregion

            #region ---DST---
            EditorGUILayout.BeginHorizontal();/*BeginHorizontal*/
            if (mComponentTool.IsMultiDst)
                EditorGUILayout.LabelField(string.Format("Dst Multi({0})", mComponentTool.DstObjArr.Length));
            else
                EditorGUILayout.LabelField("Destination GameObject");

            CursorModeRender(targetModeDetectionValue: false);

            if (!mComponentTool.IsMultiDst)
                mComponentTool.DstObj = EditorGUILayout.ObjectField(mComponentTool.DstObj, typeof(GameObject), true) as GameObject;
            else
                GUILayout.Label("Multi Dst Mode");

            EditorGUILayout.EndHorizontal();/*EndHorizontal*/
            #endregion
        }

        void RefreshCopyList()
        {
            GUILayout.Label(string.Format("-Copy List({0}):", mComponentTool.CopyList.Count));

            foreach (var item in mComponentTool.CopyList)
            {
                EditorGUILayout.BeginHorizontal();/*BeginHorizontal*/
                var isContainItem = mComponentTool.CopySelList.Contains(item);

                bool isBreakFlag = false;
                ColorBlock(col: isContainItem ? Color.Lerp(GUI.color, Color.green, 0.5f) : GUI.color,
                    blockContent: () =>
                    {
                        var guiContent = EditorGUIUtility.ObjectContent(item, item.GetType());
                        guiContent.text = item.GetType().Name;
                        GUILayout.Box(guiContent
                            , EditorStyles.wordWrappedLabel
                            , GUILayout.MaxWidth(300)
                            , GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.2f));

                        if (GUILayout.Button(isContainItem ? "Unselect" : "Select"))
                        {
                            mComponentTool.SelectItemFromCopyList(comp: item, isSelect: isContainItem);
                            isBreakFlag = true;
                            return;
                        }

                        if (GUILayout.Button("Remove"))
                        {
                            mComponentTool.RemoveFromCopyList(item);
                            isBreakFlag = true;
                            return;
                        }
                    });
                EditorGUILayout.EndHorizontal();/*EndHorizontal*/

                if (isBreakFlag) break;
            }

            if (GUILayout.Button("Clear Copy List")) mComponentTool.ClearCopyList();
        }

        void ExecutePaste()
        {
            if (mComponentTool.CopySelList.Count <= 0)
            {
                EditorUtility.DisplayDialog("Tip", "Please Select From Copy List!", "OK");
                return;
            }

            if (mComponentTool.CopySelList.FirstOrDefault(m => m is Transform) != null)
            {
                EditorUtility.DisplayDialog("Tip", "Don't Paste Transform Type!", "OK");
                return;
            }

            try { mComponentTool.Paste(); }
            catch (Exception e) { EditorUtility.DisplayDialog("Error!", e.ToString(), "OK"); }
        }

        void ColorBlock(Color col, Action blockContent)
        {
            var tmp = GUI.color;
            GUI.color = col;
            blockContent();
            GUI.color = tmp;
        }

        #region ----Common Button Redener Methods----
        void CursorModeRender(bool targetModeDetectionValue)
        {
            ColorBlock(
                col: mComponentTool.SrcOrDstSelection.GetValueOrDefault(!targetModeDetectionValue) == targetModeDetectionValue ? Color.red : GUI.color
                , blockContent: () =>
                {
                    if (GUILayout.Button("Cursor Mode"))
                    {
                        if (mComponentTool.SrcOrDstSelection.GetValueOrDefault(!targetModeDetectionValue) == targetModeDetectionValue)
                            mComponentTool.SrcOrDstSelection = null;
                        else
                            mComponentTool.SrcOrDstSelection = targetModeDetectionValue;
                    }
                });
        }

        void HightLightRectRender(Color defaultGUIColor)
        {
            var lightRect = GUILayoutUtility.GetLastRect();
            lightRect.width = EditorGUIUtility.currentViewWidth;
            if (lightRect.Contains(Event.current.mousePosition))
            {
                GUI.color = Color.Lerp(defaultGUIColor, Color.red, 0.4f);
                Repaint();
            }
        }

        void ComponentNameBoxRender(Component comp, string nameAttachStr = "")
        {
            var compType = comp.GetType();
            var guiContent = EditorGUIUtility.ObjectContent(comp, compType);
            guiContent.text = compType.Name + nameAttachStr;
            GUILayout.Box(guiContent
                , EditorStyles.wordWrappedLabel
                , GUILayout.MaxWidth(300)
                , GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.2f));
        }

        /// <summary>
        /// return is delete success.
        /// </summary>
        bool OrderModeBtnGroupRender(Component comp)
        {
            if (mComponentTool.IsAttachMode)
            {
                ToTopBtnRender(comp);
                ToBottomBtnRender(comp);
            }
            else
            {
                UpBtnRender(comp);
                DownBtnRender(comp);
            }

            var delResult = DelBtnRender(comp);

            return delResult;
        }

        bool DelBtnRender(Component comp)
        {
            if (GUILayout.Button("Del", GUILayout.MaxWidth(100)))
            {
                mComponentTool.DeleteComponent(comp);
                return true;
            }

            return false;
        }

        void UpBtnRender(Component comp) { if (GUILayout.Button("Up", GUILayout.MaxWidth(100))) mComponentTool.ComponentToUp(comp); }

        void DownBtnRender(Component comp) { if (GUILayout.Button("Down", GUILayout.MaxWidth(100))) mComponentTool.ComponentToDown(comp); }

        void ToTopBtnRender(Component comp) { if (GUILayout.Button("To Top", GUILayout.MaxWidth(100))) mComponentTool.ComponentToTop(comp); }

        void ToBottomBtnRender(Component comp) { if (GUILayout.Button("To Bottom", GUILayout.MaxWidth(100))) mComponentTool.ComponentToBottom(comp); }
        #endregion
    }
}
