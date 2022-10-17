using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Client
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        public IPEndPoint IP;
        public Socket client;
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

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
            IPAddress[] ips = Dns.Resolve(Dns.GetHostName()).AddressList;
            IPEndPoint IP = new IPEndPoint(ips[1], 2012);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Can't connect to the server...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }

        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[5120];
                    client.Receive(data);

                    string messageTxt = (string)Deserialize(data);
                    AddMessage(messageTxt);
                }
            }
            catch
            {
                Close();
            }

        }

        void AddMessage(string msg)
        {
            listview.Items.Add(new ListViewItem() { Text = msg });
            message.Clear();
        }

        void Send()
        {
            if(message.Text != String.Empty) client.Send(Serialize(message.Text)); 
        }
        void Close()
        {
            client.Close();
        }

        private void btnsend_Click(object sender, EventArgs e)
        {
            Send();
            AddMessage(message.Text);
        }

        private void message_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Close();
        }
    }
}