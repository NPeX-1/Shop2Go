using System.CodeDom;
using System.Security.Cryptography;
using System.Text;

namespace BlockChain
{
    public struct Block
    {
        public int index { get; set; }
        public string hash { get; set; }
        public int diff { get; set; }
        public int nonce { get; set; }
        public long timestamp { get; set; }
        public string value { get; set; }
        public string prevHash { get; set; }
    }

    public class Blockchain
    {
        public List<Block> blockchain { get; set; }
        public int diffAdjustInterval { get; set; }
        public int blockGenerationInterval { get; set; }
        public int currentDiff { get; set; }

    public Blockchain()
        {
            this.blockchain = new List<Block>();
            this.diffAdjustInterval = 10;
            this.blockGenerationInterval = 10;
            this.currentDiff = 4;
            CreateBlockchain();
        }

        public void CreateBlockchain()
        {
            long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int nonce = Mine(0, currentDiff, "Blockchain Start","0" , time);

            Block newblock = new Block
            {
                index = 0,
                hash = nonceToHash(0,currentDiff,nonce,time,"Blockchain Start", "0"),
                diff = currentDiff,
                nonce = nonce, 
                timestamp = time,
                value = "Blockchain Start",
                prevHash = "0"
            };

            blockchain.Add(newblock);
            Console.WriteLine("New block mined (Index: 0, Diff: " + currentDiff + ",Nonce; " + nonce + ")\n");
        }

        public void AddToChain(string value, string prevHash)
        {
                int length = blockchain.Count;
                long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                currentDiff = newDiff();
                int nonce = Mine(length, currentDiff, value, prevHash, time);

                Block newblock = new Block
                {
                    index = length,
                    hash = nonceToHash(length, currentDiff, nonce, time, value, prevHash),
                    diff = currentDiff,
                    nonce = nonce,
                    timestamp = time,
                    value = value,
                    prevHash = prevHash
                };
             while (!isBlockValid(newblock))
            {
                length = blockchain.Count;
                time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                currentDiff = newDiff();
                nonce = Mine(length, currentDiff, value, prevHash, time);

                newblock = new Block
                {
                    index = length,
                    hash = nonceToHash(length, currentDiff, nonce, time, value, prevHash),
                    diff = currentDiff,
                    nonce = nonce,
                    timestamp = time,
                    value = value,
                    prevHash = prevHash
                };
            }

            blockchain.Add(newblock);
            Console.WriteLine("New block mined (Index: " + length + ", Diff: " + currentDiff + ",Nonce; " + nonce + ")\n");
        }

        private bool isBlockValid(Block block)
        {
            if (block.prevHash != blockchain[block.index - 1].hash)
                return false;
            if (nonceToHash(block.index, block.diff, block.nonce, block.timestamp, block.value, block.prevHash) != block.hash)
                return false;
            if (block.timestamp - DateTimeOffset.UtcNow.ToUnixTimeSeconds() > 60)
                return false;
            if (block.timestamp - blockchain[blockchain.Count()-1].timestamp > 60)
                return false;
            else
                return true;
        }

        public bool isChainValid(Blockchain chain)
        {
            for(int i=1;i<blockchain.Count;i++)
            {
                if (!isBlockValid(blockchain[i])) return false;
            }
            return true;
        }

        private int Mine(int Index, int Diff, string Value,string prevHash, long Timestamp)
        {
            int nonce = 1;
            string newHash=nonceToHash(Index,Diff,nonce,Timestamp,Value,prevHash);

            while(!validateHash(newHash,Diff))
            {
                nonce++;
                newHash = nonceToHash(Index, Diff, nonce, Timestamp, Value, prevHash);
            }
            return nonce;
        }

        private string nonceToHash(int index, int diff, int nonce, long timestamp, string value, string prevHash)
        {
            string final = index.ToString()+diff.ToString()+nonce.ToString()+timestamp.ToString()+value+prevHash;
            SHA256 sha = SHA256.Create();
            byte[] hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(final));
            final = BitConverter.ToString(hashed).Replace("-", "").ToLower();
            return final;
        }

        private bool validateHash(string hash, int diff)
        {
            int zeros = 0;
            for(int i=0;i<hash.Length; i++)
            {
                if (hash[i] == '0') zeros++;
                else break;
            }

            if (zeros == diff) return true;
            else return false;
        }

        private int newDiff()
        {
            if (blockchain.Count < diffAdjustInterval+1)
                return currentDiff;

            Block prevChange = blockchain[blockchain.Count - diffAdjustInterval - 1];
            Block latestBlock = blockchain[blockchain.Count-1];
            int timeExpected = blockGenerationInterval * diffAdjustInterval;
            long timeTaken = latestBlock.timestamp - prevChange.timestamp;

            if (timeTaken < (timeExpected / 2))
                return prevChange.diff + 1;
            else if (timeTaken > (timeExpected * 2))
                return prevChange.diff - 1;
            else
                return prevChange.diff;
        }

        public string getLastHash()
        {
            return blockchain[blockchain.Count-1].hash;
        }

        public int getIndex()
        {
            return blockchain.Count;
        }

        public int getDiff()
        {
            return currentDiff;
        }

        public int getTotalDiff()
        {
            int diff = 0;
            int temp = 0;
            double temp2 = 0.0d;
            for(int i=0; i<blockchain.Count; i++)
            {
                temp = blockchain[i].diff;
                temp2 = Math.Pow(2.0d, temp);
                diff += Convert.ToInt32(temp2);
            }

            return diff;
        }

        public string printChain()
        {
            string final = "";
            for(int i=0; i<blockchain.Count;i++)
            {
                final += blockchain[i].index + " \n" + blockchain[i].hash + "\n" + blockchain[i].prevHash + "\n" + blockchain[i].diff + "\n" + blockchain[i].nonce + "\n" + blockchain[i].value + "\n\n\n";
            }
            return final;
        }
    }

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
            /*Blockchain chain = new Blockchain();
            for(int i=0;i<100;i++)
            {
                chain.AddToChain("edasdas",chain.getLastHash());
            }*/
        }
    }
}