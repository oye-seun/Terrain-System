//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//[InitializeOnLoad]
//public class ObjectChangeMonitor : MonoBehaviour
//{
//    static ObjectChangeMonitor()
//    {
//        ObjectChangeEvents.changesPublished += ChangesPublished;
//    }

//    static void ChangesPublished(ref ObjectChangeEventStream stream)
//    {
//        for (int i = 0; i < stream.length; ++i)
//        {
//            var type = stream.GetEventType(i);
//            switch (type)
//            {
//                //case ObjectChangeKind.ChangeScene:
//                //    stream.GetChangeSceneEvent(i, out var changeSceneEvent);
//                //    Debug.Log($"{type}: {changeSceneEvent.scene}");
//                //    break;

//                case ObjectChangeKind.CreateGameObjectHierarchy:
//                    stream.GetCreateGameObjectHierarchyEvent(i, out var createGameObjectHierarchyEvent);
//                    var newGameObject = EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
//                    Debug.Log($"{type}: {newGameObject} in scene {createGameObjectHierarchyEvent.scene}.");
//                    TerrainParent tp;
//                    if(newGameObject.TryGetComponent<TerrainParent>(out tp))
//                    {
//                        tp.SetTerrain();
//                    }
//                    break;
//            }
//        }
//    }
//}
