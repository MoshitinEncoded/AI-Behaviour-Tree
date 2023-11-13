using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using MoshitinEncoded.BehaviourTree;

namespace MoshitinEncoded.Editor.BehaviourTree
{
    internal class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public struct NodeInfo
        {
            public Type Type;
            public string MenuPath;
            public string[] MenuPathSpliced;
        }

        private BehaviourTreeView _behaviourTreeView;
        private EditorWindow _window;
        private Texture2D _icon;

        public void Init(EditorWindow editorWindow, BehaviourTreeView behaviourTreeView)
        {
            _behaviourTreeView = behaviourTreeView;
            _window = editorWindow;

            // Workaround for indentation of search tree entries with an icon
            _icon = new Texture2D(1, 1);
            _icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _icon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchTree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"))
            };

            CreateTreeEntries(searchTree);

            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var localMousePosition = _behaviourTreeView.contentViewContainer.ScreenToLocal(_window, context.screenMousePosition);

            _behaviourTreeView.CreateNode(
                nodeType: searchTreeEntry.userData as Type,
                nodeTitle: searchTreeEntry.content.text,
                position: localMousePosition);

            return true;
        }

        private void CreateTreeEntries(List<SearchTreeEntry> searchTree)
        {
            var nodesInfo = GetNodesInfo();
            nodesInfo.Sort(SortByPath);

            string[] previousNodePath = null;

            foreach (var nodeInfo in nodesInfo)
            {
                var nodePath = nodeInfo.MenuPathSpliced;
                var groupsToCreate = 0;

                if (previousNodePath != null)
                {
                    var groupsToCompare = Mathf.Min(nodePath.Length, previousNodePath.Length) - 1;

                    for (var i = 0; i < groupsToCompare; i++)
                    {
                        if (nodePath[i] != previousNodePath[i])
                        {
                            groupsToCreate = nodePath.Length - 1 - i;
                        }
                    }
                }
                else
                {
                    groupsToCreate = nodePath.Length - 1;
                }

                for (var i = nodePath.Length - 1 - groupsToCreate; i < nodePath.Length - 1; i++)
                {
                    searchTree.Add(new SearchTreeGroupEntry(new GUIContent(nodePath[i]), i + 1));
                }

                searchTree.Add(new SearchTreeEntry(new GUIContent(nodeInfo.MenuPathSpliced.Last(), _icon))
                {
                    userData = nodeInfo.Type,
                    level = nodePath.Length
                });

                previousNodePath = nodePath;
            }
        }

        private static List<NodeInfo> GetNodesInfo()
        {
            var nodeTypesCollection = TypeCache.GetTypesDerivedFrom<MoshitinEncoded.BehaviourTree.Node>();
            var nodesInfo = new List<NodeInfo>();

            foreach (var nodeType in nodeTypesCollection)
            {
                var nodeMenuPath = nodeType.GetCustomAttribute<CreateNodeMenuAttribute>()?.Path;
                if (nodeMenuPath == null)
                {
                    continue;
                }

                nodesInfo.Add(new NodeInfo()
                {
                    Type = nodeType,
                    MenuPath = nodeMenuPath,
                    MenuPathSpliced = nodeMenuPath.Split('/')
                });
            }

            return nodesInfo;
        }

        private static int SortByPath(NodeInfo x, NodeInfo y) =>
            string.Compare(x.MenuPath, y.MenuPath, StringComparison.CurrentCulture);
    }
}