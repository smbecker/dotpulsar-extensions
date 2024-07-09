using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ProtoBuf.Meta;

public static class UlidExtensions
{
	private const string Name = ".ulid.Ulid";
	private const string Origin = "ulid.proto";

	public static RuntimeTypeModel AddUlid(this RuntimeTypeModel? model) => model.AddUlidSurrogate<UlidSurrogate>();

	public static RuntimeTypeModel AddUlidAsString(this RuntimeTypeModel? model) => model.AddUlidSurrogate<UlidStringSurrogate>();

	public static RuntimeTypeModel AddUlidAsGuidString(this RuntimeTypeModel? model) => model.AddUlidSurrogate<GuidStringSurrogate>();

	private static RuntimeTypeModel AddUlidSurrogate<TSurrogate>(this RuntimeTypeModel? model) {
		model ??= RuntimeTypeModel.Default;
		Add<Ulid>(model);
		Add<Ulid?>(model);
		return model;

		static void Add<T>(RuntimeTypeModel model) {
			var metaModel = model.Add<T>(false);
			metaModel.Origin = Origin;
			metaModel.Name = Name;
			metaModel.SetSurrogate(typeof(TSurrogate));
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

	[ProtoContract(Name = Name, Origin = Origin)]
	internal struct UlidStringSurrogate
	{
		[ProtoMember(1, Name = "value")]
		private readonly string value;

		public UlidStringSurrogate(Ulid value) {
			this.value = value.ToString();
		}

		public Ulid ToUlid() {
			return Ulid.Parse(value, CultureInfo.InvariantCulture);
		}

		public static implicit operator Ulid(UlidStringSurrogate surrogate) {
			return surrogate.ToUlid();
		}

		public static implicit operator Ulid?(UlidStringSurrogate? surrogate) {
			if (surrogate is null) {
				return null;
			}

			return surrogate.Value.ToUlid();
		}

		public static implicit operator UlidStringSurrogate(Ulid source) {
			return new UlidStringSurrogate(source);
		}

		public static implicit operator UlidStringSurrogate?(Ulid? source) {
			if (source is null) {
				return null;
			}

			return new UlidStringSurrogate(source.Value);
		}
	}

	[ProtoContract(Name = Name, Origin = Origin)]
	internal struct GuidStringSurrogate
	{
		[ProtoMember(1, Name = "value")]
		private readonly string value;

		public GuidStringSurrogate(Ulid value) {
			this.value = value.ToGuid().ToString();
		}

		public Ulid ToUlid() {
			return new Ulid(Guid.Parse(value));
		}

		public static implicit operator Ulid(GuidStringSurrogate surrogate) {
			return surrogate.ToUlid();
		}

		public static implicit operator Ulid?(GuidStringSurrogate? surrogate) {
			if (surrogate is null) {
				return null;
			}

			return surrogate.Value.ToUlid();
		}

		public static implicit operator GuidStringSurrogate(Ulid source) {
			return new GuidStringSurrogate(source);
		}

		public static implicit operator GuidStringSurrogate?(Ulid? source) {
			if (source is null) {
				return null;
			}

			return new GuidStringSurrogate(source.Value);
		}
	}
}
