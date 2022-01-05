using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace ho
{
    public static class ExtensionMethods
    {
        public static List<T> CreateShallowCopy<T>(this List<T> src)
        {
            return new List<T>(src);
        }

        private static System.Collections.IEnumerator ExecuteAfterDelayCor(float delay, UnityAction action)
        {
            yield return new WaitForSeconds(delay);

            action?.Invoke();
        }

        public static void ExecuteAfterDelay(this MonoBehaviour monoBehaviour, float delay, UnityAction action)
        {
            monoBehaviour.StartCoroutine(ExecuteAfterDelayCor(delay, action));
            
        }
    }
}
