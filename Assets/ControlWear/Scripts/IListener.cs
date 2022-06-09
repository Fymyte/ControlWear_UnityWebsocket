using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.ohrizon.ControlWear
{
    public interface IListener
    {
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
