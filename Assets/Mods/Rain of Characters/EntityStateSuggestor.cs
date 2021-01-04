using EntityStates;
using PassivePicasso.ThunderKit.Deploy.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SEST = EntityStates.SerializableEntityStateType;

[CustomPropertyDrawer(typeof(SEST))]
public class EntityStateSuggestor : PropertyDrawer
{
    //static SearchSuggest<SEST> _suggestor;
    //static SearchSuggest<SEST> suggestor
    //{
    //    get
    //    {
    //        if (!_suggestor)
    //        {
    //            _suggestor = ScriptableObject.CreateInstance<SearchSuggest<SEST>>();
    //            if (suggestor)
    //            {
    //                suggestor.OnSuggestionGUI = RenderOption;
    //                suggestor.Evaluate = UpdateSearch;
    //            }
    //        }
    //        return _suggestor;
    //    }
    //}

    static List<SEST> _entityStates;
    static List<SEST> entityStates
    {
        get
        {
            if (_entityStates == null)
            {
                _entityStates = new List<SEST>();
                foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in ass.GetTypes())
                    {
                        if (typeof(EntityState).IsAssignableFrom(type))
                        {
                            var sest = new SEST(type);
                            if (sest.stateType != null)
                                entityStates.Add(new SEST(type));
                        }
                    }
                }
            }
            return _entityStates;
        }
    }

    //private static IEnumerable<SEST> UpdateSearch(string searchString) => entityStates.Where(s => s.stateType.FullName.Contains(searchString));

    string searchText = "";
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) //TODO: Add search box and pretty it up
    {
        EditorGUI.BeginProperty(new Rect(position.x, position.y, position.width, GetPropertyHeight(property, label)), label, property);

        EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), label);

        var currentSearch = EditorGUI.TextField(new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), searchText != "" ? searchText : "Search");
        if (currentSearch != "Search") searchText = currentSearch;

        if (EditorGUI.DropdownButton(new Rect(position.x + EditorGUIUtility.labelWidth, position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight, position.width - EditorGUIUtility.labelWidth, position.height), new GUIContent(property.FindPropertyRelative("_typeName").stringValue), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            var filteredStates = entityStates.Where(state => state.stateType.FullName.ToLower().Contains(searchText.ToLower())).GroupBy(state => state.stateType.Namespace);
            foreach (var group in filteredStates)
            {
                foreach (var serType in group)
                {
                    menu.AddItem(
                        new GUIContent(serType.stateType.Namespace + "/" + serType.stateType.FullName),
                        false,
                        (data) => 
                        {
                            var ser = data as SerializedProperty;
                            ser.FindPropertyRelative("_typeName").stringValue = serType.stateType.AssemblyQualifiedName;
                            ser.serializedObject.ApplyModifiedProperties();
                        }, 
                        property
                    );
                }
            }

            if (filteredStates.Count() == 0)
                menu.AddItem(new GUIContent("No items found for search"), false, null);

            menu.DropDown(new Rect(position.x + EditorGUIUtility.labelWidth, position.y + EditorGUIUtility.standardVerticalSpacing + (EditorGUIUtility.singleLineHeight * 2), position.width - EditorGUIUtility.labelWidth, 0));
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (2 * EditorGUIUtility.singleLineHeight) + EditorGUIUtility.standardVerticalSpacing;
    }

    //static bool RenderOption(int index, SEST state)
    //{
    //    if (GUILayout.Button(ObjectNames.NicifyVariableName(state.stateType.FullName)))
    //    {
    //        //AssetDatabase.AddObjectToAsset(state, target);

    //        //var stepField = runSteps.GetArrayElementAtIndex(runSteps.arraySize++);

    //        //stepField.objectReferenceValue = stepInstance;
    //        //stepField.serializedObject.SetIsDifferentCacheDirty();
    //        //stepField.serializedObject.ApplyModifiedProperties();

    //        //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stepInstance));
    //        //AssetDatabase.SaveAssets();
    //        //suggestor.Cleanup();
    //        return true;
    //    }
    //    return false;
    //}
}
