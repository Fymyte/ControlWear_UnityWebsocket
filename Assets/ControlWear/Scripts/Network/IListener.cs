using System;

namespace Ohrizon.ControlWear.Network
{
    public interface IListener
    {
        /// <summary>
        /// Event fired when a message is received from the listener
        /// </summary>
        public event Action<string> MessageReceived;
        /// <summary>
        /// Event fired when a connection is accepted by the listener
        /// </summary>
        public event Action<string> ClientConnected;
        /// <summary>
        /// Event fired when the client closes the connection
        /// </summary>
        public event Action<string> ClientDisconnected;
        
        /// <summary>
        /// Start listening for incoming connection in a separated thread.
        /// </summary>
        public void Listen();

        /// <summary>
        /// Close resources for listening and associated thread.
        /// </summary>
        public void Cancel();
    }
}
