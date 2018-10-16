using NBitcoin;
using Secp256k1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinAddressSearch
{
    class UtilityClasses
    {

        static void extractNonExistanceRecordsFromFiles(string path1, string path2, string newPath)
        {
            string line;
            HashSet<string> hash1 = new HashSet<string>();

            System.IO.StreamReader file = new System.IO.StreamReader(path1);
            while ((line = file.ReadLine()) != null)
            {
                if (hash1.Contains(line))
                    Console.WriteLine(line + " address already exists in first file");
                else
                    hash1.Add(line);
            }
            file.Close();


            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("New Addresses Found");
            Console.WriteLine("");
            string notFoundAddresses = "";
            file = new System.IO.StreamReader(path2);
            while ((line = file.ReadLine()) != null)
            {
                if (!hash1.Contains(line))
                {
                    Console.WriteLine(line);
                    notFoundAddresses = notFoundAddresses + "\r\n " + line;
                }
            }
            file.Close();

            System.IO.File.WriteAllText(newPath + "NewBitcoinAddres.txt", notFoundAddresses);
        }








        static char[] HexCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        static StringBuilder addressBuilder = new StringBuilder();
        public static string generateNextRandomAddress2()
        {
            addressBuilder.Clear();
            for (int a = 0; a < 64; a++)
            {
                //addressBuilder.Append(HexCharacters[randomGenerate.Next(0,16)]);
            }
            return addressBuilder.ToString();
        }




        public static string convertBitcoinHaxAddressToWIF(string hex)
        {
            hex = "80" + hex;
            string secret;
            byte[] b = Secp256k1.SHA256.DoubleHashCheckSum(Hex.HexToBytes(hex));
            secret = hex + Hex.BytesToHex(b);
            secret = Base58.Encode(Hex.HexToBytes(secret));
            return secret;
        }



    }
}
