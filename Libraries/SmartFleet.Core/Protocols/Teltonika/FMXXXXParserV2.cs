using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SmartFleet.Core.Contracts.Commands;

namespace SmartFleet.Core.Protocols.Teltonika
{
    public class FMXXXXParserV2:IFMParserProtocol
    {
        private static string _IMEI;
        private const int CODEC_FMXXX = 0x08;

        private const int ACC = 1;
        private const int DOOR = 2;
        private const int Analog = 4;
        private const int GSM = 5;
        private const int SPEED = 6;
        private const int VOLTAGE = 7;
        private const int GPSPOWER = 8;
        private const int TEMPERATURE = 9;
        private const int ODOMETER = 16;
        private const int STOP = 20;
        private const int TRIP = 28;
        private const int IMMOBILIZER = 29;
        private const int AUTHORIZED = 30;
        private const int GREEDRIVING = 31;
        private const int OVERSPEED = 33;
        private const int CAN_2 = 147;
        public List<CreateTeltonikaGps> DecodeAvl(List<byte> byteBuffer, string imei)
        {
            _IMEI = imei;
            var result = ParsePositions(byteBuffer.ToArray());
           
            return result;
        }
        private static List<CreateTeltonikaGps> ParsePositions(Byte[] byteBuffer)
        {
            var index = 0;
            index += 7;
            uint dataSize = byteBuffer[index];

            index++;
            uint codecID = byteBuffer[index];

            if (codecID == CODEC_FMXXX)
            {
                index++;
                uint numberOfData = byteBuffer[index];

                Trace.WriteLine(string.Format("{0} {1} {2} ", codecID, numberOfData, dataSize));

                List<CreateTeltonikaGps> result = new List<CreateTeltonikaGps>();

                index++;
                for (int i = 0; i < numberOfData; i++)
                {
                    CreateTeltonikaGps position = new CreateTeltonikaGps();
                    position.Imei = _IMEI;
                    Trace.WriteLine(_IMEI);
                    var timestamp = Int64.Parse(Parsebytes(byteBuffer, index, 8), System.Globalization.NumberStyles.HexNumber);
                    index += 8;
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);

                    position.Timestamp = origin.AddMilliseconds(timestamp);

                    var Preority = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    position.Priority = Preority;
                    index++;

                    position.Long = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber) / 10000000.0;
                    index += 4;

                    position.Lat = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber) / 10000000.0;
                    index += 4;

                    var Altitude = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);
                    index += 2;
                    position.Altitude = Altitude;
                    var dir = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);

                    if (dir < 90) position.Direction = 1 ;// east
                    else if (dir == 90) position.Direction = 2;//east
                    else if (dir < 180) position.Direction = 3;// east
                    else if (dir == 180) position.Direction = 4;// ??
                    else if (dir < 270) position.Direction = 5;//west 
                    else if (dir == 270) position.Direction = 6;// west
                    else if (dir > 270) position.Direction = 7;//west
                    index += 2;

                    var satellite = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    index++;

                    if (satellite >= 3)
                        position.Status = "A";
                    else
                        position.Status = "L";
                    var speed = Parsebytes(byteBuffer, index, 2).ToString(CultureInfo.InvariantCulture);
                    byte x = Convert.ToByte(speed, 16);
                    position.Speed = Convert.ToDouble(x);
                    index += 2;

                    int ioEvent = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    index++;
                    int ioCount = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                    index++;
                    //read 1 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;
                            //Add output status
                            switch (id)
                            {
                                case ACC:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Status += value == 1 ? ",ACC off" : ",ACC on";
                                        index++;
                                        break;
                                    }
                                case DOOR:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Status += value == 1 ? ",door close" : ",door open";
                                        index++;
                                        break;
                                    }
                                case GSM:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Status += string.Format(",GSM {0}", value);
                                        index++;
                                        break;
                                    }
                                case STOP:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.StopFlag = value == 1;
                                        position.IsStop = value == 1;

                                        index++;
                                        break;
                                    }
                                case IMMOBILIZER:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        position.Alarm = value == 0 ? "Activate Anti-carjacking success" : "Emergency release success";
                                        index++;
                                        break;
                                    }
                                case GREEDRIVING:
                                    {
                                        var value = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    position.Alarm = "Acceleration intense !!";
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    position.Alarm = "Freinage brusque !!";
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    position.Alarm = "Virage serré !!";
                                                    break;
                                                }
                                            default:
                                                break;
                                        }
                                        index++;
                                        break;
                                    }
                                default:
                                    {
                                        index++;
                                        break;
                                    }
                            }

                        }
                    }

                    //read 2 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;



                            switch (id)
                            {
                                case Analog:
                                    {
                                        var value = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);
                                        if (value < 12)
                                            position.Alarm += string.Format("Low voltage", value);
                                        index += 2;
                                        break;
                                    }
                                case SPEED:
                                    {
                                        var value = Int16.Parse(Parsebytes(byteBuffer, index, 2), System.Globalization.NumberStyles.HexNumber);
                                        position.Alarm += string.Format("Speed", value);
                                        index += 2;
                                        break;
                                    }
                                default:
                                    {
                                        index += 2;
                                        break;
                                    }

                            }
                        }
                    }

                    //read 4 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;

                            switch (id)
                            {
                                case TEMPERATURE:
                                    {
                                        var value = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber);
                                        position.Temprature = value;
                                        position.Alarm += string.Format("Temperature {0}", value);
                                        index += 4;
                                        break;
                                    }
                                case ODOMETER:
                                    {
                                        var value = Int32.Parse(Parsebytes(byteBuffer, index, 4), System.Globalization.NumberStyles.HexNumber);
                                        position.Mileage = value;
                                        index += 4;
                                        break;
                                    }
                                default:
                                    {
                                        index += 4;
                                        break;
                                    }

                            }


                        }
                    }

                    //read 8 byte
                    {
                        int cnt = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                        index++;
                        for (int j = 0; j < cnt; j++)
                        {
                            int id = byte.Parse(Parsebytes(byteBuffer, index, 1), System.Globalization.NumberStyles.HexNumber);
                            index++;

                            var io = Int64.Parse(Parsebytes(byteBuffer, index, 8), System.Globalization.NumberStyles.HexNumber);
                            position.Status += string.Format(",{0} {1}", id, io);
                            index += 8;
                        }
                    }

                    result.Add(position);
                   // Console.WriteLine(position.ToString());
                }

                return result;
            }
            return null;
        }
        private static string Parsebytes(Byte[] byteBuffer, int index, int Size)
        {
            return BitConverter.ToString(byteBuffer, index, Size).Replace("-", string.Empty);
        }
    }
}
