using Google.Protobuf;
using Protos;

namespace Shim
{
   /// <summary>
   /// Handle response messages back to the peer.
   /// </summary>
    public static class Shim
    {
        public static Response Success()
        {
            return Success(ByteString.Empty);
        }

        public static Response Success(ByteString payload)
        {
            return new Response
            {
                Status = (int)ResponseCodes.Ok,
                Payload = payload
            };
        }

        public static Response Error(string message)
        {
            return new Response
            {
                Status = (int)ResponseCodes.Error,
                Message = message
            };
        }

        public static Response Error(Exception exception)
        {
            return Error(exception.ToString());
        }
    }
}
