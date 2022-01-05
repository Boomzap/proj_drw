using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout( LayoutKind.Explicit ), Serializable]
public class SerializableGUID : IComparable, IComparable<Guid>, IEquatable<Guid>
{
	public SerializableGUID( )
	{
		Guid = Guid.NewGuid( );
	}
	public SerializableGUID(Guid fromGUID )
	{
		if (fromGUID == null) 
		{
			Guid = Guid.Empty;
		} else
		{
			Guid = fromGUID;
		}
	}

	public static SerializableGUID Empty() { return new SerializableGUID(Guid.Empty); }

	[FieldOffset(0)]
	public Guid Guid;
	[FieldOffset(0), SerializeField]
	private Int32 GuidPart1;
	[FieldOffset(4), SerializeField]
	private Int32 GuidPart2;
	[FieldOffset(8), SerializeField]
	private Int32 GuidPart3;
	[FieldOffset(12), SerializeField]
	private Int32 GuidPart4;

	public static implicit operator Guid ( SerializableGUID uGuid )
	{
		if (uGuid == null) return Guid.Empty;
		return uGuid.Guid;
	}

	public Int32 CompareTo ( object obj )
	{
		if( obj == null )
			return -1;

		if( obj is SerializableGUID )
			return ((SerializableGUID)obj).Guid.CompareTo( Guid );

		if( obj is Guid )
			return ((Guid)obj).CompareTo( Guid );

		return -1;
	}
	public Int32 CompareTo ( Guid other )
	{
		return Guid.CompareTo( other );
	}
	public Boolean Equals ( Guid other )
	{
		return Guid == other;
	}


    public static bool operator ==(SerializableGUID lhs, SerializableGUID rhs)
    {
		if (ReferenceEquals(lhs, null)) return false;
		if (ReferenceEquals(rhs, null)) return false;
        return lhs.Equals(rhs.Guid);
    }

    public static bool operator !=(SerializableGUID lhs, SerializableGUID rhs)
    {
		if (ReferenceEquals(lhs, null)) return false;
		if (ReferenceEquals(rhs, null)) return false;
        return !(lhs.Equals(rhs.Guid));
    }

	public override Boolean Equals ( object obj )
	{
		if( obj == null )
			return false;

		if( obj is SerializableGUID )
			return (SerializableGUID)obj == Guid;

		if( obj is Guid )
			return (Guid)obj == Guid;

		return false;
	}

	public override Int32 GetHashCode ( )
	{
		return Guid.GetHashCode( );
	}

	public override string ToString()	{ return GuidPart1.ToString() + "-" + GuidPart2.ToString()+ "-" + GuidPart3.ToString()+ "-" + GuidPart4.ToString(); }  

    public string ToStringHex() 
    {
        return $"{GuidPart1:X8}-{GuidPart2:X8}-{GuidPart3:X8}-{GuidPart4:X8}";
    }
}
