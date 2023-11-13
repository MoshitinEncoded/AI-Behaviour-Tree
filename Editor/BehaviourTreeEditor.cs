using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using MoshitinEncoded.BehaviourTree;

namespace MoshitinEncoded.Editor.BehaviourTree
{
    internal class BehaviourTreeEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _VisualTreeAsset = default;
        [SerializeField] private BehaviourTreeController _TreeController;

        private BehaviourTreeView _TreeView;
        private BehaviourTreeController _TreeControllerActive;

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTreeController)
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
            if (_TreeController == null)
            {
                LoadWindowState();
            }

            PopulateTreeView(_TreeController);
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
            var window = JsonUtility.FromJson(windowJson, typeof(BehaviourTreeEditor)) as BehaviourTreeEditor;
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
                    PopulateTreeView(_TreeController);
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

        private BehaviourTreeController GetTreeFromSelection()
        {
            var selectedTree = Selection.activeObject as BehaviourTreeController;
            if (!selectedTree)
            {
                selectedTree = GetTreeFromGameObject();
            }

            return selectedTree;
        }

        private BehaviourTreeController GetTreeFromGameObject()
        {
            if (Selection.activeGameObject != null &&
                Selection.activeGameObject.TryGetComponent(out BehaviourTreeMachine behaviourMachine))
            {
                var behaviourTree = behaviourMachine.BehaviourTree;
                if (Application.isPlaying)
                {
                    _TreeController = behaviourMachine.BehaviourTree;
                    behaviourTree = behaviourMachine.BehaviourTreeInstance;
                }
                
                return behaviourTree;
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
                if (_TreeControllerActive != null)
                {
                    _TreeControllerActive.Updated -= OnTreeUpdate;
                }

                treeController.Updated += OnTreeUpdate;
                _TreeView.PopulateView(treeController);
                _TreeControllerActive = treeController;
            }
            else
            {
                if (AssetDatabase.CanOpenAssetInEditor(treeController.GetInstanceID()))
                {
                    _TreeView.PopulateView(treeController);
                    _TreeController = treeController;
                    _TreeControllerActive = treeController;
                }
            }
        }

        private void OnTreeUpdate() =>
            _TreeView?.UpdateNodeStates();
    }
}