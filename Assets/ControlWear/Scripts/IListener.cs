using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.ohrizon.ControlWear
{
    public interface IListener
    {
        /// <summary>
        /// Event fired when a message is received from the listener
        /// </summary>
        public event Action<string> MessageReceived; 
        
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
