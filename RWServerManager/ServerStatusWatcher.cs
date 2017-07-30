using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RWServerManager
{
    public delegate void ServerStatusDelegate(ServerInfo info);
    public class ServerStatusWatcher
    {
        private bool _isrunning = false;
        private string _ip;
        private int _port;
        private Thread _myThread;
        public event ServerStatusDelegate OnServerStatus;

        public ServerStatusWatcher(string ip, int port)
        {
            this._ip = ip;
            this._port = port;
        }

        public void Start()
        {
            if(_myThread != null)
            {
                _isrunning = false;
                _myThread.Abort();
                _myThread.Join();
                _myThread = null;
            }

            _myThread = new Thread(new ThreadStart(DoWork));
            _myThread.Start();
        }

        public void Stop()
        {
            _isrunning = false;
            if(_myThread != null)
            {
                _myThread.Abort();
                _myThread.Join();
            }
        }

        private void DoWork()
        {
            _isrunning = true;
            while (_isrunning)
            {
                ServerInfo info = ServerInfo.GetServerInfo(this._ip, this._port);
                if (info != null)
                    info.IsOnline = true;
                else
                {
                    info = new ServerInfo();
                    info.HiveProt = false;
                    info.IsOnline = false;
                    info.Locked = false;
                    info.Lua = false;
                    info.MaxPlayerCount = 0;
                    info.PlayerCount = 0;
                    info.PvP = false;
                    info.ServerIp = "";
                    info.ServerName = "";
                    info.ServerPort = 0;
                    info.Verification = false;
                    info.Version = "";
                    info.WhiteList = false;
                }
                if (info.ServerIp.ToLower().Equals("null")) info.ServerIp = "localhost";
                if (OnServerStatus != null) OnServerStatus(info);
                Thread.Sleep(500);
            }
            _isrunning = false;
        }
    }
}
