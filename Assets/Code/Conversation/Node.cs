using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boomzap.Conversation
{

    [System.Serializable]
    public abstract class Node : ScriptableObject
    {
        [SerializeField, HideInInspector] protected bool                _isOption = false;
        [SerializeField, HideInInspector] protected Conversation        owner;
        [SerializeField, HideInInspector] protected Node                _parent;
        [SerializeField, HideInInspector] protected List<Connection>    connections = new List<Connection>();
        [SerializeField, HideInInspector] protected SerializableGUID    _guid = new SerializableGUID();

        public SerializableGUID guid
        {
            get { return _guid; }
        }
        public bool isOption
        {
            get { return _isOption; }
            set { _isOption = value; }
        }

        public Node parent { get { return _parent; } }
        public object editorData { get; set; }

        protected abstract bool IsChildTypeValid(Node node);
        virtual protected void Initialize(Conversation owner)
        {
            hideFlags = HideFlags.HideInHierarchy;
            this.owner = owner;

            #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(this, owner);
            #endif
        }

        public bool SameOwner(Node other)
        {
            return other.owner == this.owner;
        }

        public void AddChild(Node child)
        {
            if (!IsChildTypeValid(child)) return;

            child._parent = this;
            connections.Add(new Connection(child, false));
        }

        public void AddChildAt(Node child, int idx)
        {
            if (!IsChildTypeValid(child)) return;

            child._parent = this;
            connections.Insert(idx, new Connection(child, false));
        }

        public void ReplaceChild(Node child, Node withChild)
        {
            if (!IsChildTypeValid(child)) return;

            withChild._parent = this;

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].Target == child)
                    connections[i] = new Connection(withChild, false);
            }
        }

        public void AddChildBefore(Node child, Node after)
        {
            int chIdx = connections.FindIndex(x => x.Target == after);
            AddChildBefore(child, chIdx);
        }

        public void AddChildBefore(Node child, int before)
        {
            if (!IsChildTypeValid(child)) return;

            child._parent = this;

            connections.Insert(before, new Connection(child, false));
        }

        public void AddChildAfter(Node child, Node after)
        {
            int chIdx = connections.FindIndex(x => x.Target == after);
            AddChildAfter(child, chIdx);
        }

        public void AddChildAfter(Node child, int after)
        {
            if (!IsChildTypeValid(child)) return;

            child._parent = this;

            connections.Insert(after + 1, new Connection(child, false));
        }

        public int NumChildren()
        {
            return connections?.Count ?? 0;
        }

        public bool IsLink(int i)
        {
            if (connections == null || connections.Count <= i) return false;
            return connections[i].IsLink;
        }

        public Node GetChild(int i)
        {
            if (connections == null || connections.Count <= i) return null;
            return connections[i].Target;
        }

        public int IndexOfChild(Node child)
        {
            int index = connections.FindIndex(connection => connection.Target == child && !connection.IsLink);

            return index;
        }

        public virtual void RemoveChild(Node child)
        {
            if (!IsChildTypeValid(child)) return;

            int index = connections.FindIndex(connection => connection.Target == child && !connection.IsLink);

            connections.RemoveAt(index);

            child._parent = null;
        }

        public void ClearChildren()
        {
            connections.Clear();
        }

        public void SwapChildren(int a, int b)
        {
            var t = connections[a];
            connections[a] = connections[b];
            connections[b] = t;
        }
    }
}
