using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using MoshitinEncoded.BehaviourTree;
using System;

namespace MoshitinEncoded.Editor.BehaviourTree
{
    public class BehaviourGraphEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _VisualTreeAsset = default;
        [SerializeField] private BehaviourTreeController _TreeController;

        private BehaviourGraphView _TreeView;

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTreeController) {
                OpenWindow();
                return true;
            }

            return false;
        }

        [MenuItem("Window/Moshitin Encoded/Behaviour Graph")]
        public static void OpenWindow()
        {
            BehaviourGraphEditor wnd = GetWindow<BehaviourGraphEditor>();
            wnd.titleContent = new GUIContent("Behaviour Graph");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            _VisualTreeAsset.CloneTree(root);

            _TreeView = root.Q<BehaviourGraphView>();
            _TreeView.Init(this);

            OnSelectionChange();

            if (!_TreeView.IsPopulated)
            {
                if (_TreeController == null)
                {
                    LoadWindowState();
                }

                PopulateTreeView(_TreeController);
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            SaveWindowState();
        }

        private void SaveWindowState()
        {
            var windowJson = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("BehaviourGraphWindow", windowJson);
        }

        private void LoadWindowState()
        {
            if (_TreeController != null)
            {
                return;
            }

            var windowJson = EditorPrefs.GetString("BehaviourGraphWindow", JsonUtility.ToJson(this, false));
            var window = JsonUtility.FromJson(windowJson, typeof(BehaviourGraphEditor)) as BehaviourGraphEditor;
            if (window != null)
            {
                _TreeController = window._TreeController;
            }
        }

        private void OnPlayModeStateChange(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    PopulateViewFromSelection();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    PopulateViewFromSelection();
                    break;
            }
        }

        private void OnSelectionChange()
        {
            PopulateViewFromSelection();
        }

        private void PopulateViewFromSelection()
        {
            var selectedGraphAsset = GetGraphFromSelection();
            PopulateTreeView(selectedGraphAsset);
        }

        private static BehaviourTreeController GetGraphFromSelection()
        {
            var selectedGraphAsset = Selection.activeObject as BehaviourTreeController;
            if (!selectedGraphAsset)
            {
                selectedGraphAsset = GetGraphFromGameObject();
            }

            return selectedGraphAsset;
        }

        private static BehaviourTreeController GetGraphFromGameObject()
        {
            if (Selection.activeGameObject != null && 
                Selection.activeGameObject.TryGetComponent(out BehaviourTreeMachine behaviourMachine))
            {
                return behaviourMachine.BehaviourTree;
            }

            return null;
        }

        private void PopulateTreeView(BehaviourTreeController treeController)
        {
            if (treeController == null || _TreeView == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                _TreeView.PopulateView(treeController);
                if (_TreeController != null)
                {
                    _TreeController.Updated -= OnTreeUpdate;
                }

                _TreeController = treeController;
                _TreeController.Updated += OnTreeUpdate;
            }
            else
            {
                if (AssetDatabase.CanOpenAssetInEditor(treeController.GetInstanceID()))
                {
                    _TreeView.PopulateView(treeController);
                    _TreeController = treeController;
                }
            }
        }

        private void OnTreeUpdate() =>
            _TreeView?.UpdateNodeStates();
    }
}