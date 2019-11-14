internal sealed class CompilerIO
{
	public Mode mode;
	public readonly Parser parser;
	public Buffer<CompileError> errors = new Buffer<CompileError>();
	public int sourceIndex;

	public bool isInPanicMode;
	public ByteCodeChunk chunk;

	public Buffer<LocalVariable> localVariables = new Buffer<LocalVariable>(256);
	public int scopeDepth;

	public Buffer<ValueType> functionReturnTypeStack = new Buffer<ValueType>(4);

	public Buffer<LoopBreak> loopBreaks = new Buffer<LoopBreak>(4);
	public Buffer<Slice> loopNesting = new Buffer<Slice>(4);

	public CompilerIO()
	{
		void AddTokenizerError(Slice slice, string message, object[] args)
		{
			AddHardError(slice, message, args);
		}

		var tokenizer = new Tokenizer(TokenScanners.scanners);
		parser = new Parser(tokenizer, AddTokenizerError);
		Reset(null, Mode.Release, null, 0);
	}

	public void Reset(ByteCodeChunk chunk, Mode mode, string source, int sourceIndex)
	{
		this.mode = mode;
		this.sourceIndex = sourceIndex;
		parser.tokenizer.Reset(source);
		parser.Reset();

		errors.count = 0;

		isInPanicMode = false;
		this.chunk = chunk;
		localVariables.count = 0;
		scopeDepth = 0;
	}

	public CompilerIO AddSoftError(Slice slice, string format, params object[] args)
	{
		if (!isInPanicMode)
			errors.PushBack(new CompileError(sourceIndex, slice, string.Format(format, args)));
		return this;
	}

	public CompilerIO AddHardError(Slice slice, string format, params object[] args)
	{
		if (!isInPanicMode)
		{
			isInPanicMode = true;
			if (args == null || args.Length == 0)
				errors.PushBack(new CompileError(sourceIndex, slice, format));
			else
				errors.PushBack(new CompileError(sourceIndex, slice, string.Format(format, args)));
		}
		return this;
	}
}
