using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MoshitinEncoded.AI
{
    public class BehaviourTreeEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField]
        private BehaviourTree _tree;

        private BehaviourTreeView _treeView;

        [MenuItem("Window/Moshitin Encoded/Behaviour Tree")]
        public static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("Behaviour Tree");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree) {
                OpenWindow();
                return true;
            }

            return false;
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            m_VisualTreeAsset.CloneTree(root);

            _treeView = root.Q<BehaviourTreeView>();
            _treeView.Init(this);

            OnSelectionChange();

            if (!_treeView.IsPopulated())
            {
                if (_tree == null)
                {
                    LoadWindowState();
                }

                _treeView.PopulateView(_tree);
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

        private void LoadWindowState()
        {
            if (_tree != null)
            {
                return;
            }

            var data = EditorPrefs.GetString("BehaviourTreeWindow", JsonUtility.ToJson(this, false));
            var window = JsonUtility.FromJson(data, typeof(BehaviourTreeEditor)) as BehaviourTreeEditor;
            if (window != null)
            {
                _tree = window._tree;
            }
        }

        private void SaveWindowState()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("BehaviourTreeWindow", data);
        }

        private void OnPlayModeStateChange(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
            }
        }

        private void OnSelectionChange()
        {
            var behaviourTree = Selection.activeObject as BehaviourTree;
            if (!behaviourTree)
            {
                behaviourTree = GetBehaviourFromGameObject();
            }

            if (Application.isPlaying)
            {
                if (behaviourTree != null)
                {
                    _treeView.PopulateView(behaviourTree);
                    _tree = behaviourTree;
                }
            }
            else
            {
                if (behaviourTree != null && AssetDatabase.CanOpenAssetInEditor(behaviourTree.GetInstanceID()))
                {
                    _treeView.PopulateView(behaviourTree);
                    _tree = behaviourTree;
                }
            }
        }

        private static BehaviourTree GetBehaviourFromGameObject()
        {
            if (Selection.activeGameObject != null && 
                Selection.activeGameObject.TryGetComponent(out BehaviourMachine treeMachine))
            {
                return treeMachine.BehaviourTree;
            }

            return null;
        }

        private void OnInspectorUpdate()
        {
            _treeView?.UpdateNodeStates();
        }
    }
}