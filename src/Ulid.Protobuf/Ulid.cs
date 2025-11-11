using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Google.Protobuf.WellKnownTypes;

partial class Ulid : ICustomDiagnosticMessage
{
	public Ulid(System.Ulid value) {
		_surrogate = new UlidSurrogate(value);
	}

	public System.Ulid ToUlid() {
		return _surrogate.ToUlid();
	}

	public Guid ToGuid() {
		return _surrogate.ToUlid().ToGuid();
	}

	string ICustomDiagnosticMessage.ToDiagnosticString() => $"\"{ToUlid().ToString()}\"";

	public static implicit operator System.Ulid(Ulid? surrogate) {
		if (surrogate == null) {
			return System.Ulid.Empty;
		}
		return surrogate.ToUlid();
	}

	public static implicit operator Ulid(System.Ulid source) {
		return new Ulid(source);
	}

	[StructLayout(LayoutKind.Explicit, Size = 16)]
	private struct UlidSurrogate
	{
		[FieldOffset(0)]
		internal ulong Upper;

		[FieldOffset(8)]
		internal ulong Lower;

		public UlidSurrogate(System.Ulid value) {
			this = Unsafe.As<System.Ulid, UlidSurrogate>(ref value);
		}

		public System.Ulid ToUlid() {
			return Unsafe.As<UlidSurrogate, System.Ulid>(ref this);
		}
	}
}
