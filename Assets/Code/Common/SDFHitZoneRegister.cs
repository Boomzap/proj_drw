using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ho
{
    public class SDFHitZoneRegister : MonoBehaviour
    {
        public delegate bool FilterDelegate(GameObject testGameObject);
        public static FilterDelegate s_HitTestFilter;
        
        List<SDFHitZone>    registeredHitzones = new List<SDFHitZone>();

        private void Awake()
        {
            registeredHitzones.Clear();
        }

        public void Register(SDFHitZone z)
        {
            //Debug.Log($"Add hitzone: {z.gameObject.name}");
            if (!registeredHitzones.Contains(z))
            {
                registeredHitzones.Add(z);

                registeredHitzones.Sort((SDFHitZone a, SDFHitZone b) =>
                {
                    return b.GetComponent<SpriteRenderer>().sortingOrder.CompareTo(a.GetComponent<SpriteRenderer>().sortingOrder);
                });
            }
        }

        public void Unregister(SDFHitZone z)
        {
            //Debug.Log($"Remove hitzone: {z.gameObject.name}");

            if (registeredHitzones.Contains(z))
                registeredHitzones.Remove(z);
        }

        public GameObject HitTest(Vector2 worldPos)
        {
            foreach (var z in registeredHitzones)
            {
                if (z == null) continue;
                if (!z.enabled) continue;

                if ((s_HitTestFilter?.Invoke(z.gameObject) ?? true) == false)
                    continue;

                if (z.IsInside(worldPos))
                    return z.gameObject;
            }

            return null;
        }

        public IEnumerable<GameObject> HitTestAll(Vector2 worldPos)
        {
            List<GameObject> go = new List<GameObject>();

            foreach (var z in registeredHitzones)
            {
                if (z == null) continue;
                if (!z.enabled) continue;

                if ((s_HitTestFilter?.Invoke(z.gameObject) ?? true) == false)
                    continue;

                if (z.IsInside(worldPos))
                    go.Add(z.gameObject);
            }

            return go;;
        }
    }
}
