using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProtoBuf.Meta;

public static class UlidExtensions
{
	private const string Name = ".ulid.Ulid";
	private const string Origin = "ulid.proto";

	public static RuntimeTypeModel AddUlid(this RuntimeTypeModel? model) {
		model ??= RuntimeTypeModel.Default;
		Add<Ulid>(model);
		Add<Ulid?>(model);
		return model;

		static void Add<T>(RuntimeTypeModel model) {
			var metaModel = model.Add<T>(false);
			metaModel.Origin = Origin;
			metaModel.Name = Name;
			metaModel.SetSurrogate(typeof(UlidSurrogate));
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 16)]
	[ProtoContract(Name = Name, Origin = Origin)]
	internal struct UlidSurrogate
	{
		[ProtoMember(1, Name = "upper", DataFormat = DataFormat.FixedSize), FieldOffset(0)]
		private readonly ulong upper;

		[ProtoMember(2, Name = "lower", DataFormat = DataFormat.FixedSize), FieldOffset(8)]
		private readonly ulong lower;

		public UlidSurrogate(Ulid value) {
			this = Unsafe.As<Ulid, UlidSurrogate>(ref value);
		}

		public Ulid ToUlid() {
			return Unsafe.As<UlidSurrogate, Ulid>(ref this);
		}

		public static implicit operator Ulid(UlidSurrogate surrogate) {
			return surrogate.ToUlid();
		}

		public static implicit operator Ulid?(UlidSurrogate? surrogate) {
			if (surrogate is null) {
				return null;
			}

			return surrogate.Value.ToUlid();
		}

		public static implicit operator UlidSurrogate(Ulid source) {
			return new UlidSurrogate(source);
		}

		public static implicit operator UlidSurrogate?(Ulid? source) {
			if (source is null) {
				return null;
			}

			return new UlidSurrogate(source.Value);
		}
	}
}
