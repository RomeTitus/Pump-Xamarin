using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.SocketController.BT;

namespace Pump.SocketController
{
    class SocketPicker
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly InitializeFirebase _initializeFirebase;
        private readonly InitializeBlueTooth _initializeBlueTooth;

        public SocketPicker(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            _initializeFirebase = new InitializeFirebase(_observableIrrigation);
            _initializeBlueTooth = new InitializeBlueTooth(_observableIrrigation);
        }

        public void Disposable()
        {
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();

            if(pumpConnection.ConnectionType == 0)
                _initializeFirebase.Disposable();
            if (pumpConnection.ConnectionType == 2)
                _initializeBlueTooth.Disposable();
        }

        public async Task Subscribe()
        {
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            if (pumpConnection.ConnectionType == 0)
                _initializeFirebase.SubscribeFirebase();
            if (pumpConnection.ConnectionType == 2)
                await _initializeBlueTooth.SubscribeFirebase();
            //Prepare for new connection subscription

        }
    }
}
