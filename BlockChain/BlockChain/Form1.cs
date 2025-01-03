using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BlockChain
{
    public partial class Form1 : Form
    {
        private static Thread miningThreadVar;
        private static Thread listenerThread;
        public static Blockchain currentBlockchain;
        public static bool mine = false;
        public static bool first = true;
        //public static List<Thread> senderThreads;
        public Form1()
        {
            InitializeComponent();
        }

        private void connectbutton_Click(object sender, EventArgs e)
        {
            Thread clientThread = new Thread(new ThreadStart(receiverThread));
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        private void minebutton_Click(object sender, EventArgs e)
        {
            mine = true;
            try
            {
                miningThreadVar = new Thread(new ThreadStart(miningThread));
                miningThreadVar.IsBackground = true;
                miningThreadVar.Start();
                listenerThread = new Thread(new ThreadStart(listenerLoop));
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + ex.StackTrace);
            }
        }

        private void listenerLoop()
        {
            try
            {
                string ipaddr = "127.0.0.1";
                string port = serverbox.Text;
                bool running = true;
                var server = new TcpListener(IPAddress.Any, int.Parse(port));
                server.Start();

                while(running)
                {
                    TcpClient newClient = server.AcceptTcpClient();
                    Thread newListener = new Thread(new ParameterizedThreadStart(senderThread));
                    newListener.IsBackground = true;
                    newListener.Start(newClient);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString() + ex.StackTrace);
            }
        }

        private void miningThread()
        {
            if(first)
            {
                currentBlockchain = new Blockchain();
                first = false;
                minebox.Invoke(new Action(() => minebox.AppendText("New Block Mined!\n" + currentBlockchain.getLastHash() + "\n\n")));
            }
            while (mine)
            {
                currentBlockchain.AddToChain("Index: " + (currentBlockchain.getIndex()+1) + " by user: "+ username.Text,currentBlockchain.getLastHash());
                minebox.Invoke(new Action(() => minebox.AppendText("New Block Mined!\n" + currentBlockchain.getLastHash() + "\n\n")));
                chainbox.Invoke(new Action(() => chainbox.AppendText(currentBlockchain.printChain())));
            }
        }

        private void senderThread(object obj)
        {
            TcpClient client = (TcpClient)obj;
            string tempchain = "";
            long timer = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                while (client.Connected)
                {
                    if (timer - DateTimeOffset.UtcNow.ToUnixTimeSeconds() > -5)
                    {
                        
                        continue;
                    }
                    timer = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    //var json = JsonSerializer.Serialize<Blockchain>(currentBlockchain);
                    //tempchain = json.ToString();
                    int tempDiff = currentBlockchain.getDiff();
                    int tempCount = currentBlockchain.blockchain.Count();
                    Send(Encoding.UTF8.GetBytes("BLC|"+tempDiff.ToString()+"|"+tempCount.ToString()), client.GetStream());
                    Receive(client.GetStream());
                    for(int i=0; i<currentBlockchain.blockchain.Count; i++)
                    {
                        var json = JsonSerializer.Serialize<Block>(currentBlockchain.blockchain[i]);
                        tempchain = json.ToString();
                        Send(Encoding.UTF8.GetBytes("BLK|"+tempchain), client.GetStream());
                        Receive(client.GetStream());
                    }

                }
                client.Close();
            }catch(Exception ex)
            {
                Console.WriteLine("Error Receiving!: " + ex.Message + "\n Stacktrace: " + ex.StackTrace + "\n");
            }
        }

        private void receiverThread()
        {
            if(first)
            {
                currentBlockchain=new Blockchain();
                first = false;
            }

            string port = destinationport.Text;
            TcpClient client = new TcpClient();
            string temp = "";
            try
            {
                client.Connect(IPAddress.Parse("127.0.0.1"), int.Parse(port));
                NetworkStream networkStream = client.GetStream();
                while (true)
                {
                    temp = "";
                    byte[] receivedChain = Receive(networkStream);
                    temp = Encoding.UTF8.GetString(receivedChain);
                    if (temp.Substring(0, 3) == "BLC")
                    {
                        int counter = 4;
                        string tempDiff = "";
                        string tempBlocks = "";
                        int blocks = 0;
                        for(;counter<temp.Length && temp[counter]!='|'; counter++) {
                            tempDiff+= temp[counter];
                        }
                        counter++;
                        for (; counter < temp.Length; counter++)
                        {
                            tempBlocks += temp[counter];
                        }
                        blocks = int.Parse(tempBlocks);

                        Blockchain receivedBlockchain = new Blockchain();
                        receivedBlockchain.currentDiff = int.Parse(tempDiff);
                        receivedBlockchain.blockchain.Clear();
                        Send(new byte[1], networkStream);
                        for(int i=0;i<blocks;i++)
                        {
                            byte[] receivedBlock = Receive(networkStream);
                            temp=Encoding.UTF8.GetString(receivedBlock);
                            if (temp.Substring(0, 3) == "BLK")
                            {
                                var json = JsonObject.Parse(temp.Substring(4));
                                receivedBlockchain.blockchain.Add(json.Deserialize<Block>());
                                Send(new byte[1], networkStream);
                            }
                        }
                        
                        //var json = JsonObject.Parse(temp);
                        //Blockchain receivedBlockchain = json.Deserialize<Blockchain>();
                        if (validateReceivedChain(receivedBlockchain))
                        {
                            /*mine = false;
                            if(miningThreadVar!=null)
                                miningThreadVar.Join();
                            mine = true;
                            miningThreadVar = new Thread(new ThreadStart(miningThread));
                            miningThreadVar.IsBackground = true;
                            miningThreadVar.Start();*/
                            currentBlockchain = receivedBlockchain;
                            chainbox.Invoke(new Action(() => chainbox.AppendText(receivedBlockchain.printChain())));
                            if (!currentBlockchain.isChainValid(currentBlockchain))
                            {
                                chainbox.Invoke(new Action(() => chainbox.AppendText("It's probably invalid\n")));
                            }
                        }
                    }
                }

            }catch(Exception ex)
            {
                Console.WriteLine("Error sending message!: " + ex.Message + "\n Stacktrace: " + ex.StackTrace + "\n");
            }
        }

        private bool validateReceivedChain(Blockchain receivedChain)
        {
            int localDiff = currentBlockchain.getTotalDiff();
            int receivedDiff = receivedChain.getTotalDiff();

            if (localDiff > receivedDiff)
                return false;
            else
                return true;

          
        }


        static void Send(byte[] msg, NetworkStream networkStream)
        {
            try
            {
                networkStream.Write(msg, 0, msg.Length);

            }
            catch (Exception exception)
            {
                Console.WriteLine("Error receiving message!: " + exception.Message + "\n Stacktrace: " + exception.StackTrace + "\n");
            }

        }

        static byte[] Receive(NetworkStream networkStream)
        {
            try
            {
                byte[] message= new byte[4096];

                int length = networkStream.Read(message, 0, message.Length);
                byte[] final = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    final[i] = message[i];
                }
                return final;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error receiving message!: " + exception.Message + "\n Stacktrace: " + exception.StackTrace + "\n");
                return null;
            }
        }
    }
}
