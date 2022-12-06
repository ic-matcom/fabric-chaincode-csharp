using System.Collections.Concurrent;
using Google.Protobuf;
using Protos;

namespace Shim
{
    /// <summary>
    /// This class handles queuing messages to be sent to the peer based on transaction id
    /// The peer can access requests coming from different transactions concurrently but
    /// cannot handle concurrent requests for the same transaction.Given the nature of asynchronouse
    /// programming this could present a problem so this implementation provides a way to allow
    /// code to perform concurrent request by serialising the calls to the peer.
    /// </summary>
    public class MessageQueue : IMessageQueue
{
    private readonly IHandler _handler;

    private readonly ConcurrentDictionary<string, ConcurrentQueue<QueueMessage>> _txQueues =
        new ConcurrentDictionary<string, ConcurrentQueue<QueueMessage>>();

    public ILogger _logger { get; set; }
    public MessageQueue(IHandler handler, ILogger logger)
    {
        _handler = handler;
        _logger = logger;
    }

        /// <summary>
        /// Queue a message to be sent to the peer. If it is the first
        /// message on the queue then send the message to the peer
        /// </summary>
        /// <param name="queueMessage">the message to queue</param>
        public Task QueueMessage(QueueMessage queueMessage)
    {
        var messageQueue = _txQueues.GetOrAdd(queueMessage.MessageTxContextId, new ConcurrentQueue<QueueMessage>());

        messageQueue.Enqueue(queueMessage);

        if (messageQueue.Count == 1) return SendMessage(queueMessage.MessageTxContextId);

        return Task.CompletedTask;
    }

        /// <summary>
        /// Handle a response to a message. This looks at the top of
        /// the queue for the specific txn id to get the message this
        /// response is associated with so it can drive the task waiting
        /// on this message response. It then removes that message from the
        /// queue and sends the next message on the queue if there is one.
        /// </summary>
        /// <param name="response">the received response</param>
        public void HandleMessageResponse(ChaincodeMessage response)
    {
        var messageTxContextId = response.ChannelId + response.Txid;

        var message = GetCurrentMessage(messageTxContextId) as dynamic;

        HandleResponseMessage(message, messageTxContextId, response);
    }

        /// <summary>
        /// send the current message to the peer.
        /// </summary>
        /// <param name="messageTxContextId">the transaction context id</param>
        private Task SendMessage(string messageTxContextId)
    {
        var message = GetCurrentMessage(messageTxContextId);

        if (message == null) return Task.CompletedTask;

        try
        {
            return _handler.ResponseStream.WriteAsync(message.Message);
        }
        catch (Exception ex)
        {
            message.Fail(ex);
            return Task.CompletedTask;
        }
        }

        /// <summary>
        /// Get the current message.
        /// this returns the message at the top of the queue for the particular transaction.
        /// </summary>
        /// <param name="messageTxContextId">the transaction context id</param>
        private QueueMessage GetCurrentMessage(string messageTxContextId)
    {
        if (_txQueues.TryGetValue(messageTxContextId, out var messageQueue) &&
            messageQueue.TryPeek(out var message))
            return message;

       _logger.LogDebug($"Failed to find a message for transaction context id {messageTxContextId}");
        return null;
    }

        /// <summary>
        /// Remove the current message and send the next message in the queue if there is one.
        ///delete the queue if there are no more messages.
        /// </summary>
        /// <param name="messageTxContextId">the transaction context id</param>
        private void RemoveCurrentAndSendNextMessage(string messageTxContextId)
    {
        if (!_txQueues.TryGetValue(messageTxContextId, out var messageQueue) || messageQueue.Count <= 0) return;

        messageQueue.TryDequeue(out _);

        if (messageQueue.Count == 0)
        {
            _txQueues.TryRemove(messageTxContextId, out _);
            return;
        }

        SendMessage(messageTxContextId);
    }
    private void HandleResponseMessage<T>(
        QueueMessage<T> message,
        string messageTxContextId,
        ChaincodeMessage response
    )
    {
        try
        {
            var parsedResponse = _handler.ParseResponse(response, message.Method);

            message.Success((T) parsedResponse);
        }
        catch (Exception ex)
        {
            message.Fail(ex);
        }

        RemoveCurrentAndSendNextMessage(messageTxContextId);
    }
}
}
