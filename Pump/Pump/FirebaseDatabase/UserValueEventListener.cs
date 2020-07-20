using System;

namespace Pump.FirebaseDatabase
{
    internal class UserValueEventListener
    {
        private Action<object, object> p;

        public UserValueEventListener(Action<object, object> p)
        {
            this.p = p;
        }
    }
}