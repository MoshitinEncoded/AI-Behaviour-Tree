using System;

using MoshitinEncoded.AI.BehaviourTreeLib;
using MoshitinEncoded.GraphTools;

using UnityEditor;

using UnityEngine;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    public class BehaviourTreeAssetProcessor : AssetPostprocessor
    {
        public static event Action AssetDeleted;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                ProcessImportedAsset(assetPath);
            }

            foreach (var assetPath in deletedAssets)
            {
                ProcessDeletedAsset(assetPath);
            }
        }

        private static void ProcessImportedAsset(string assetPath)
        {
            var behaviourTree = GetBehaviourTreeAsset(assetPath);

            if (!behaviourTree || behaviourTree.RootNode && behaviourTree.Blackboard)
            {
                return;
            }

            var serializedTree = new SerializedObject(behaviourTree);

            if (!behaviourTree.RootNode)
            {
                AddRootNode(behaviourTree, serializedTree);
            }

            if (!behaviourTree.Blackboard)
            {
                AddBlackboard(behaviourTree, serializedTree);
            }
        }

        private static void AddRootNode(BehaviourTree behaviourTree, SerializedObject serializedTree)
        {
            var node = ScriptableObject.CreateInstance<Node>();
            node.name = typeof(Node).Name;
            node.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(node, behaviourTree);

            var behaviour = ScriptableObject.CreateInstance<RootNode>();
            behaviour.name = typeof(RootNode).Name;
            behaviour.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(behaviour, behaviourTree);

            var serializedNode = new SerializedObject(node);
            serializedNode.FindProperty("_Title").stringValue = "Root";
            serializedNode.FindProperty("_Guid").stringValue = GUID.Generate().ToString();
            serializedNode.FindProperty("_Position").vector2Value = Vector2.zero;
            serializedNode.FindProperty("_Behaviour").objectReferenceValue = behaviour;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();

            var serializedBehaviour = new SerializedObject(behaviour);
            serializedBehaviour.FindProperty("_Node").objectReferenceValue = node;
            serializedBehaviour.ApplyModifiedPropertiesWithoutUndo();

            serializedTree.Update();
            serializedTree.FindProperty("_RootNode").objectReferenceValue = node;
            serializedTree.FindProperty("_Nodes").AddToArray(node);
            serializedTree.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddBlackboard(BehaviourTree behaviourTree, SerializedObject serializedTree)
        {
            var blackboard = ScriptableObject.CreateInstance<Blackboard>();
            blackboard.name = "Blackboard";
            blackboard.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(blackboard, behaviourTree);

            serializedTree.Update();
            serializedTree.FindProperty("_Blackboard").objectReferenceValue = blackboard;
            serializedTree.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ProcessDeletedAsset(string assetPath)
        {
            if (!assetPath.EndsWith(".asset"))
            {
                return;
            }

            AssetDeleted?.Invoke();
        }

        private static BehaviourTree GetBehaviourTreeAsset(string assetPath)
        {
            if (!assetPath.EndsWith(".asset"))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<BehaviourTree>(assetPath);
        }
    }
}
