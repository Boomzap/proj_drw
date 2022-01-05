using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ho
{
    [Serializable]
    public class HOFindableLogicValidity
    {
        public List<string>     validLogicTypes = new List<string>();

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static string[]  logicTypes { get { return GetLogicTypes(); } }
        public static string[]  friendlyTypes { get { return GetFriendlyTypes(); } }
        
        static string[] _logicTypes;
        static string[] _friendlyTypes;

        static string[] GetLogicTypes()
        {
            if (_logicTypes == null)
            {
                _logicTypes = typeof(HOLogic).Assembly.GetTypes()
                                .Where(x => x.IsSubclassOf(typeof(HOLogic)))
                                .Select(x => x.Name).ToArray();
            }

            return _logicTypes;
        }

        static string[] GetFriendlyTypes()
        {
            if (_friendlyTypes == null)
            {
                var n = typeof(HOLogic).Assembly.GetTypes()
                                .Where(x => x.IsSubclassOf(typeof(HOLogic)));

                _friendlyTypes = new string[n.Count()];

                for (int i = 0; i < n.Count(); i++)
                {
                    var a = n.ElementAt(i).GetCustomAttributes(typeof(FriendlyNameAttribute), false);
                    if (a.Length > 0)
                        _friendlyTypes[i] = (a[0] as FriendlyNameAttribute).friendlyName;
                    else
                        _friendlyTypes[i] = n.ElementAt(i).Name;
                }
            }

            return _friendlyTypes;            
        }

        #endif
    }
}
