using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Sirenix.OdinInspector;

namespace ho
{
    public class PoolableFX : PoolableBehaviour
    {
        [SerializeField, InfoBox("If this is 0 it will wait until the IsDone flag is set.")]
        float disableAfterTime = 0f;

        float startTime = 0;

        void OnEnable()
        {
            startTime = Time.time;
        }

        protected virtual void Update()
        {
            if (disableAfterTime > 0)
            {
                if ((Time.time - startTime) >= disableAfterTime)
                    gameObject.SetActive(false);
            } else if (IsDone())
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual bool IsDone()
        {
            return true;
        }
    }
}
