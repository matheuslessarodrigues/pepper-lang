using System.Collections.Generic;

public readonly struct CompileError
{
	public readonly int sourceIndex;
	public readonly string message;

	public CompileError(int sourceIndex, string message)
	{
		this.sourceIndex = sourceIndex;
		this.message = message;
	}
}

public sealed class Compiler
{
	public readonly List<CompileError> errors = new List<CompileError>();
	public Token previousToken;
	public Token currentToken;

	private ITokenizer tokenizer;
	private bool panicMode;
	private ByteCodeChunk chunk;

	public void Begin(ITokenizer tokenizer)
	{
		errors.Clear();
		this.tokenizer = tokenizer;
		previousToken = new Token(Token.EndKind, 0, 0);
		currentToken = new Token(Token.EndKind, 0, 0);
		panicMode = false;
		chunk = new ByteCodeChunk();
	}

	public void AddError(int sourceIndex, string message)
	{
		if (panicMode)
			return;

		panicMode = true;
		errors.Add(new CompileError(sourceIndex, message));
	}

	public ByteCodeChunk GetByteCodeChunk()
	{
		var c = chunk;
		chunk = null;
		return c;
	}

	public void Next()
	{
		previousToken = currentToken;

		while (true)
		{
			currentToken = tokenizer.Next();
			if (currentToken.kind != Token.ErrorKind)
				break;

			AddError(currentToken.index, "Invalid char");
		}
	}

	public void Consume(int tokenKind, string errorMessage)
	{
		if (currentToken.kind == tokenKind)
			Next();
		else
			AddError(currentToken.index, errorMessage);
	}

	public T Convert<T>(System.Func<string, Token, T> converter)
	{
		return tokenizer.Convert(previousToken, converter);
	}

	public void EmitByte(byte value)
	{
		chunk.WriteByte(value, previousToken.index);
	}

	public void EmitInstruction(Instruction instruction)
	{
		EmitByte((byte)instruction);
	}

	public void EmitLoadConstant(Value value)
	{
		var index = System.Array.IndexOf(chunk.constants.buffer, value);
		if (index < 0)
			index = chunk.AddConstant(value);

		EmitInstruction(Instruction.LoadConstant);
		EmitByte((byte)index);
	}
}