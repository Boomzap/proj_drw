using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ho
{
    public class ResetParticleEffectsOnAwake : MonoBehaviour
    {
        [SerializeField, Sirenix.OdinInspector.ReadOnly]
        ParticleSystem[] particleSystems = new ParticleSystem[0];
        

        private void Reset()
        {
            RefreshSystemArray();   
        }

        [Sirenix.OdinInspector.Button]
        void RefreshSystemArray()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        private void OnEnable()
        {
            foreach (var p in particleSystems)
            {
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                p.Play();
            }
        }
    }
}
