using System.Runtime.InteropServices;

public readonly struct Value
{
	public enum Type
	{
		Nil,
		Bool,
		Int,
		Float,
	}

	[StructLayout(LayoutKind.Explicit)]
	public readonly struct Data
	{
		[FieldOffset(0)]
		public readonly bool asBool;
		[FieldOffset(0)]
		public readonly int asInt;
		[FieldOffset(0)]
		public readonly float asFloat;

		public Data(bool value)
		{
			this = default(Data);
			asBool = value;
		}

		public Data(int value)
		{
			this = default(Data);
			asInt = value;
		}

		public Data(float value)
		{
			this = default(Data);
			asFloat = value;
		}
	}

	public readonly Type type;
	public readonly Data data;

	public Value(bool value)
	{
		type = Type.Bool;
		data = new Data(value);
	}

	public Value(int value)
	{
		type = Type.Int;
		data = new Data(value);
	}

	public Value(float value)
	{
		type = Type.Int;
		data = new Data(value);
	}

	public static bool AreEqual(Value a, Value b)
	{
		if (a.type != b.type)
			return false;

		switch (a.type)
		{
		case Type.Bool: return a.data.asBool == b.data.asBool;
		case Type.Int: return a.data.asInt == b.data.asInt;
		case Type.Float: return a.data.asFloat == b.data.asFloat;
		default: return false;
		}
	}

	public bool IsTruthy()
	{
		return type != Type.Nil && (type != Type.Bool || data.asBool);
	}

	public bool IsFalsey()
	{
		return type == Type.Nil || (type == Type.Bool && !data.asBool);
	}

	public override string ToString()
	{
		switch (type)
		{
		case Type.Nil:
			return "nil";
		case Type.Bool:
			return data.asBool ? "true" : "false";
		case Type.Int:
			return data.asInt.ToString();
		case Type.Float:
			return data.asFloat.ToString();
		default:
			return "<invalid value>";
		}
	}
}