using System;
using UnityEngine;

namespace DeadLine
{
 
    public class EventPredicate : IPredicate
    {
        private bool _triggered = false;
        private string description;

        public EventPredicate(string description = null){
            this.description = description;
        }

        public bool Evaluate()
        {
            if (_triggered)
            {
                _triggered = false; 
                return true;
            }
            return false;
        }

        /// <summary>
        /// Llamar esto cuando el evento ocurra
        /// </summary>
        public void Trigger()
        {
            _triggered = true;
        }

        public void Reset()
        {
            _triggered = false;
        }

        public override string ToString()
        {
            return description;
        }
    }
}
