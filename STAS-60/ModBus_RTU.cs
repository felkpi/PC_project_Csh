using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

class HoldingRegisters
{
    protected SerialPort port = new SerialPort(); // екземпляр класу SerialPort для роботи з СОМ портами
    protected CRC crc16 = new CRC(); // екземпляр класу підрахунку контрольної суми пакету данних

    protected UInt16[] rezultat;

    // біти данних
    protected byte[] msg = new byte[8];
    protected byte[] startAdress;// = BitConverter.GetBytes(Convert.ToUInt16(StartAddr));
    protected byte[] countWord;// = BitConverter.GetBytes(Convert.ToUInt16(numberReg));
    protected UInt16 CRC_16;
    protected byte[] CRC16;
    protected byte[] buff;

    public HoldingRegisters(SerialPort port)
    {
        this.port=port;
    }

    // RC=3 Зчитування холдингових регітрів
    public virtual UInt16[] readRtu(Int32 ModBusAddr, Int32 StartAddr, Int32 numberReg, Int32 ReadTimeOut)
    {


        msg[0]=Convert.ToByte(ModBusAddr);
        msg[1]=Convert.ToByte(3);
        startAdress=BitConverter.GetBytes(Convert.ToUInt16(StartAddr));
        msg[2]=startAdress[1];
        msg[3]=startAdress[0];
        countWord=BitConverter.GetBytes(Convert.ToUInt16(numberReg));
        msg[4]=countWord[1];
        msg[5]=countWord[0];

        CRC_16=crc16.ModRTU(msg, 6);
        CRC16=BitConverter.GetBytes(CRC_16);
        msg[6]=CRC16[0];
        msg[7]=CRC16[1];
        port.Write(msg, 0, msg.Length);

        Thread.Sleep(ReadTimeOut);

        buff=new byte[port.BytesToRead];
        if (buff.Length<6)
        {
            rezultat=new UInt16[numberReg];
            for (int i = 0; i<numberReg; i++)
            {
                rezultat[i]=0;
            }
        }
        else
        {
            port.Read(buff, 0, buff.Length);
            int buffLenth = buff.Length;
            rezultat=new UInt16[buffLenth/2];

            for (int i = 0; i<(buff.Length/2)-2; i++)
            {
                rezultat[i]=Convert.ToUInt16((buff[2*i+3]<<8)+buff[2*i+4]);
            }
        }

        return rezultat;

    }

    // RC=6 Запис одного холдингового ригістру
    public void writeRtu(Int32 ModBusAddr, Int32 addrWriteRegister, Int32 valueRegisters, Int32 ReadTimeOut)
    {
        msg[0]=Convert.ToByte(ModBusAddr);
        msg[1]=Convert.ToByte(6);
        startAdress=BitConverter.GetBytes(Convert.ToUInt16(addrWriteRegister));
        msg[2]=startAdress[1];
        msg[3]=startAdress[0];
        countWord=BitConverter.GetBytes(Convert.ToUInt16(valueRegisters));
        msg[4]=countWord[1];
        msg[5]=countWord[0];

        CRC_16=crc16.ModRTU(msg, 6);
        CRC16=BitConverter.GetBytes(CRC_16);
        msg[6]=CRC16[0];
        msg[7]=CRC16[1];
        port.Write(msg, 0, msg.Length);

        Thread.Sleep(ReadTimeOut);

        buff=new byte[port.BytesToRead];
        port.Read(buff, 0, buff.Length);
        int buffLenth = buff.Length;
        rezultat=new UInt16[buffLenth/2];

        for (int i = 0; i<(buff.Length/2)-2; i++)
        {
            rezultat[i]=Convert.ToUInt16((buff[2*i+3]<<8)+buff[2*i+4]);
        }

    }

    // RC=16 Запис багатьох холдингових регістрів
    public void writeMultiRtu(Int32 ModBusAddr, Int32 starAddrWriteRegister, Int32 countRegisters, ushort[] registersDOS, Int32 ReadTimeOut)
    {
        byte[] msg_Multi = new byte[9+2*countRegisters];
        msg_Multi[0]=Convert.ToByte(ModBusAddr);
        msg_Multi[1]=Convert.ToByte(16);
        startAdress=BitConverter.GetBytes(Convert.ToUInt16(starAddrWriteRegister));
        msg_Multi[2]=startAdress[1];
        msg_Multi[3]=startAdress[0];
        countWord=BitConverter.GetBytes(countRegisters);
        msg_Multi[4]=countWord[1];
        msg_Multi[5]=countWord[0];
        msg_Multi[6]=Convert.ToByte(2*countRegisters);

        for (int i = 0; i<countRegisters; i++)
        {
            byte[] dataSend = BitConverter.GetBytes(registersDOS[i]);
            msg_Multi[7+2*i]=dataSend[1];
            msg_Multi[8+2*i]=dataSend[0];
        }

        UInt16 qwerty = crc16.ModRTU(msg_Multi, (7+countRegisters*2));
        byte[] CRC16 = BitConverter.GetBytes(qwerty);
        msg_Multi[9+(countRegisters-1)*2]=CRC16[0];
        msg_Multi[10+(countRegisters-1)*2]=CRC16[1];
        port.Write(msg_Multi, 0, msg_Multi.Length);

        Thread.Sleep(ReadTimeOut);

        buff=new byte[port.BytesToRead];
        port.Read(buff, 0, buff.Length);
        int buffLenth = buff.Length;
        rezultat=new UInt16[buffLenth/2];

        for (int i = 0; i<(buff.Length/2)-2; i++)
        {
            rezultat[i]=Convert.ToUInt16((buff[2*i+3]<<8)+buff[2*i+4]);
        }
    }


}

/// <summary>
///Вхідні регістри
/// </summary>
class InputRegisters : HoldingRegisters
{

    public InputRegisters(SerialPort port) : base(port) { }

    // Зчитування вхідних регістрів
    public override ushort[] readRtu(int ModBusAddr, int StartAddr, int numberReg, int ReadTimeOut) // Зчитування вхідних регістрів
    {
        msg[0]=Convert.ToByte(ModBusAddr);
        msg[1]=Convert.ToByte(4);
        startAdress=BitConverter.GetBytes(Convert.ToUInt16(StartAddr));
        msg[2]=startAdress[1];
        msg[3]=startAdress[0];
        countWord=BitConverter.GetBytes(Convert.ToUInt16(numberReg));
        msg[4]=countWord[1];
        msg[5]=countWord[0];

        CRC_16=crc16.ModRTU(msg, 6);
        CRC16=BitConverter.GetBytes(CRC_16);
        msg[6]=CRC16[0];
        msg[7]=CRC16[1];
        port.Write(msg, 0, msg.Length);

        Thread.Sleep(ReadTimeOut);

        buff=new byte[port.BytesToRead];

        if (buff.Length<6)
        {
            rezultat=new UInt16[numberReg];
            for (int i = 0; i<numberReg; i++)
            {
                rezultat[i]=0;
            }
        }

        else
        {

            port.Read(buff, 0, buff.Length);
            int buffLenth = buff.Length;
            rezultat=new UInt16[buffLenth/2];

            for (int i = 0; i<(buff.Length/2)-2; i++)
            {
                rezultat[i]=Convert.ToUInt16((buff[2*i+3]<<8)+buff[2*i+4]);
            }
        }

        return rezultat;
    }


}

/// <summary>
/// Контрольна сума
/// </summary>
class CRC 
{
    public UInt16 ModRTU(byte[] buf, int len)
    {
        UInt16 crc = 0xFFFF;

        for (int pos = 0; pos<len; pos++)
        {
            crc^=(UInt16) buf[pos];
            for (int i = 8; i!=0; i--)
            {
                if ((crc&0x0001)!=0)
                {
                    crc>>=1;
                    crc^=0xA001;
                }
                else
                    crc>>=1;
            }
        }
        return crc;
    }

}