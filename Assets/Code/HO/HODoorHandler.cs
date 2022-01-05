using System;
using UnityEngine;


namespace ho
{
    [Serializable]
    public class HODoorHandler
    {
        public string               baseName;

        public HORoomReference      subHO;
        public HOKeyItem            keyItem;
        
        public GameObject           closedState;
        public GameObject           openState;

        public bool                 isValid
        {
            get { return openState != null && closedState != null; }
        }

        public bool                 isOpen
        {
            get { return openState?.activeSelf ?? false; }
            set { SetOpen(value); }
        }

        public void                 SetOpen(bool open)
        {
            closedState?.SetActive(!open);
            openState?.SetActive(open);
        }
    }
}
