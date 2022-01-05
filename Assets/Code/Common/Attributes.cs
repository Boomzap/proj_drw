using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Diagnostics;

namespace ho
{
    public class FriendlyNameAttribute : Attribute
    {
        public string friendlyName;

        public FriendlyNameAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
        }
    }

    [Conditional("UNITY_EDITOR")]
    public class CheckListAttribute : Attribute
    {
        public string getter;

        public CheckListAttribute(string getterName)
        {
            getter = getterName;
        }
    }
}
