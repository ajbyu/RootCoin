using EllipticCurve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RootCoin
{
    public class Transaction
    {
        public PublicKey FromAddress { get; set; }
        public PublicKey ToAddress { get; set; }
        public decimal Amount { get; set; }
        public Signature Signature { get; set; }

        public Transaction(PublicKey fromAddress, PublicKey toAddress, decimal amount)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
        }

        public void SignTransaction(PrivateKey signingKey)
        {
            string fromAddressDER = BitConverter.ToString(FromAddress.toDer()).Replace("-", "");
            string signingDER = BitConverter.ToString(signingKey.publicKey().toDer()).Replace("-", "");

            if (fromAddressDER != signingDER)
            {
                throw new Exception("You cannot sign transactions for another wallet.");
            }

            string txHash = this.CalculateHash();

            Signature = Ecdsa.sign(txHash, signingKey);
        }

        public string CalculateHash()
        {
            string fromAddressDER = BitConverter.ToString(FromAddress.toDer()).Replace("-", "");
            string toAddressDER = BitConverter.ToString(ToAddress.toDer()).Replace("-", "");
            string transactionData = fromAddressDER + toAddressDER + Amount;

            byte[] transactionBytes = Encoding.ASCII.GetBytes(transactionData);

            return BitConverter.ToString(SHA256.Create().ComputeHash(transactionBytes)).Replace("-", "");
        }

        public bool IsValid()
        {
            if (this.FromAddress == null) return true;

            if (this.Signature == null)
            {
                throw new Exception("Signature cannot be null");
            }

            return Ecdsa.verify(this.CalculateHash(), Signature, FromAddress);
        }
    }
}
