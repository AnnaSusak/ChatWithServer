using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWithServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncServer server = new AsyncServer();
            server.StartListenning();
        }
    }
}
