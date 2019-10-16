using System.Text;

public static class ClefInspectionExtensions
{
	public static string TraceCallStack(this CFlat self)
	{
		var vm = self.virtualMachine;
		var sb = new StringBuilder();

		sb.AppendLine("callstack:");
		for (var i = vm.callframeStack.count - 1; i >= 0; i--)
		{
			var callframe = vm.callframeStack.buffer[i];
			var codeIndex = callframe.codeIndex - 1;
			var sourceIndex = vm.chunk.sourceSlices.buffer[codeIndex].index;
			var source = self.sources.buffer[vm.chunk.FindSourceIndex(codeIndex)];

			switch (callframe.type)
			{
			case CallFrame.Type.EntryPoint:
				break;
			case CallFrame.Type.Function:
				var pos = CompilerHelper.GetLineAndColumn(
					source.content,
					sourceIndex,
					1
				);
				sb.Append("[line ");
				sb.Append(pos.line);
				sb.Append("] ");

				vm.chunk.FormatFunction(callframe.functionIndex, sb);

				sb.Append(" => ");
				var line = CompilerHelper.GetLines(
					source.content,
					pos.line - 1,
					pos.line - 1
				);
				sb.AppendLine(line.TrimStart());
				break;
			case CallFrame.Type.NativeFunction:
				sb.Append("[native function] ");
				vm.chunk.FormatNativeFunction(callframe.functionIndex, sb);
				sb.AppendLine();
				break;
			}
		}

		return sb.ToString();
	}

	public static string Disassemble(this CFlat self)
	{
		var sb = new StringBuilder();
		self.chunk.Disassemble(self.sources.buffer, sb);
		return sb.ToString();
	}
}