using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User1
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncUser1 u1 = new AsyncUser1();
            u1.StartListenning();
        }
    }
}
