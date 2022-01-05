using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Boomzap.Conversation
{
    [System.Serializable]
    public class Connection
    {
        [SerializeField, HideInInspector] Node _target;
        [SerializeField, HideInInspector] bool _isLink;

        public Node Target { get { return _target; } }
        public bool IsLink { get { return _isLink; } }

        public Connection(Node target, bool isLink)
        {
            _target = target;
            _isLink = isLink;
        }
    }
}
