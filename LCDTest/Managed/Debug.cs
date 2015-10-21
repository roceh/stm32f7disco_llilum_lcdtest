using System;
using System.IO.Ports;
using Microsoft.Zelig.Runtime;

namespace Managed
{
    public class Debug
    {
        private static volatile Debug _instance;
        private static object _syncRoot = new Object();

        public static Debug Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Debug();
                    }
                }

                return _instance;
            }
        }

        private SerialStream _port;

        public Debug()
        {
            var cnf = new BaseSerialStream.Configuration("UART1");
            _port = SerialPortsManager.Instance.Open(ref cnf);
        }

        public void Log(string log)
        {
            for (int i = 0; i < log.Length; i++)
            {
                _port.WriteByte((byte) log[i]);
            }

            _port.WriteByte((byte)13);
            _port.WriteByte((byte)10);
        }
    }
}
