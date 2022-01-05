using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ho
{
    public class MainMenuWorld : MonoBehaviour, IWorldState
    {
        public GameObject ta;
        public bool ShouldDestroyOnLeave()
        {
            return true;
        }

        public void OnLeave()
        {
            StateCache.instance.UnloadMainMenu();
        }
    }
}
