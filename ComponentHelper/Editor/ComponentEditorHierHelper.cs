using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using System;

namespace Hont
{
    public static class ComponentEditorHierHelper
    {
        public static void ChangeUnityObjectHierLink(Component targetComp, bool justCompareType = false)
        {
            HashSet<UnityEngine.Object> saerchCompList = new HashSet<UnityEngine.Object>();
            ParentForEach(targetComp.transform
                , m =>
                {
                    foreach (var item in m.GetComponents<Component>())
                        saerchCompList.Add(item);

                    saerchCompList.Add(m.gameObject);
                }
                , true);

            ChildForEach(targetComp.transform
                , m =>
                {
                    foreach (var item in m.GetComponents<Component>())
                        saerchCompList.Add(item);

                    saerchCompList.Add(m.gameObject);
                }, true);

            var fields = targetComp.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var item in fields)
            {
                var value = item.GetValue(targetComp) as UnityEngine.Object;
                if (value == null) continue;
                var target = saerchCompList.FirstOrDefault(m =>
                {
                    bool result = false;
                    result = justCompareType && m.GetType() == value.GetType();
                    result |= m.name == value.name && m.GetType() == value.GetType();
                    return result;
                });

                item.SetValue(targetComp, target);
            }
        }

        public static void ChildForEach(Transform tfm, Action<Transform> action, bool hasSelf = false)
        {
            if (hasSelf) action(tfm);
            RecursiveChildTfm(tfm, action);
        }

        static void RecursiveChildTfm(Transform tfm, Action<Transform> action)
        {
            foreach (Transform item in tfm)
            {
                if (action != null) action(item);

                if (item.childCount > 0)
                {
                    RecursiveChildTfm(item, action);
                }
            }
        }

        static void ParentForEach(Transform tfm, Action<Transform> action, bool hasSelf = false)
        {
            if (hasSelf) action(tfm);
            Transform tmpTfm = tfm;
            while (tmpTfm != null)
            {
                action(tmpTfm);
                tmpTfm = tmpTfm.parent;
            }
        }
    }
}
