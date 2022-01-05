using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimedLerpInterpolation
{
	Lerp,
	CLerp,
	Hermite,
	EaseOut,
	EaseIn,
	Boing
}

public abstract class TimedLerp<T> 
{
	protected T from, to;
	protected float duration = 0;
	protected float time = 0;
	protected T value;
	protected TimedLerpInterpolation interpolation = TimedLerpInterpolation.Hermite;

	public bool IsDone { get { return time>=duration; } } 
	public T Value { get { return value; } } 
	public T From { get { return from; } } 
	public T To { get { return to; } } 

    // Update is called once per frame
    public void Update()
    {
		time += Time.deltaTime;
        if (time < duration)
		{
			float t = time / duration;
			UpdateValue(t);
		} else
		{
			value = to;
		}
    }
	protected abstract void UpdateValue(float t);
}

public class TimedFloatLerp : TimedLerp<float>
{
	public TimedFloatLerp(float _from, float _to, float _duration, TimedLerpInterpolation _interpolation = TimedLerpInterpolation.Hermite)
	{
		from = _from;
		value = _from;
		to = _to;
		duration = _duration;
		interpolation  = _interpolation;
	}
	protected override void UpdateValue(float t)
	{
		switch (interpolation)
		{
			case TimedLerpInterpolation.Lerp: value = Mathf.Lerp(from, to, t); break;
			case TimedLerpInterpolation.CLerp: value = Mathfx.Clerp(from, to, t); break;
			case TimedLerpInterpolation.Hermite: value = Mathfx.Hermite(from, to, t); break;
			case TimedLerpInterpolation.EaseOut:value = Mathfx.Sinerp(from, to, t); break;
			case TimedLerpInterpolation.EaseIn:value = Mathfx.Coserp(from, to, t); break;
			case TimedLerpInterpolation.Boing:value = Mathfx.Berp(from, to, t); break;
		}
	}
}

public class TimedVec2Lerp : TimedLerp<Vector2>
{
	public TimedVec2Lerp(Vector2 _from, Vector2 _to, float _duration, TimedLerpInterpolation _interpolation = TimedLerpInterpolation.Hermite)
	{
		from = _from;
		value = _from;
		to = _to;
		duration = _duration;
		interpolation  = _interpolation;
	}
	protected override void UpdateValue(float t)
	{
		switch (interpolation)
		{
			case TimedLerpInterpolation.Lerp: value = Vector2.Lerp(from, to, t); break;
			case TimedLerpInterpolation.CLerp: value = Vector2.Lerp(from, to, t); break;
			case TimedLerpInterpolation.Hermite: value = Mathfx.Hermite(from, to, t); break;
			case TimedLerpInterpolation.EaseOut:value = Mathfx.Sinerp(from, to, t); break;
			case TimedLerpInterpolation.EaseIn:value = Mathfx.Coserp(from, to, t); break;
			case TimedLerpInterpolation.Boing:value = Mathfx.Berp(from, to, t); break;
		}
	}
}

public class TimedVec3Lerp : TimedLerp<Vector3>
{
	public TimedVec3Lerp(Vector3 _from, Vector3 _to, float _duration, TimedLerpInterpolation _interpolation = TimedLerpInterpolation.Hermite)
	{
		from = _from;
		value = _from;
		to = _to;
		duration = _duration;
		interpolation  = _interpolation;
	}
	protected override void UpdateValue(float t)
	{
		switch (interpolation)
		{
			case TimedLerpInterpolation.Lerp: value = Vector3.Lerp(from, to, t); break;
			case TimedLerpInterpolation.CLerp: value = Vector3.Lerp(from, to, t); break;
			case TimedLerpInterpolation.Hermite: value = Mathfx.Hermite(from, to, t); break;
			case TimedLerpInterpolation.EaseOut:value = Mathfx.Sinerp(from, to, t); break;
			case TimedLerpInterpolation.EaseIn:value = Mathfx.Coserp(from, to, t); break;
			case TimedLerpInterpolation.Boing:value = Mathfx.Berp(from, to, t); break;
		}
	}
}

public class TimedVec3BounceLerp : TimedLerp<Vector3>
{
	Vector3 offset = Vector3.zero;
	public TimedVec3BounceLerp(Vector3 _from, Vector3 _to, Vector3 _offset, float _duration, TimedLerpInterpolation _interpolation = TimedLerpInterpolation.Hermite)
	{
		from = _from;
		value = _from;
		to = _to;
		duration = _duration;
		offset = _offset;
		interpolation  = _interpolation;
	}
	protected override void UpdateValue(float t)
	{
		Vector3 bounce = Mathf.Sin(t * Mathf.PI) * offset;
		switch (interpolation)
		{
			case TimedLerpInterpolation.Lerp: value = bounce + Vector3.Lerp(from, to, t); break;
			case TimedLerpInterpolation.CLerp: value = bounce + Vector3.Lerp(from, to, t); break;
			case TimedLerpInterpolation.Hermite: value = bounce + Mathfx.Hermite(from, to, t); break;
			case TimedLerpInterpolation.EaseOut:value = bounce + Mathfx.Sinerp(from, to, t); break;
			case TimedLerpInterpolation.EaseIn:value = bounce + Mathfx.Coserp(from, to, t); break;
			case TimedLerpInterpolation.Boing:value = bounce + Mathfx.Berp(from, to, t); break;
		}
	}
}