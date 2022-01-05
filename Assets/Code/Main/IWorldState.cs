
using System.Collections.Generic;

namespace ho
{
#if UNITY_EDITOR
    public static class WorldStateHelper
    {
        static List<string> worldStates = null;

        public static List<string> WorldStates
        {
            get
            {
                if (worldStates == null)
                {
                    worldStates = new List<string>();
                    var types = TypeHelper.GetTypesImplementingInterface(typeof(IWorldState));
                    foreach (var t in types)
                    {
                        worldStates.Add(t.Name);
                    }
                }

                return worldStates;
            }
        }
    }
#endif

    public interface IWorldState
    {
        public void OnLeave();
        public bool ShouldDestroyOnLeave();

    }
}
