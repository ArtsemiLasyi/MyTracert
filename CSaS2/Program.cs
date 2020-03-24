using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace CSaS2
{
    class Program
    {
        const byte HOPS_NUM = 30;           //максимальное число прыжков
        const byte PACKETS_NUM = 3;         //число пакетов
        const byte REQUEST_TYPE = 8;        //8 тип - эхо-запрос
        const byte RESPONSE_TYPE = 0;       //0 тип - эхо-ответ
        const byte EXPAND_TTL_TYPE = 11;    //11 тип - время жизни пакета истекло
        const int TIMEOUT = 1000;           //время ожидания в милисекундах

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Input the IP address or host address:");
                string address = Console.ReadLine();
                if ((isHost(address)) || (isIP(address)))
                {
                    IPAddress temp = getIP(address);

                    Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Raw, ProtocolType.Icmp);
                    IPEndPoint ipPoint = new IPEndPoint(temp, 0);
                    byte[] data = Encoding.ASCII.GetBytes("Just a message");
                    ICMPPacket packet = new ICMPPacket(REQUEST_TYPE, 0, data);
                    Trace(socket, packet, ipPoint);
                }
                else
                {
                    Console.WriteLine("ERROR!");
                }
            }
        }

        //Трассировка пути ICMP пакета
        static void Trace(Socket _socket, ICMPPacket _packet, IPEndPoint _ipPoint)
        {
            _socket.Connect(_ipPoint);
            Console.WriteLine("Tracing " + _ipPoint.Address.ToString() + "...");
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, TIMEOUT);
            for (byte i = 1; i < HOPS_NUM; i++)
            {
                Console.Write(i.ToString() + ". ");
                for(byte j = 1; j <= PACKETS_NUM; j++)
                {
                    int eCount = 0;
                    int eTime = 0;
                    IPEndPoint tempPoint = _ipPoint;
                    try
                    {
                        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, i);
                        byte type = sendAndReceive(_socket, _packet, ref tempPoint, ref eTime);
                        Console.Write(" " + eTime.ToString() + "ms ");

                        if (j == PACKETS_NUM)
                        {
                            Console.WriteLine(tempPoint.ToString());
                            if (type == RESPONSE_TYPE)
                            {
                                Console.WriteLine();
                                Console.WriteLine("TRACING WAS FINISHED SUCCESSFULLY!");
                                Console.WriteLine();
                                return;
                            }
                        }
                    }
                    catch (SocketException)
                    {
                        Console.Write("*\t");
                        eCount++;
                        if (eCount == HOPS_NUM)
                        {
                            Console.WriteLine();
                            Console.WriteLine("HOST IS UNREACHEABLE!");
                            Console.WriteLine();
                            return;
                        }
                    }
                }
            }
            _socket.Close();
        }


        static byte sendAndReceive(Socket _socket, ICMPPacket _packet,ref IPEndPoint _ipPoint, ref int eTime)
        {
            int sTime;
            int fTime;

            sTime = Environment.TickCount;

            _socket.SendTo(_packet.Packet, _packet.PacketSize, SocketFlags.None, _ipPoint);
            EndPoint tempPoint = _ipPoint;
            byte[] rPacket = new byte[106];
            int rSize = _socket.ReceiveFrom(rPacket, ref tempPoint);
            fTime = Environment.TickCount;
            ICMPPacket response = new ICMPPacket(rPacket, rSize);
            _ipPoint = (IPEndPoint)tempPoint;
            if ((response.Type == RESPONSE_TYPE) || (response.Type == EXPAND_TTL_TYPE))
                eTime = fTime - sTime;
            return response.Type;
        }

        static bool isHost(string test)
        {
            string ValidHostnameRegex = @"^[a-zA-Z0-9]*\.[a-z]*$";
            Regex regex = new Regex(ValidHostnameRegex);
            MatchCollection matches = regex.Matches(test);
            if (matches.Count > 0)
                return true;
            else
                return false;
 
        }

        static bool isIP(string test)
        {
            string ValidIpAddressRegex = @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}";
            Regex regex = new Regex(ValidIpAddressRegex);
            MatchCollection matches = regex.Matches(test);
            if (matches.Count > 0)
                return true;
            else
                return false;
        }

        static IPAddress getIP(string address)
        {
            IPAddress temp;
            if (isHost(address))
            {
                IPAddress[] ipaddress = Dns.GetHostAddresses(address);
                return ipaddress[0];
            }
            else
                return IPAddress.Parse(address);
        }
    }
}
