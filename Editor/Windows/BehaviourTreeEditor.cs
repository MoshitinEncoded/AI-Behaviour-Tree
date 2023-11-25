using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using MoshitinEncoded.AI;
using MoshitinEncoded.AI.BehaviourTreeLib;

namespace MoshitinEncoded.Editor.AI.BehaviourTreeLib
{
    internal class BehaviourTreeEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _VisualTreeAsset = default;
        [SerializeField] private BehaviourTree _BehaviourTree;

        private BehaviourTreeView _TreeView;
        private BehaviourTree _BehaviourTreeActive;
        private readonly string _EditorPrefKey = "BehaviourTree";

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        [MenuItem("Window/AI/Behaviour Tree")]
        public static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("Behaviour Tree");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            _VisualTreeAsset.CloneTree(root);

            _TreeView = root.Q<BehaviourTreeView>();
            _TreeView.Init(this);

            PopulateViewFromSelection();

            if (!_TreeView.IsPopulated)
            {
                PopulateViewFromBackup();
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

        private void PopulateViewFromBackup()
        {
            if (_BehaviourTree == null)
            {
                LoadWindowState();
            }

            PopulateTreeView(_BehaviourTree);
        }

        private void SaveWindowState()
        {
            var treeJson = _BehaviourTree != null ? JsonUtility.ToJson(_BehaviourTree, false) : "";
            EditorPrefs.SetString(_EditorPrefKey, treeJson);
        }

        private void LoadWindowState()
        {
            if (_BehaviourTree != null)
            {
                return;
            }

            var treeJson = EditorPrefs.GetString(_EditorPrefKey);
            var behaviourTree = treeJson != "" ? JsonUtility.FromJson<BehaviourTree>(treeJson) : null;
            if (behaviourTree != null)
            {
                _BehaviourTree = behaviourTree;
            }
        }

        private void OnPlayModeStateChange(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    PopulateViewFromBackup();
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
            var selectedTree = GetTreeFromSelection();
            PopulateTreeView(selectedTree);
        }

        private BehaviourTree GetTreeFromSelection()
        {
            var selectedTree = Selection.activeObject as BehaviourTree;
            if (!selectedTree)
            {
                selectedTree = GetTreeFromGameObject();
            }

            return selectedTree;
        }

        private BehaviourTree GetTreeFromGameObject()
        {
            if (Selection.activeGameObject != null &&
                Selection.activeGameObject.TryGetComponent(out BehaviourTreeRunner behaviourMachine))
            {
                var behaviourTree = behaviourMachine.BehaviourTree;
                if (Application.isPlaying)
                {
                    _BehaviourTree = behaviourMachine.BehaviourTree;
                    behaviourTree = behaviourMachine.BehaviourTreeInstance;
                }
                
                return behaviourTree;
            }

            return null;
        }

        private void PopulateTreeView(BehaviourTree behaviourTree)
        {
            if (behaviourTree == null || _TreeView == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                if (_BehaviourTreeActive != null)
                {
                    _BehaviourTreeActive.Updated -= OnTreeUpdate;
                }

                behaviourTree.Updated += OnTreeUpdate;
                _TreeView.PopulateView(behaviourTree);
                _BehaviourTreeActive = behaviourTree;
            }
            else
            {
                if (AssetDatabase.CanOpenAssetInEditor(behaviourTree.GetInstanceID()))
                {
                    _TreeView.PopulateView(behaviourTree);
                    _BehaviourTree = behaviourTree;
                    _BehaviourTreeActive = behaviourTree;
                }
            }
        }

        private void OnTreeUpdate() =>
            _TreeView?.UpdateNodeStates();
    }
}