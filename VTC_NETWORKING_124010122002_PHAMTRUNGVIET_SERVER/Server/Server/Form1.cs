using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Text;
using Microsoft.Azure.KeyVault.Cryptography.Algorithms;

namespace Server
{
    public partial class Form1 : Form
    {
        RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
        public RSAParameters _privatekey;
        public RSAParameters _publickey;
        const int maxAccess = 10000;
        public IPEndPoint IP;
        public Socket server;
        public List<Socket> clients = new List<Socket> ();

        public void RSAEncryption()
        {
            _privatekey = csp.ExportParameters(true);
            _publickey = csp.ExportParameters(false);
        }

        public string GetPublicKey()
        {
            var sw = new StringWriter();
            var xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, _privatekey);
            return sw.ToString();
        }

        public string Encrypt(string txt)
        {
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(_publickey);
            var data = Encoding.Unicode.GetBytes(txt);
            var cypher = csp.Encrypt(data, false);
            return Convert.ToBase64String(cypher);
        }

        public string Decrypt(string txt)
        {
            var data = Convert.FromBase64String(txt);
            csp.ImportParameters(_privatekey);
            var ori = csp.Decrypt(data, false);
            return Encoding.Unicode.GetString(ori);
        }
        public Form1()
        {
            RSAEncryption rsa = new RsaEncryption();
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void btnsend_Click(object sender, EventArgs e)
        {
            foreach(Socket item in clients)
            {
                Send(item);
            }
            message.Clear();
        }

        byte[] Serialize(Object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }

        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }
        void Connect()
        {
            
            IPEndPoint IP = new IPEndPoint(IPAddress.Any, 2012);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(IP);
            Thread listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(maxAccess);
                        Socket client = server.Accept();
                        clients.Add(client);
                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                
                {
                    IPEndPoint IP = new IPEndPoint(IPAddress.Any, 2012);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            });
            listen.IsBackground = true;
            listen.Start();
        }

        void Receive(Object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[5120];
                    client.Receive(data);

                    string messageTxt = (string)Deserialize(data);
                    AddMessage(messageTxt);
                    foreach(Socket item in clients)
                    {
                        if (item != null && item != client) item.Send(Serialize(messageTxt));
                    }
                }
            }
            catch
            {
                clients.Remove(client);
                client.Close();
            }

        }

        void AddMessage(string msg)
        {
            listview.Items.Add(new ListViewItem() { Text = msg });
        }

        void Send(Socket client)
        {
            if (message.Text != String.Empty) client.Send(Serialize(message.Text));
        }
        void Close()
        {
            server.Close();
        }
    }
}