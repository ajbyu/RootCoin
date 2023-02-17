using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using EllipticCurve;

namespace RootCoin
{
    class Program
    {
        static void Main(string[] args)
        {
            //Make wallets
            PrivateKey key1 = new PrivateKey();
            PublicKey wallet1 = key1.publicKey();

            PrivateKey key2 = new PrivateKey();
            PublicKey wallet2 = key2.publicKey();

            //Initialize blockchain
            Blockchain RootChain = new Blockchain(2, 100);

            //Create initial transaction
            Console.WriteLine("Starting miner.");
            RootChain.MinePendingTransactions(wallet1);

            Console.WriteLine("Balance of wallet1 is $" + RootChain.GetWalletBalance(wallet1).ToString());

            //Transfer money from one wallet to another
            Transaction tx1 = new Transaction(wallet1, wallet2, 10);
            tx1.SignTransaction(key1);
            RootChain.AddPendingTransaction(tx1);
            RootChain.MinePendingTransactions(wallet2);

            //Check balances
            Console.WriteLine("Balance of wallet1 is $" + RootChain.GetWalletBalance(wallet1).ToString());
            Console.WriteLine("Balance of wallet2 is $" + RootChain.GetWalletBalance(wallet2).ToString());

            string blockJson = JsonConvert.SerializeObject(RootChain, Formatting.Indented);
            Console.WriteLine(blockJson);

            if (RootChain.IsChainValid())
            {
                Console.WriteLine("Blockchain is valid.");
            }
            else
            {
                Console.WriteLine("Blockchain is not valid.");
            }
        }
    }
}