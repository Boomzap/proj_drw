using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	static T		_self;
	public static bool isValid { get { return _self != null; } } 
	public static T instance
	{
		get 
		{
			if (_self == null)
			{
				T dataobject = Transform.FindObjectOfType<T>();
				if (dataobject ==null)
				{
					Debug.LogError("No valid singleton entries of type " + typeof(T).ToString());
					return null;
				}
				_self = dataobject;
			}
			return _self;
		}
	}
}

