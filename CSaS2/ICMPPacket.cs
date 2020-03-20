using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSaS2
{
    //реализация ICMP-пакета
    public class ICMPPacket
    {
        const int hIP = 20;
        const int hICMP = 4;

        private byte[] mes = new byte[106];
        private int mSize;
        private ushort checkedsum = 0;

        public byte Type { set; get; }
        public byte Code { set; get; }
        public int PacketSize { set; get; }
        public byte[] Packet
        {
            get
            {
                byte[] temp = new byte[PacketSize];
                Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, temp, 0, 1);
                Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, temp, 1, 1);
                Buffer.BlockCopy(BitConverter.GetBytes(checkedsum), 0, temp, 2, 2);
                Buffer.BlockCopy(mes, 0, temp, hICMP, mSize);
                return temp;
            }
        }

        //на передачу
        public ICMPPacket(byte _type, byte _code, byte[] _data)
        {
            Type = _type;
            Code = _code;
            Buffer.BlockCopy(_data, 0, mes, hICMP, _data.Length);
            mSize = _data.Length + 4;               // идентификатор и номер последовательности - по 2 байта
            PacketSize = mSize + hICMP;
            checkedsum = getCheckedSum();
        }

        //на приём
        public ICMPPacket(byte[] data, int size)
        {
            Type = data[hIP];   
            Code = data[hIP + 1];   
            checkedsum = BitConverter.ToUInt16(data, hIP + 2);
            PacketSize = size - hIP;
            mSize = size - (hIP + hICMP);
            Buffer.BlockCopy(data, hIP + hICMP, mes, 0, mSize);
        }


        private ushort getCheckedSum()
        {
            UInt32 CheckedSum = 0;
            byte[] data = Packet;

            for (int i = 0; i < PacketSize; i += 2)
            {
                CheckedSum += Convert.ToUInt32(BitConverter.ToUInt16(data, i));
            }
            CheckedSum = (CheckedSum >> 16) + (CheckedSum & 0xFFFF);
            CheckedSum += (CheckedSum >> 16);     //дополняем сумму
            return (ushort)(~CheckedSum);
        }
    }
}
