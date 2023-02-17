using EllipticCurve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootCoin
{
    public class Blockchain
    {
        public int Difficulty { get; set; }
        public decimal MiningReward { get; set; }
        public List<Block> Chain { get; set; }
        public List<Transaction> PendingTransactions { get; set; }

        public Blockchain(int difficulty, decimal miningReward)
        {
            Chain = new List<Block>();
            Chain.Add(CreateGenesisBlock());
            Difficulty = difficulty;
            MiningReward = miningReward;
            PendingTransactions = new List<Transaction>();
        }

        /// <summary>
        /// Create genesis block. Should onlybe called once.
        /// </summary>
        /// <returns></returns>
        public Block CreateGenesisBlock()
        {
            return new Block(0, DateTime.Now.ToString("yyyyMMddHHmmssffff"), new List<Transaction>());
        }

        /// <summary>
        /// Get the last block object on the chain.
        /// </summary>
        /// <returns></returns>
        public Block GetLatestBlock()
        {
            return Chain.Last();
        }

        /// <summary>
        /// Add a block to the chain. Sets hashes.
        /// </summary>
        /// <param name="newBlock"></param>
        public void AddBlock(Block newBlock)
        {
            newBlock.PreviousHash = GetLatestBlock().Hash;
            newBlock.Hash = newBlock.CalculateHash();
            this.Chain.Add(newBlock);
        }

        /// <summary>
        /// Add a pending transaction. Will be discarded if improperly formed.
        /// </summary>
        /// <param name="transaction"></param>
        /// <exception cref="Exception"></exception>
        public void AddPendingTransaction(Transaction transaction)
        {
            if (transaction.FromAddress == null || transaction.ToAddress == null)
            {
                throw new Exception("Transactions must include to and from addresses");
            }

            if (transaction.Amount > this.GetWalletBalance(transaction.FromAddress))
            {
                throw new Exception("Transaction cannot be for more than is in the wallet. Get more money kid.");
            }

            if (!transaction.IsValid())
            {
                throw new Exception("Transaction is invalid.");
            }

            PendingTransactions.Add(transaction);
        }

        /// <summary>
        /// Search through chain for balance of wallet.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public decimal GetWalletBalance(PublicKey address)
        {
            decimal balance = 0;

            string addressDER = BitConverter.ToString(address.toDer()).Replace("-", "");

            foreach (var block in this.Chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.FromAddress != null)
                    {
                        string fromDER = BitConverter.ToString(transaction.FromAddress.toDer()).Replace("-", "");

                        if (fromDER == addressDER)
                        {
                            balance -= transaction.Amount;
                        }
                        
                    }
                    string toDER = BitConverter.ToString(transaction.ToAddress.toDer()).Replace("-", "");
                    if (toDER == addressDER)
                    {
                        balance += transaction.Amount;
                    }
                }
            }

            return balance;
        }

        /// <summary>
        /// Mine all pending transactions.
        /// </summary>
        /// <param name="rewardAddress"></param>
        public void MinePendingTransactions(PublicKey rewardWallet)
        {
            Transaction rewardTx = new Transaction(null, rewardWallet, MiningReward);
            this.PendingTransactions.Add(rewardTx);

            Block newBlock = new Block(GetLatestBlock().Index + 1, DateTime.Now.ToString("yyyyMMddHHmmssffff"), PendingTransactions, GetLatestBlock().Hash);
            newBlock.Mine(this.Difficulty);

            Console.WriteLine("Block mined.");
            this.Chain.Add(newBlock);
            this.PendingTransactions = new List<Transaction>();
        }

        /// <summary>
        /// Check if chain is valid by comparing hashes.
        /// </summary>
        /// <returns></returns>
        public bool IsChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                //Check if current block hash is same as calculated hash
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
