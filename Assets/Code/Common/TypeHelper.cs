using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


using Sirenix.OdinInspector;

namespace ho
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, Inherited = false)]
    public class ExcludeFromTypeHelperListingAttribute : PropertyAttribute
    {

    }

    public class TypePair
    {
        public string niceName;
        public Type type;
    }


    public static class TypeHelper
    {
        public static List<Type> GetTypesImplementingBaseClass(Type baseClass)
        {
            return baseClass.Assembly.GetTypes().Where(x => x.IsAssignableFrom(baseClass)).ToList();
        }

        public static List<Type> GetTypesImplementingInterface(Type baseClass)
        {
            return baseClass.Assembly.GetTypes().Where(x => !x.IsInterface && baseClass.IsAssignableFrom(x)
                && x.GetCustomAttribute<ExcludeFromTypeHelperListingAttribute>() == null
                ).ToList();
        }
    }
}