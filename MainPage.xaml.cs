using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ACNH_Chat
{
    public partial class MainPage : ContentPage
    {
        private Socket socket;
        //updated offset to work on latest verion of ACNH Game.  
        private static string chat = "[main+5254A40]+40"; /// OLD "[main+4AA9CD8]+40";
        private static bool sendLock = false;

        String getipaddress = "";
        bool connecting = false;
        bool chatting = false;

        private controller controller = null;

        public MainPage()
        {
            InitializeComponent();
            ipaddresstext.Text = Preferences.Default.Get("ipaddress", "");
            connectbutton.BackgroundColor = Color.Parse("Aqua");
            startchat.BackgroundColor = Color.Parse("Aqua");
            sendchattext.BackgroundColor = Color.Parse("Aqua");
        }


        public void Connect(object sender, EventArgs e)
        {


            if (ipaddresstext.Text == null)
            {
                DisplayAlertAsync("Missing input", "You must enter your switch ip address", "OK");
                return;
            }

            Preferences.Default.Set("ipaddress", ipaddresstext.Text);


            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipaddresstext.Text), 6000);

            IAsyncResult result = socket.BeginConnect(ep, null, null);
            bool conSuceded = result.AsyncWaitHandle.WaitOne(3000, true);
            if (conSuceded == true)
            {
                connectbutton.BackgroundColor = Color.Parse("Lime");
                connectbutton.Text = "Connected";
                controller = new controller(socket);
            }
            else
            {
                connectbutton.BackgroundColor = Color.Parse("Aqua");
                connectbutton.Text = "Connect";
            }

        }


        public void Startchat(object sender, EventArgs e)
        {
            controller.detachController();
            controller.clickA();
            controller.clickZR();
            Thread.Sleep(1000);
            controller.clickB();
            controller.clickB();
            controller.clickB();
        }

        public void Sendchat(object sender, EventArgs e)
        {

            Chat(socket);
            string cleanStr = chattext.Text.Trim().Replace("\n", " ");

            if (sendLock)
                return;
            if (cleanStr.Equals(""))
                return;


            sendLock = true;

            Thread sendThread = new Thread(delegate () { sendChat(cleanStr); });
            sendThread.Start();


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
        public static int ReceiveString(Socket socket, byte[] buffer, int offset = 0, int size = 0, int timeout = 30000)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;  // how many bytes is already received
            if (size == 0) size = buffer.Length - offset;
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        //Thread.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurr
                }
            } while (received < size && buffer[received - 1] != 0xA);
            return received;
        }


        public static void pokeAbsoluteAddress(Socket socket, string address, string value)
        {
            //lock (botLock)
            {
                string msg = String.Format("pokeAbsolute 0x{0:X8} 0x{1}\r\n", address, value);
                SendString(socket, Encoding.UTF8.GetBytes(msg));
            }
        }


        public static string ByteToHexString(byte[] b)
        {
            String hexString = BitConverter.ToString(b);
            hexString = hexString.Replace("-", "");

            return hexString;
        }

        public static byte[] ByteTrim(byte[] input)
        {
            int newLength = 1;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == 0x0)
                {
                    newLength = i;
                    break;
                }
            }

            byte[] newArray = new byte[newLength];
            Array.Copy(input, newArray, newArray.Length);

            return newArray;
        }

        public static byte[] peekMainAddress(Socket socket, string address, int size)
        {
            //lock (botLock)
            {
                byte[] result = new byte[size];

                string msg = String.Format("peekMain 0x{0:X8} 0x{1}\r\n", address, size);
                //Debug.Print("PeekMain : " + msg);
                SendString(socket, Encoding.UTF8.GetBytes(msg));

                byte[] b = new byte[size * 2 + 64];
                int first_rec = ReceiveString(socket, b);
                string buffer = Encoding.ASCII.GetString(b, 0, size * 2);

                if (buffer == null)
                {
                    return null;
                }
                for (int i = 0; i < size; i++)
                {
                    result[i] = Convert.ToByte(buffer.Substring(i * 2, 2), 16);
                }

                return result;
            }
        }

        public static byte[] peekAbsoluteAddress(Socket socket, string address, int size)
        {
            //lock (botLock)
            {
                byte[] result = new byte[size];

                string msg = String.Format("peekAbsolute 0x{0:X8} {1}\r\n", address, size);
                SendString(socket, Encoding.UTF8.GetBytes(msg));
                byte[] b = new byte[size * 2 + 64];
                int first_rec = ReceiveString(socket, b);
                string buffer = Encoding.ASCII.GetString(b, 0, size * 2);

                if (buffer == null)
                {
                    return null;
                }
                for (int i = 0; i < size; i++)
                {
                    result[i] = Convert.ToByte(buffer.Substring(i * 2, 2), 16);
                }

                return result;
            }
        }

        /*
        public static void pokeAbsoluteAddress(Socket socket, string address, string value)
        {
            //lock (botLock)
            {
                string msg = String.Format("pokeAbsolute 0x{0:X8} 0x{1}\r\n", address, value);
                SendString(socket, Encoding.UTF8.GetBytes(msg));
            }
        }
        */
        public static ulong GetCoordinateAddress(string strInput, Socket s)
        {
            //lock (lockObject)
            {
                // Regex pattern to get operators and offsets from pointer expression.	
                string pattern = @"(\+|\-)([A-Fa-f0-9]+)";
                Regex regex = new Regex(pattern);
                Match match = regex.Match(strInput);

                // Get first offset from pointer expression and read address at that offset from main start.	
                var ofs = Convert.ToUInt64(match.Groups[2].Value, 16);
                var address = BitConverter.ToUInt64(peekMainAddress(s, ofs.ToString("X"), 0x8), 0);
                match = match.NextMatch();

                // Matches the rest of the operators and offsets in the pointer expression.	
                while (match.Success)
                {
                    // Get operator and offset from match.	
                    string opp = match.Groups[1].Value;
                    ofs = Convert.ToUInt64(match.Groups[2].Value, 16);

                    // Add or subtract the offset from the current stored address based on operator in front of offset.	
                    switch (opp)
                    {
                        case "+":
                            address += ofs;
                            break;
                        case "-":
                            address -= ofs;
                            break;
                    }

                    // Attempt another match and if successful read bytes at address and store the new address.	
                    match = match.NextMatch();
                    if (match.Success)
                    {
                        byte[] bytes = peekAbsoluteAddress(s, address.ToString("X"), 0x8);
                        address = BitConverter.ToUInt64(bytes, 0);
                    }
                }

                return address;
            }
        }



        public void Chat(Socket Socket)
        {
            socket = Socket;

            ///InitializeComponent();            
            //chattext.SelectAll();
            chattext.CursorPosition = 0;
            chattext.SelectionLength = chattext.Text?.Length ?? 0;

          
        }


        private void chatButton_Click(object sender, EventArgs e)
        {
            string cleanStr = chattext.Text.Trim().Replace("\n", " ");

            if (sendLock)
                return;
            if (cleanStr.Equals(""))
                return;


            sendLock = true;

            Thread sendThread = new Thread(delegate () { sendChat(cleanStr); });
            sendThread.Start();
        }

        private void sendChat(string message)
        {
            ulong ChatAddress = GetCoordinateAddress(chat, socket);

            controller.clickR();
            Thread.Sleep(800);
            controller.clickY();

            byte[] StrBytes = Encoding.Unicode.GetBytes(message);
            byte[] sendBytes = new byte[StrBytes.Length * 2];
            Buffer.BlockCopy(StrBytes, 0, sendBytes, 0, StrBytes.Length);
            pokeAbsoluteAddress(socket, ChatAddress.ToString("X"), ByteToHexString(sendBytes));

            controller.clickPLUS();
            Thread.Sleep(400);

            controller.clickB();
            Thread.Sleep(200);
            controller.clickB();
            Thread.Sleep(200);
            controller.clickB();
            Thread.Sleep(200);

            sendLock = false;
        }















    }
}
