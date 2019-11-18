using System.Collections.Specialized;

namespace cflat.debug
{
	internal static class DebugServerActions
	{
		public static void Help(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
			using (var root = writer.Object)
			{
				root.String("/", "show this help");

				root.String("/continue", "resume execution");
				root.String("/pause", "pause execution");
				root.String("/stop", "stop debug server");

				root.String("/breakpoints/all", "list all breakpoints of all sources");
				root.String("/breakpoints/clear", "clear all breakpoints of all sources");
				root.String("/breakpoints/set?source=dot.separated.source.uri&lines=1,2,42,999", "set all breakpoints for a source");

				root.String("/query/paused", "query if execution is paused");
				root.String("/query/all", "query all values on stack");
				root.String("/query/value?myvar.field", "query a value on stack");
			}
		}

		public static void Continue(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
			self.paused = false;
			self.QueryPaused(query, writer);
		}

		public static void Pause(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
			self.paused = true;
			self.QueryPaused(query, writer);
		}

		public static void BreakpointsAll(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
			using (var root = writer.Array)
			{
				for (var i = 0; i < self.breakpoints.count; i++)
				{
					var breakpoint = self.breakpoints.buffer[i];
					using (var b = root.Object)
					{
						b.String("source", breakpoint.uri.value);
						b.Number("line", breakpoint.line);
					}
				}
			}
		}

		public static void BreakpointsClear(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
			self.breakpoints.count = 0;
			self.BreakpointsAll(query, writer);
		}

		public static void BreakpointsSet(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
			var uri = query["source"].Replace('.', '/');
			if (string.IsNullOrEmpty(uri))
			{
				self.BreakpointsAll(query, writer);
				return;
			}

			var lines = query["lines"].Split(',');

			for (var i = self.breakpoints.count - 1; i >= 0; i--)
			{
				var breakpoint = self.breakpoints.buffer[i];
				if (breakpoint.uri.value == uri)
					self.breakpoints.SwapRemove(i);
			}

			foreach (var line in lines)
			{
				if (int.TryParse(line, out var lineNumber))
				{
					self.breakpoints.PushBack(new SourcePosition(
						new Uri(uri),
						(ushort)lineNumber
					));
				}
			}

			self.BreakpointsAll(query, writer);
		}

		public static void QueryPaused(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
			using (var root = writer.Object)
			{
				root.Boolean("paused", self.paused);
			}
		}

		public static void QueryAll(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
		}

		public static void QueryValue(this DebugServer self, NameValueCollection query, JsonWriter writer)
		{
		}
	}
}