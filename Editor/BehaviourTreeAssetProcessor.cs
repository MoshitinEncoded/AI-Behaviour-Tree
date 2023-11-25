using MoshitinEncoded.AI.BehaviourTreeLib;
using MoshitinEncoded.GraphTools;
using UnityEditor;
using UnityEngine;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    public class BehaviourTreeAssetProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                if (!assetPath.EndsWith(".asset")) continue;
                var behaviourTree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(assetPath);
                if (!behaviourTree || behaviourTree.RootNode && behaviourTree.Blackboard)
                {
                    continue;
                }

                var serializedTree = new SerializedObject(behaviourTree);

                if (!behaviourTree.RootNode)
                {
                    var rootNode = behaviourTree.CreateNode(typeof(RootNode));
                    
                    AssetDatabase.AddObjectToAsset(rootNode, behaviourTree);

                    var serializedNode = new SerializedObject(rootNode);
                    serializedNode.FindProperty("_Guid").stringValue = GUID.Generate().ToString();
                    serializedNode.FindProperty("_Position").vector2Value = Vector2.zero;
                    serializedNode.FindProperty("_Title").stringValue = "Root";
                    serializedNode.ApplyModifiedPropertiesWithoutUndo();

                    serializedTree.Update();
                    serializedTree.FindProperty("_RootNode").objectReferenceValue = rootNode;
                    serializedTree.FindProperty("_Nodes").AddToObjectArray(rootNode);
                    serializedTree.ApplyModifiedPropertiesWithoutUndo();
                }

                if (!behaviourTree.Blackboard)
                {
                    var blackboard = ScriptableObject.CreateInstance<Blackboard>();
                    blackboard.name = "Blackboard";
                    blackboard.hideFlags = HideFlags.HideInHierarchy;

                    AssetDatabase.AddObjectToAsset(blackboard, behaviourTree);

                    serializedTree.Update();
                    serializedTree.FindProperty("_Blackboard").objectReferenceValue = blackboard;
                    serializedTree.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }
}
