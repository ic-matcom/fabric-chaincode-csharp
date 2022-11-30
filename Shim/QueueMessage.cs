
using Protos;

namespace Shim
{

    /// <summary>
    /// Simple class to represent a message to be queued
    /// </summary>
    public class QueueMessage
    {
        public QueueMessage(ChaincodeMessage message, MessageMethod method)
        {
            Message = message;
            Method = method;
        }

        public ChaincodeMessage Message { get; }
        public string MessageTxContextId => Message.ChannelId + Message.Txid;
        public MessageMethod Method { get; }

        public virtual void Fail(Exception exception)
        {
            throw exception;
        }
    }

    /// <summary>
    /// Generic class to represent a message that has been sent to the peer and 
    /// it contains a Task which is waiting for a response of type T
    /// </summary>
    /// <typeparam name="T">Generic response type T</typeparam>
    public class QueueMessage<T> : QueueMessage
    {
        private readonly TaskCompletionSource<T> _taskCompletionSource;

        public QueueMessage(
            ChaincodeMessage message,
            MessageMethod method,
            TaskCompletionSource<T> taskCompletionSource
        )
            : base(message, method)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        /// <summary>
        /// Set result to unblock task
        /// </summary>
        /// <param name="result">response returned from the peer to set as task result</param>
        public void Success(T result)
        {
            _taskCompletionSource.SetResult(result);
        }

        /// <summary>
        /// Unblock task by throwing exception
        /// </summary>
        public override void Fail(Exception exception)
        {
            _taskCompletionSource.SetException(exception);
        }
    }
}
