using System.Threading.Channels;
using Core.Common.Messages;

namespace Infrastructure.Messaging;

internal sealed class MessageChannelRegistry
{
    private readonly Dictionary<string, Channel<BaseMessage>> _channels = [];

    internal ChannelWriter<BaseMessage> GetWriter(string queueName) =>
        GetOrCreate(queueName).Writer;

    internal ChannelReader<BaseMessage> GetReader(string queueName) =>
        GetOrCreate(queueName).Reader;

    private Channel<BaseMessage> GetOrCreate(string queueName)
    {
        if (!_channels.TryGetValue(queueName, out var channel))
        {
            channel = Channel.CreateUnbounded<BaseMessage>(new UnboundedChannelOptions { SingleReader = true });
            _channels[queueName] = channel;
        }

        return channel;
    }
}
