using System;
using System.Net.Sockets;
using System.Text;


namespace ACNH_Chat
{
    class controller
    {
        private static Socket s;

        private static readonly Encoding Encoder = Encoding.UTF8;
        private static byte[] Encode(string command, bool addrn = true) => Encoder.GetBytes(addrn ? command + "\r\n" : command);
        private static byte[] X() => Encode("click X");
        private static byte[] pX() => Encode("press X");
        private static byte[] rX() => Encode("release X");
        private static byte[] Y() => Encode("click Y");
        private static byte[] pY() => Encode("press Y");
        private static byte[] rY() => Encode("release Y");
        private static byte[] A() => Encode("click A");
        private static byte[] pA() => Encode("press A");
        private static byte[] rA() => Encode("release A");
        private static byte[] B() => Encode("click B");
        private static byte[] pB() => Encode("press B");
        private static byte[] rB() => Encode("release B");

        private static byte[] L() => Encode("click L");
        private static byte[] R() => Encode("click R");
        private static byte[] ZL() => Encode("click ZL");
        private static byte[] ZR() => Encode("click ZR");

        private static byte[] PLUS() => Encode("click PLUS");
        private static byte[] MINUS() => Encode("click MINUS");

        private static byte[] Home() => Encode("click HOME");
        private static byte[] Capture() => Encode("click CAPTURE");

        private static byte[] Up() => Encode("click DUP");
        private static byte[] Right() => Encode("click DRIGHT");
        private static byte[] Down() => Encode("click DDOWN");
        private static byte[] Left() => Encode("click DLEFT");

        private static byte[] LSTICK() => Encode("click LSTICK");
        private static byte[] RSTICK() => Encode("click RSTICK");


        private static byte[] pL() => Encode("press L");
        private static byte[] rL() => Encode("release L");
        private static byte[] detach() => Encode("detachController");

        private static byte[] LstickTR() => Encode("setStick LEFT 0x7FFF 0x7FFF");
        private static byte[] LstickTL() => Encode("setStick LEFT -0x8000 0x7FFF");
        private static byte[] LstickBR() => Encode("setStick LEFT 0x7FFF -0x8000");
        private static byte[] LstickBL() => Encode("setStick LEFT -0x8000 -0x8000");
        private static byte[] LstickU() => Encode("setStick LEFT 0x0 0x7FFF");
        private static byte[] LstickL() => Encode("setStick LEFT -0x8000 0x0");
        private static byte[] LstickD() => Encode("setStick LEFT 0x0 -0x8000");
        private static byte[] LstickR() => Encode("setStick LEFT 0x7FFF 0x0");
        private static byte[] resetLeft() => Encode("setStick LEFT 0 0");
        private static byte[] RstickU() => Encode("setStick RIGHT 0x0 0x7FFF");
        private static byte[] RstickL() => Encode("setStick RIGHT -0x8000 0x0");
        private static byte[] RstickD() => Encode("setStick RIGHT 0x0 -0x8000");
        private static byte[] RstickR() => Encode("setStick RIGHT 0x7FFF 0x0");
        private static byte[] resetRight() => Encode("setStick RIGHT 0 0");

        private static string IslandName = "";

        public controller(Socket S)
        {
            s = S;
        }

        public static void clickA()
        {
            SendString(s, A());
        }

        public static void pressA()
        {
            SendString(s, pA());
        }

        public static void releaseA()
        {
            SendString(s, rA());
        }

        public static void clickB()
        {
            SendString(s, B());
        }

        public static void pressB()
        {
            SendString(s, pB());
        }

        public static void releaseB()
        {
            SendString(s, rB());
        }

        public static void clickX()
        {
            SendString(s, X());
        }

        public static void pressX()
        {
            SendString(s, pX());
        }

        public static void releaseX()
        {
            SendString(s, rX());
        }

        public static void clickY()
        {
            SendString(s, Y());
        }

        public static void pressY()
        {
            SendString(s, pY());
        }

        public static void releaseY()
        {
            SendString(s, rY());
        }

        public static void clickL()
        {
            SendString(s, L());
        }
        public static void clickR()
        {
            SendString(s, R());
        }
        public static void clickZL()
        {
            SendString(s, ZL());
        }
        public static void clickZR()
        {
            SendString(s, ZR());
        }

        public static void clickPLUS()
        {
            SendString(s, PLUS());
        }
        public static void clickMINUS()
        {
            SendString(s, MINUS());
        }

        public static void clickHOME()
        {
            SendString(s, Home());
        }
        public static void clickCAPTURE()
        {
            SendString(s, Capture());
        }

        public static void clickUp()
        {
            SendString(s, Up());
        }
        public static void clickLeft()
        {
            SendString(s, Left());
        }
        public static void clickDown()
        {
            SendString(s, Down());
        }
        public static void clickRight()
        {
            SendString(s, Right());
        }

        public static void clickRightStick()
        {
            SendString(s, RSTICK());
        }
        public static void clickLeftStick()
        {
            SendString(s, LSTICK());
        }

        public static void pressL()
        {
            SendString(s, pL());
        }
        public static void releaseL()
        {
            SendString(s, rL());
        }

        public static void LstickTopRight()
        {
            SendString(s, LstickTR());
        }
        public static void LstickTopLeft()
        {
            SendString(s, LstickTL());
        }
        public static void LstickBottomRight()
        {
            SendString(s, LstickBR());
        }
        public static void LstickBottomLeft()
        {
            SendString(s, LstickBL());
        }

        public static void LstickUp()
        {
            SendString(s, LstickU());
        }
        public static void LstickLeft()
        {
            SendString(s, LstickL());
        }
        public static void LstickDown()
        {
            SendString(s, LstickD());
        }
        public static void LstickRight()
        {
            SendString(s, LstickR());
        }
        public static void resetLeftStick()
        {
            SendString(s, resetLeft());
        }

        public static void RstickUp()
        {
            SendString(s, RstickU());
        }
        public static void RstickLeft()
        {
            SendString(s, RstickL());
        }
        public static void RstickDown()
        {
            SendString(s, RstickD());
        }
        public static void RstickRight()
        {
            SendString(s, RstickR());
        }
        public static void resetRightStick()
        {
            SendString(s, resetRight());
        }

        public static void detachController()
        {
            SendString(s, detach());
        }




        public static void SendString(Socket socket, byte[] buffer, int offset = 0, int size = 0, int timeout = 100)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes is already sent
            if (size == 0)
                for (int i = offset; i < buffer.Length; i++)
                    if (buffer[i] == 0xA)
                    {
                        size = i + 1 - offset;
                        break;
                    }
            if (size == 0) size = buffer.Length - offset;
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        //Thread.Sleep(10);
                    }
                    else
                        throw;  // any serious error occurr
                }
            } while (sent < size);
        }





    }
}
