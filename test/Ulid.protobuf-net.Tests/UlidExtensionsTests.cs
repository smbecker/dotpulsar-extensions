using System.Text;

namespace ProtoBuf.Meta;

public class UlidExtensionsTests
{
	[Fact]
	public void can_serialize_and_deserialize_Ulid_types() {
		var model = RuntimeTypeModel.Create();
		model.AddUlid();
		model.Add<MyType>();

		var expectedId = Ulid.NewUlid();

		using var stream = new MemoryStream();
		model.Serialize(stream, new MyType {
			Id = expectedId
		});

		stream.Position = 0;
		var actual = model.Deserialize<MyType>(stream);
		Assert.NotNull(actual);
		Assert.Equal(expectedId, actual.Id);
	}

	[Fact]
	public void can_serialize_and_deserialize_as_string() {
		var model = RuntimeTypeModel.Create();
		model.AddUlidAsString();
		model.Add<MyType>();

		var expectedId = Ulid.NewUlid();

		using var stream = new MemoryStream();
		model.Serialize(stream, new MyType {
			Id = expectedId
		});

		stream.Position = 0;
		var actual = model.Deserialize<MyType>(stream);
		Assert.NotNull(actual);
		Assert.Equal(expectedId, actual.Id);

		stream.Position = 0;
		Assert.Equal(expectedId.ToString(), Encoding.UTF8.GetString(stream.ToArray(), 4, 26));
	}

	[Fact]
	public void can_serialize_and_deserialize_as_guid_string() {
		var model = RuntimeTypeModel.Create();
		model.AddUlidAsGuidString();
		model.Add<MyType>();

		var expectedId = Ulid.NewUlid();

		using var stream = new MemoryStream();
		model.Serialize(stream, new MyType {
			Id = expectedId
		});

		stream.Position = 0;
		var actual = model.Deserialize<MyType>(stream);
		Assert.NotNull(actual);
		Assert.Equal(expectedId, actual.Id);

		stream.Position = 0;
		Assert.Equal(expectedId.ToGuid().ToString(), Encoding.UTF8.GetString(stream.ToArray(), 4, 36));
	}

	[ProtoContract]
	internal sealed class MyType
	{
		[ProtoMember(1)]
		public required Ulid Id {
			get;
			init;
		}
	}
}
