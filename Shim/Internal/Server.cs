using Grpc.AspNetCore;
using System.Timers;

namespace Internal
{
    public class Server
    {
        private TimeSpan ServerInterval { get => TimeSpan.FromHours(2); }
        private TimeSpan ServerTimeout { get => TimeSpan.FromSeconds(20);}
        private TimeSpan ServerMinInterval { get => TimeSpan.FromMinutes(1); }
        private TimeSpan ConnectionTimeout { get => TimeSpan.FromSeconds(5); }
        public Server()
        {
            
        }

        
        public void Start()
        {
        }

        public void Stop() { }

        //public void NewServer() { }
    }
}