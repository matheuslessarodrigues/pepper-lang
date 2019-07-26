public sealed class ByteCodeChunk
{
	public Buffer<byte> bytes = new Buffer<byte>(256);
	public Buffer<int> sourceIndexes = new Buffer<int>(256);
	public Buffer<Value> constants = new Buffer<Value>(64);
	public Buffer<string> stringLiterals = new Buffer<string>(16);

	public int AddConstant(Value value)
	{
		var index = constants.count;
		constants.PushBack(value);
		return index;
	}

	public int AddStringLiteral(string literal)
	{
		var constantIndex = constants.count;
		var stringIndex = stringLiterals.count;
		stringLiterals.PushBack(literal);
		constants.PushBack(new Value(Value.Type.Object, new Value.Data(stringIndex)));
		return constantIndex;
	}

	public void WriteByte(byte value, int sourceIndex)
	{
		bytes.PushBack(value);
		sourceIndexes.PushBack(sourceIndex);
	}
}