using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Hont
{
    public class ComponentHelperCore
    {
        Component mLastOpComp;
        GameObject mSrcObj;
        GameObject mDstObj;
        GameObject[] mDstObjArr;
        HashSet<Component> mWillRemoveList = new HashSet<Component>();
        HashSet<Component> mWaitCopyList = new HashSet<Component>();
        HashSet<Component> mCopySelList = new HashSet<Component>();
        bool mIsOrderMode;
        bool mIsAttachMode;
        bool mIsIntelligentFindParent;
        bool mIsIntelligentFindParentOnlyType;
        bool? mSrcOrDstSelection = true;

        public Component LastOpComponent { get { return mLastOpComp; } }

        public HashSet<Component> CopyList { get { return mWaitCopyList; } }
        public HashSet<Component> CopySelList { get { return mCopySelList; } }
        public HashSet<Component> WillRemoveList { get { return mWillRemoveList; } }

        public bool CanPaste
        {
            get
            {
                bool flag = false;
                flag = mDstObj != null || mDstObjArr != null;
                flag &= mSrcObj != null;

                return flag;
            }
        }

        public bool IsOrderMode { get { return mIsOrderMode; } set { mIsOrderMode = value; } }
        public bool IsAttachMode { get { return mIsAttachMode; } set { mIsAttachMode = value; } }

        public string SrcObjInfo { get { return string.Format("-Source(Name:{0}  ID:{1})", mSrcObj.name, mSrcObj.GetInstanceID()); } }
        public string DstObjInfo { get { return string.Format("-Dst(Name:{0}  ID:{1})", mDstObj.name, mDstObj.GetInstanceID()); } }

        public bool IsIntelligentFindParent
        {
            get
            { return mIsIntelligentFindParent; }
            set
            {
                mIsIntelligentFindParent = value;
                if (mIsIntelligentFindParent) mIsIntelligentFindParentOnlyType = false;
            }
        }
        public bool IsIntelligentFindParentOnlyType
        {
            get { return mIsIntelligentFindParentOnlyType; }
            set
            {
                mIsIntelligentFindParentOnlyType = value;
                if (mIsIntelligentFindParentOnlyType) mIsIntelligentFindParent = false;
            }
        }

        public GameObject SrcObj { get { return mSrcObj; } set { mSrcObj = value; } }
        public GameObject DstObj { get { return mDstObj; } set { mDstObj = value; } }
        public GameObject[] DstObjArr { get { return mDstObjArr; } set { mDstObjArr = value; } }

        public Component[] SrcObjComps { get { return mSrcObj.GetComponents<Component>(); } }
        public Component[] DstObjComps { get { return mDstObj.GetComponents<Component>(); } }

        public bool? SrcOrDstSelection { get { return mSrcOrDstSelection; } set { mSrcOrDstSelection = value; } }
        public bool IsMultiDst { get { return mDstObjArr != null && mDstObjArr.Length > 0; } }


        public void RefreshSelectList()
        {
            if (mSrcOrDstSelection.HasValue == false)
            {
                mDstObjArr = null;
            }
            else if (mSrcOrDstSelection.Value)
            {
                mSrcObj = Selection.activeGameObject;
            }
            else
            {
                mDstObj = Selection.activeGameObject;
                mDstObjArr = Selection.gameObjects;
                if (mDstObjArr.Length == 1) mDstObjArr = null;
            }
        }

        public void AddToWillRemoveList(Component comp)
        {
            mWillRemoveList.Add(comp);
            mLastOpComp = comp;
        }

        public void RemoveFromCopyList(Component comp)
        {
            mWaitCopyList.Remove(comp);

            if (mWillRemoveList.Contains(comp))
                mWillRemoveList.Remove(comp);

            if (mCopySelList.Contains(comp))
                mCopySelList.Remove(comp);
        }

        public void SelectItemFromCopyList(Component comp, bool isSelect)
        {
            if (mCopySelList.Contains(comp)) mCopySelList.Remove(comp);
            else mCopySelList.Add(comp);
        }

        public void ClearCopyList()
        {
            mWillRemoveList.Clear();
            mWaitCopyList.Clear();
            mCopySelList.Clear();
        }

        public void ReverseCopyList()
        {
            var temp = mCopySelList.Reverse().ToArray();
            mCopySelList.Clear();
            foreach (var item in temp)
                mCopySelList.Add(item);

            temp = mWillRemoveList.Reverse().ToArray();
            mWillRemoveList.Clear();
            foreach (var item in temp)
                mWillRemoveList.Add(item);

            temp = mWaitCopyList.Reverse().ToArray();
            mWaitCopyList.Clear();
            foreach (var item in temp)
                mWaitCopyList.Add(item);
        }

        #region Component Operation Methods.

        public void ComponentToTop(Component comp)
        {
            var go = comp.gameObject;

#if !(UNITY_2018_3_OR_NEWER)
            if (!AssetDatabase.IsMainAsset(go))
                PrefabUtility.DisconnectPrefabInstance(go);
#endif

            for (int j = 0; j < go.GetComponents<Component>().Length; j++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(comp);
            }
            mLastOpComp = comp;
        }

        public void ComponentToBottom(Component comp)
        {
            var go = comp.gameObject;

#if !(UNITY_2018_3_OR_NEWER)
            if (!AssetDatabase.IsMainAsset(go))
                PrefabUtility.DisconnectPrefabInstance(go);
#endif

            for (int j = 0; j < go.GetComponents<Component>().Length; j++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(comp);
            }
            mLastOpComp = comp;
        }

        public void ComponentToUp(Component comp)
        {
            var go = comp.gameObject;

#if !(UNITY_2018_3_OR_NEWER)
            if (!AssetDatabase.IsMainAsset(go))
                PrefabUtility.DisconnectPrefabInstance(go);
#endif

            UnityEditorInternal.ComponentUtility.MoveComponentUp(comp);
            mLastOpComp = comp;
        }

        public void ComponentToDown(Component comp)
        {
            var go = comp.gameObject;

#if !(UNITY_2018_3_OR_NEWER)
            if (!AssetDatabase.IsMainAsset(go))
                PrefabUtility.DisconnectPrefabInstance(go);
#endif

            UnityEditorInternal.ComponentUtility.MoveComponentDown(comp);
            mLastOpComp = comp;
        }

        public void DeleteComponent(Component comp)
        {
            if (mWillRemoveList.Contains(comp))
                mWillRemoveList.Remove(comp);

            if (mWaitCopyList.Contains(comp))
                mWaitCopyList.Remove(comp);

            if (mCopySelList.Contains(comp))
                mCopySelList.Remove(comp);

            Undo.DestroyObjectImmediate(comp);
        }

        public void AddCutComponentToCopyList(Component comp)
        {
            mWillRemoveList.Add(comp);
            mWaitCopyList.Add(comp);
            mCopySelList.Add(comp);
            mLastOpComp = comp;
        }

        public void AddComponentToCopyList(Component comp)
        {
            mWaitCopyList.Add(comp);
            mCopySelList.Add(comp);
            mLastOpComp = comp;
        }

        public void ComponentDuplicate(Component comp)
        {
            mWaitCopyList.Add(comp);
            mCopySelList.Add(comp);
            Paste();
            mWaitCopyList.Remove(comp);
            mCopySelList.Remove(comp);
            mLastOpComp = comp;
        }
        #endregion

        public void Paste()
        {
            foreach (var item in mCopySelList)
            {
                if (!IsMultiDst)
                {
                    Paste(item, mDstObj);
                }
                else//multiple copy target state.
                {
                    foreach (var dstObj in mDstObjArr)
                    {
                        Paste(item, dstObj);
                    }
                }
            }

            if (mSrcObj != null)
            {
                foreach (var item in mWillRemoveList)
                {
                    if (mCopySelList.Contains(item))
                        mCopySelList.Remove(item);

                    if (mWaitCopyList.Contains(item))
                        mWaitCopyList.Remove(item);

                    Undo.DestroyObjectImmediate(item);
                }
                mWillRemoveList.Clear();
            }
        }

        public void PasteValue(Component comp)
        {
            if (mCopySelList.Count == 1)
            {
                EditorUtility.CopySerialized(mCopySelList.FirstOrDefault(), comp);

                if (mSrcObj != null)
                {
                    foreach (var item in mWillRemoveList)
                    {
                        Undo.DestroyObjectImmediate(item);
                    }
                    mWillRemoveList.Clear();
                }
            }
        }

        public void Paste(Component comp, GameObject dstObj)
        {
            var newComp = Undo.AddComponent(dstObj, comp.GetType());
            if (newComp == null) return;

            EditorUtility.CopySerialized(comp, newComp);

            if (mIsIntelligentFindParent || mIsIntelligentFindParentOnlyType)
                ComponentEditorHierHelper.ChangeUnityObjectHierLink(newComp, mIsIntelligentFindParentOnlyType);
        }
    }
}
