using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace DwarfServer
{
    static class Responder
    {
        private static Logger.Logger logger = Logger.Logger.GetInstance();
        private static int responses_cntr = 0;

        public static void Respond(Server.ConnectionInfo conn_info)
        {
            ++responses_cntr;
            string resp = "DwarfDB response #"+
                responses_cntr.ToString()+
                " was received!";
            int content_length = resp.Length;

            string resp_main = "HTTP/1.1 200 OK\r\n" +
                                "Content-Length: " + content_length.ToString() + "\r\n" +
                                "Content-Type: text/html\r\n\r\n" +
                                resp;

            logger.WriteEntry("Sending a response: " + resp_main);
            conn_info.socket.Send(Encoding.ASCII.GetBytes(resp_main));
            logger.WriteEntry("READY");
        }
    }
}
