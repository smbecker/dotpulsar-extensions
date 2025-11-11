using ProtoBuf;
using ProtoBuf.Meta;

namespace Google.Protobuf.WellKnownTypes;

public class UlidTests
{
	[Fact]
	public void can_serialize_and_deserialize_Ulid_types() {
		var expectedId = System.Ulid.NewUlid();

		using var stream = new MemoryStream();
		new Consumer {
			Id = expectedId
		}.WriteTo(stream);

		stream.Position = 0;
		var actual = Consumer.Parser.ParseFrom(stream);
		Assert.NotNull(actual);
		Assert.Equal(expectedId, actual.Id.ToUlid());
	}

	[Fact]
	public void can_interoperate_with_protobuf_net() {
		var model = RuntimeTypeModel.Create();
		model.AddUlid();
		model.Add<InteropType>();

		var expectedId = System.Ulid.NewUlid();

		using var stream = new MemoryStream();
		new Consumer {
			Id = expectedId
		}.WriteTo(stream);

		stream.Position = 0;
		var interopType = model.Deserialize<InteropType>(stream);
		Assert.NotNull(interopType);
		Assert.Equal(expectedId, interopType.Id);

		stream.SetLength(0);
		model.Serialize(stream, new InteropType {
			Id = expectedId
		});

		stream.Position = 0;
		var consumer = Consumer.Parser.ParseFrom(stream);
		Assert.NotNull(consumer);
		Assert.Equal(expectedId, consumer.Id.ToUlid());
	}

	[ProtoContract]
	internal sealed class InteropType
	{
		[ProtoMember(1)]
		public required System.Ulid Id {
			get;
			init;
		}
	}
}
