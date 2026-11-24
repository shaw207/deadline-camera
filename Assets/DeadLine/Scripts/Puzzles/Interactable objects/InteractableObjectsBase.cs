using UnityEngine;

namespace DeadLine
{
    public abstract class InteractableObjectsBase : MonoBehaviour
    {
        [Header("Base")]
        [SerializeField] protected bool active = false;
        [SerializeField] protected bool interactable = true;
        [SerializeField] protected GameObject[] RelativeObjectRefs;

        protected IRelativeObjects[] RelativeObjects;
        private bool _resolved = false;

        [Header("Proximity Prompt")]
        [SerializeField] private bool usePrompt = true;
        [SerializeField] private GameObject promptPrefab;

        [Header("Trigger (optional auto-setup)")]
        [SerializeField] private bool autoEnsureTrigger = false;
        [SerializeField] private float autoTriggerRadius = 1.5f;

        private ProximityPrompt _promptInstance;
        private PlayerInteraction _playerInteractionCached;
        private int _playerOverlapCount = 0;

        public abstract void Interact();

        protected virtual void Awake()
        {
            if (!autoEnsureTrigger) return;

            var col = GetComponent<Collider>();
            if (col == null)
            {
                var sc = gameObject.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = autoTriggerRadius;
            }
            else
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var pi = other.GetComponentInParent<PlayerInteraction>();
            if (pi == null) return;

            if (_playerInteractionCached == null)
            {
                _playerInteractionCached = pi;
                _playerOverlapCount = 0;
            }

            if (_playerInteractionCached != pi) return;

            _playerOverlapCount++;
            if (_playerOverlapCount == 1)
            {
                pi.Register(this);
                UpdatePromptVisibility(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var pi = other.GetComponentInParent<PlayerInteraction>();
            if (pi == null) return;

            if (_playerInteractionCached != pi) return;

            _playerOverlapCount = Mathf.Max(0, _playerOverlapCount - 1);

            if (_playerOverlapCount == 0)
            {
                pi.Unregister(this);
                HidePrompt();
                _playerInteractionCached = null;
            }
        }

        private void Update()
        {
            if (_playerInteractionCached != null && usePrompt)
                UpdatePromptVisibility(false);
        }

        private void UpdatePromptVisibility(bool forceCreateIfNeeded)
        {
            if (!usePrompt) return;

            bool shouldShow = _playerInteractionCached != null && ShouldShowPrompt();

            if (!shouldShow)
            {
                HidePrompt();
                return;
            }

            if (_promptInstance == null)
            {
                if (!forceCreateIfNeeded) return;
                if (promptPrefab == null) return;

                var go = Instantiate(promptPrefab, transform);
                _promptInstance = go.GetComponent<ProximityPrompt>();
                if (_promptInstance == null)
                {
                    Destroy(go);
                    return;
                }
            }

            _promptInstance.gameObject.SetActive(true);

            var cam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            _promptInstance.Bind(transform, cam);
        }

        protected virtual bool ShouldShowPrompt()
        {
            if (!interactable) return false;
            return CheckStatusForPrompt();
        }

        private bool CheckStatusForPrompt()
        {
            ResolveRelativeObjectsIfNeeded();

            if (!interactable) return false;

            if (RelativeObjects == null || RelativeObjects.Length == 0)
                return true;

            foreach (IRelativeObjects ro in RelativeObjects)
            {
                if (ro == null) continue;
                if (!ro.checkStatu())
                    return false;
            }

            return true;
        }

        private void HidePrompt()
        {
            if (_promptInstance != null)
                _promptInstance.gameObject.SetActive(false);
        }

        protected virtual void OnDisable()
        {
            HidePrompt();

            if (_playerInteractionCached != null)
            {
                _playerInteractionCached.Unregister(this);
                _playerInteractionCached = null;
            }

            _playerOverlapCount = 0;
        }

        protected void ResolveRelativeObjectsIfNeeded()
        {
            if (_resolved) return;
            _resolved = true;

            if (RelativeObjects != null && RelativeObjects.Length > 0)
                return;

            if (RelativeObjectRefs == null || RelativeObjectRefs.Length == 0)
            {
                RelativeObjects = null;
                return;
            }

            RelativeObjects = new IRelativeObjects[RelativeObjectRefs.Length];

            for (int i = 0; i < RelativeObjectRefs.Length; i++)
            {
                var go = RelativeObjectRefs[i];
                if (go == null) continue;

                var ro = go.GetComponent<IRelativeObjects>();
                if (ro == null) continue;

                RelativeObjects[i] = ro;
            }
        }

        protected virtual void OnValidate()
        {
            _resolved = false;
        }

        protected bool checkStatus()
        {
            ResolveRelativeObjectsIfNeeded();

            if (RelativeObjects == null)
                return false;

            if (!interactable)
                return false;

            foreach (IRelativeObjects ro in RelativeObjects)
            {
                if (ro == null) continue;
                if (!ro.checkStatu())
                    return false;
            }

            return true;
        }

        public bool checkInteractable() => interactable;

        public void setInteractable(bool value) => interactable = value;

        public void setRelativeObjects(IRelativeObjects[] value)
        {
            RelativeObjects = value;
            _resolved = true;
        }

        public IRelativeObjects[] getRelativeObjects()
        {
            ResolveRelativeObjectsIfNeeded();
            return RelativeObjects;
        }

        public void setRelativeObjectRefs(GameObject[] value)
        {
            RelativeObjectRefs = value;
            _resolved = false;
        }

        protected void interpretAction()
        {
            ResolveRelativeObjectsIfNeeded();
            if (RelativeObjects == null || RelativeObjects.Length == 0) return;

            foreach (IRelativeObjects ro in RelativeObjects)
            {
                if (ro == null) continue;

                if (!ro.UseParam()) ro.action();
                else ro.action(this.gameObject);
            }
        }

        protected void interpretNoAction()
        {
            ResolveRelativeObjectsIfNeeded();
            if (RelativeObjects == null || RelativeObjects.Length == 0) return;

            foreach (IRelativeObjects ro in RelativeObjects)
            {
                if (ro == null) continue;

                if (!ro.UseParam()) ro.Noaction();
                else ro.Noaction(this.gameObject);
            }
        }
    }
}
