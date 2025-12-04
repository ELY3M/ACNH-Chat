using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using System.Net.Sockets;
using Environment = System.Environment;
using System.Text;
using Java.Lang;
using String = System.String;
using Exception = System.Exception;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Preferences;
using System.Net;

namespace ACNH_Chat
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]


    public class MainActivity : AppCompatActivity
    {



        private Socket socket;
        private static string chat = "[main+4AA9CD8]+40";
        private static bool sendLock = false;


        private ISharedPreferences sp;
        private ISharedPreferencesEditor spedit;
        String getipaddress = "";
        bool connecting = false;
        bool chatting = false;

        private controller controller = null;

        AppCompatEditText ipaddresstext;
        AppCompatButton connectbutton;
        AppCompatButton disconnectbutton;

        AppCompatButton startchat;
        AppCompatEditText chattext;
        AppCompatButton sendbutton;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            var policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
            StrictMode.SetThreadPolicy(policy);
            


            sp = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            spedit = sp.Edit();

            ipaddresstext = FindViewById<AppCompatEditText>(Resource.Id.ipaddress);
            connectbutton = FindViewById<AppCompatButton>(Resource.Id.connect);
            disconnectbutton = FindViewById<AppCompatButton>(Resource.Id.disconnect);

            connectbutton.Click += (sender, e) =>
            {

                spedit.PutString("ipaddress", ipaddresstext.Text.ToString());
                spedit.Commit();

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipaddresstext.Text), 6000);

                IAsyncResult result = socket.BeginConnect(ep, null, null);
                bool conSuceded = result.AsyncWaitHandle.WaitOne(3000, true);
                if (conSuceded == true)
                {
                    connectbutton.SetBackgroundColor(Android.Graphics.Color.Lime);
                    connectbutton.Text = "Connected";
                    controller = new controller(socket);
                }
                else {
                    connectbutton.SetBackgroundColor(Android.Graphics.Color.Aqua);
                    connectbutton.Text = "Connect";
                }
                };



            chattext = FindViewById<AppCompatEditText>(Resource.Id.chat);
            startchat = FindViewById<AppCompatButton>(Resource.Id.startchat);

            getipaddress = sp.GetString("ipaddress", "");
            ipaddresstext.Text = getipaddress;
            sendbutton = FindViewById<AppCompatButton>(Resource.Id.send);


            disconnectbutton.Click += (sender, e) =>
            {

                socket.Close();
                connecting = false;
                connectbutton.Text = "Connect";

            };

           
            //start chat
            startchat.Click += (sender, e) =>
            {
                controller.detachController();
                controller.clickA();
                controller.clickZR();
                Thread.Sleep(1000);
                controller.clickB();
                controller.clickB();
                controller.clickB();

            };



            sendbutton.Click += (sender, e) => {

                Chat(socket);
                string cleanStr = chattext.Text.Trim().Replace("\n", " ");

                if (sendLock)
                    return;
                if (cleanStr.Equals(""))
                    return;


                sendLock = true;

                Thread sendThread = new Thread(delegate () { sendChat(cleanStr); });
                sendThread.Start();



            };




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
            chattext.SelectAll();

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





















        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
