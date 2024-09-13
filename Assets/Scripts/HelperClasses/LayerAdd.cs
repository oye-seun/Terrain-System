using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace HelperClasses
{
    public class LayerAdd
    {

        public static int AddLayerAt(int index, string layerName, bool debugMode, bool tryOtherIndex = true)
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = serializedObject.FindProperty("layers");
            int layer = AddLayerAt(layers, index, layerName, debugMode, tryOtherIndex);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            return layer;
        }
        public static int AddLayerAt(SerializedProperty layers, int index, string layerName, bool debugMode, bool tryOtherIndex = true)
        {
            // Skip if a layer with the name already exists.
            for (int i = 0; i < layers.arraySize; ++i)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    if (debugMode) Debug.Log("Skipping layer '" + layerName + "' because it already exists.");
                    return i;
                }
            }

            // Extend layers if necessary
            if (index >= layers.arraySize && index < 32)
                layers.arraySize = index + 1;

            // set layer name at index
            var element = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(element.stringValue))
            {
                element.stringValue = layerName;
                if (debugMode) Debug.Log("Added layer '" + layerName + "' at index " + index + ".");
                return index;
            }
            else
            {
                if (debugMode) Debug.LogWarning("Could not add layer at index " + index + " because there already is another layer '" + element.stringValue + "'.");

                if (tryOtherIndex)
                {
                    // Go up in layer indices and try to find an empty spot.
                    for (int i = index + 1; i < 32; ++i)
                    {
                        // Extend layers if necessary
                        if (i >= layers.arraySize)
                            layers.arraySize = i + 1;

                        element = layers.GetArrayElementAtIndex(i);
                        if (string.IsNullOrEmpty(element.stringValue))
                        {
                            element.stringValue = layerName;
                            if (debugMode) Debug.Log("Added layer '" + layerName + "' at index " + i + " instead of " + index + ".");
                            return i;
                        }
                    }

                    if (debugMode) Debug.LogError("Could not add layer " + layerName + " because there is no space left in the layers array.");
                }
            }

            return -1;
        }
    }
}