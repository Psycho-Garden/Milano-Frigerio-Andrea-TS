using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System;
using System.Linq;
using static UnityEngine.ParticleSystem;

namespace AF.TS.Weapons
{
    public class MultiTypeDrawer : OdinValueDrawer<MultiTypeValue>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var attr = Property.Attributes.GetAttribute<MultiTypeAttribute>();
            if (attr == null || attr.AllowedTypes.Length == 0)
            {
                CallNextDrawer(label);
                return;
            }

            var value = ValueEntry.SmartValue;
            var currentType = GetTypeFromMode((ValueMode)value.selectedIndex);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Campo
                DrawDynamicField(currentType, ref value, label);

                // Dropdown
                if (GUILayout.Button("˅", GUILayout.Width(20)))
                {
                    var menu = new GenericMenu();
                    var validModes = GetFilteredModes(attr.AllowedTypes);
                    foreach (var mode in validModes)
                    {
                        int index = (int)mode;
                        menu.AddItem(new GUIContent(mode.ToString()), index == value.selectedIndex, () =>
                        {
                            value.selectedIndex = index;
                        });
                    }
                    menu.ShowAsContext();
                }
            }

            ValueEntry.SmartValue = value;
        }

        private void DrawDynamicField(Type type, ref MultiTypeValue value, GUIContent label)
        {
            if (type == typeof(float))
                value.FloatValue = EditorGUILayout.FloatField(label, value.FloatValue);
            else if (type == typeof(Vector2))
                value.Vector2Value = EditorGUILayout.Vector2Field(label.text, value.Vector2Value);
            else if (type == typeof(AnimationCurve))
                value.CurveValue = EditorGUILayout.CurveField(label, value.CurveValue);
            else if (type == typeof(MinMaxCurve))
                value.MinMaxCurveValue = DrawMinMaxCurve(label, value.MinMaxCurveValue);
            else
                EditorGUILayout.LabelField(label.text, $"Unsupported Type: {type.Name}");
        }

        private MinMaxCurve DrawMinMaxCurve(GUIContent label, MinMaxCurve curve)
        {
            var holder = ScriptableObject.CreateInstance<MinMaxCurveHolder>();
            holder.curve = curve;

            SerializedObject so = new SerializedObject(holder);
            SerializedProperty prop = so.FindProperty("curve");

            EditorGUILayout.PropertyField(prop, label, true);
            so.ApplyModifiedProperties();

            var result = holder.curve;
            UnityEngine.Object.DestroyImmediate(holder);

            return result;
        }

        private Type GetTypeFromMode(ValueMode mode) => mode switch
        {
            ValueMode.Constant => typeof(float),
            ValueMode.RandomBetweenTwoConstants => typeof(Vector2),
            ValueMode.Curve => typeof(AnimationCurve),
            ValueMode.RandomBetweenTwoCurves => typeof(MinMaxCurve),
            _ => typeof(object)
        };

        private ValueMode[] GetFilteredModes(Type[] allowedTypes)
        {
            return Enum.GetValues(typeof(ValueMode))
                .Cast<ValueMode>()
                .Where(mode => allowedTypes.Contains(GetTypeFromMode(mode)))
                .ToArray();
        }

        [Serializable]
        public class MinMaxCurveHolder : ScriptableObject
        {
            public MinMaxCurve curve;
        }
    }
}
