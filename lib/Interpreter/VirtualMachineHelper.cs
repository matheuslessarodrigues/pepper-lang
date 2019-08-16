using System.Text;

public static class VirtualMachineHelper
{
	public static void Return(VirtualMachine vm, int size)
	{
		vm.callframeStack.count -= 1;
		var stackTop = vm.callframeStack.buffer[vm.callframeStack.count].baseStackIndex - 1;

		var dstIdx = stackTop;
		var srcIdx = vm.valueStack.count - size;

		while (srcIdx < vm.valueStack.count)
			vm.valueStack.buffer[dstIdx++] = vm.valueStack.buffer[srcIdx++];

		stackTop += size;
		vm.valueStack.count = stackTop;
	}

	public static void ValueToString(VirtualMachine vm, int index, ValueType type, StringBuilder sb)
	{
		switch (type.kind)
		{
		case TypeKind.Unit:
			sb.Append("{}");
			return;
		case TypeKind.Bool:
			sb.Append(vm.valueStack.buffer[index].asBool ? "true" : "false");
			return;
		case TypeKind.Int:
			sb.Append(vm.valueStack.buffer[index].asInt);
			return;
		case TypeKind.Float:
			sb.Append(vm.valueStack.buffer[index].asFloat);
			return;
		case TypeKind.String:
			{
				var idx = vm.valueStack.buffer[index].asInt;
				sb.Append('"');
				sb.Append(vm.heap.buffer[idx]);
				sb.Append('"');
				return;
			}
		case TypeKind.Function:
			{
				var idx = vm.valueStack.buffer[index].asInt;
				vm.chunk.FormatFunction(idx, sb);
				return;
			}
		case TypeKind.NativeFunction:
			{
				var idx = vm.valueStack.buffer[index].asInt;
				sb.Append("native ");
				vm.chunk.FormatNativeFunction(idx, sb);
				return;
			}
		case TypeKind.Struct:
			{
				var structType = vm.chunk.structTypes.buffer[type.index];
				sb.Append(structType.name);
				sb.Append('{');
				var offset = 0;
				for (var i = 0; i < structType.fields.length; i++)
				{
					var fieldIndex = structType.fields.index + i;
					var field = vm.chunk.structTypeFields.buffer[fieldIndex];
					sb.Append(field.name);
					sb.Append('=');
					ValueToString(vm, index + offset, field.type, sb);
					if (i < structType.fields.length - 1)
						sb.Append(' ');

					offset += field.type.GetSize(vm.chunk);
				}
				sb.Append('}');
				return;
			}
		case TypeKind.Custom:
			{
				var idx = vm.valueStack.buffer[index].asInt;
				var obj = vm.heap.buffer[idx];
				sb.Append("CustomType [");
				sb.Append(obj.GetType().Name);
				sb.Append("] ");
				sb.Append(obj);
				return;
			}
		default:
			sb.Append("<invalid type '");
			sb.Append(type);
			sb.Append("'>");
			return;
		}
	}

	public static void TraceStack(VirtualMachine vm, StringBuilder sb)
	{
		sb.Append("          ");
		for (var i = 0; i < vm.valueStack.count;)
		{
			sb.Append("[");
			//var type = vm.typeStack.buffer[i];
			var type = new ValueType(TypeKind.Unit);
			ValueToString(vm, i, type, sb);
			i += type.GetSize(vm.chunk);
			sb.Append("]");
		}
		sb.AppendLine();
	}

	public static string FormatError(string source, RuntimeError error, int contextSize, int tabSize)
	{
		var sb = new StringBuilder();

		var position = CompilerHelper.GetLineAndColumn(source, error.slice.index, tabSize);

		sb.Append(error.message);
		sb.Append(" (line: ");
		sb.Append(position.line);
		sb.Append(", column: ");
		sb.Append(position.column);
		sb.AppendLine(")");

		sb.Append(CompilerHelper.GetLines(
			source,
			System.Math.Max(position.line - contextSize, 0),
			System.Math.Max(position.line - 1, 0)
		));
		sb.AppendLine();
		sb.Append(' ', position.column - 1);
		sb.Append('^', error.slice.length > 0 ? error.slice.length : 1);
		sb.Append(" here\n");

		return sb.ToString();
	}
}