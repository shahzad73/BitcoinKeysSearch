using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using System.Threading;
using Secp256k1;
using System.IO;
using System.Security.Cryptography;
using System.Numerics;


namespace BitcoinAddressSearch
{
    class Program
    {
        static int ThreadDelay = 0;
        static string TargetAddressLocation;
        static string PrivateKeyFoundSaveLocation;
        static byte[] currentAddress;
        static HashSet<string> bitcoinAddressesFound = new HashSet<string>();
        static double totalAddressesCounted = 0;
        public static byte[] tempBitcoinAddressArrayWithChecksum = new byte[37];



        static void Main(string[] args)
        {

            startCoinSearchThread(args);

            //extractNonExistanceRecordsFromFiles("f://shahzad//bitcoinaddresses.txt","f://file2.txt", "f://");

        }









        public static void startCoinSearchThread(string[] args)
        {

            if (args.Length == 0)
            {
                ThreadDelay = 1;
                TargetAddressLocation = "f:\\shahzad\\bitcoinaddresses.txt";
                PrivateKeyFoundSaveLocation = "f:\\";
            }
            else
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("3 parameters required : {Thread Delay}  {Target Address Location}  {Private Key Found Save Location}");
                    return;
                }
                ThreadDelay = int.Parse(args[0]);
                TargetAddressLocation = args[1];
                PrivateKeyFoundSaveLocation = args[2];
            }


            generateBitcoinPrivateKey();

            if (File.Exists(PrivateKeyFoundSaveLocation + "BitcoinAddres-Test.txt"))
                File.Delete(PrivateKeyFoundSaveLocation + "BitcoinAddres-Test.txt");

            System.IO.File.WriteAllText(PrivateKeyFoundSaveLocation + "BitcoinAddres-Test.txt", "Some Test String ...");

            if (File.Exists(PrivateKeyFoundSaveLocation + "BitcoinAddres-Test.txt"))
            {
                File.Delete(PrivateKeyFoundSaveLocation + "BitcoinAddres-Test.txt");
                Console.WriteLine("File location verified");
            }
            else
            {
                Console.WriteLine("File location not verified. check file location or permissions");
                return;
            }


            Thread T1 = new Thread(new ThreadStart(SearchBitcoinAddress));
            T1.Start();

            Console.WriteLine("You can now press any key to stop this thread");


            ConsoleKey key = ConsoleKey.Y;
            while (key != ConsoleKey.Escape)
            {
                key = Console.ReadKey().Key;

                if (key == ConsoleKey.Escape)
                {
                    printSummary();
                    if (T1.ThreadState == ThreadState.Running)
                        T1.Abort();
                    Console.WriteLine("Execution stopped .... ");
                    return;
                }

                if (key == ConsoleKey.I)
                    printSummary();

                if (key == ConsoleKey.P)
                {
                    if (T1.ThreadState == ThreadState.Running)
                    {
                        printSummary();
                        Console.WriteLine("Thread is now suspended");
                        T1.Suspend();
                    }
                }

                if (key == ConsoleKey.R)
                {
                    if (T1.ThreadState == ThreadState.Suspended)
                    {
                        Console.WriteLine("Thread is now resumed");
                        T1.Resume();
                    }
                }


            }
        }





        public static void printSummary()
        {
            Console.Clear();

            Console.WriteLine("Total Searched addresses " + totalAddressesCounted);

            if (bitcoinAddressesFound.Count != 0)
            {
                Console.WriteLine("------ Addresses Found ---------");
                foreach (string ad in bitcoinAddressesFound)
                {
                    Console.WriteLine(ad);
                }
            }
            else
            {
                Console.WriteLine("------ Nothing -----------");
            }
        }




        public static void SearchBitcoinAddress()
        {
            HashSet<string> bitcoinAddressesHash = new HashSet<string>();

            string line;
            string RandomTestAddress = "";
            Random rnd = new Random();
            System.IO.StreamReader file = new System.IO.StreamReader(TargetAddressLocation);
            while ((line = file.ReadLine()) != null)
            {
                bitcoinAddressesHash.Add(line);
                if (RandomTestAddress == "")
                {
                    if (rnd.Next(200) < 10)           //select a random address for hashtable test
                    {
                        RandomTestAddress = line;                        
                    }
                }
            }
            file.Close();

            Console.WriteLine("HashTable Initialized. It contains " + bitcoinAddressesHash.Count + " target addresses");

            //if we have a random address selected for testing then conduct a test on hashtable 
            if (RandomTestAddress != "")
            {
                Console.WriteLine("Conducting a test on hashtable for address " + RandomTestAddress);
                if (bitcoinAddressesHash.Contains(RandomTestAddress))
                    Console.WriteLine("Test successful");
            }

            Console.WriteLine("Thread delay is set to " + ThreadDelay);




            // ----------------------------------------------------------------------------
            // -------------------------  Main Thread -------------------------------------
            // ----------------------------------------------------------------------------
            int count = 0;
            BitcoinSecret secretKey;
            while (true)
            {
                secretKey = getNextAddress();

                //Console.WriteLine(secretKey.ToString() + " UN-Compressed : " + secretKey.PubKey.GetAddress(Network.Main).ToString() + "    Compressed: " + secretKey.PubKey.Compress().GetAddress(Network.Main).ToString());

                if (bitcoinAddressesHash.Contains(secretKey.PubKey.GetAddress(Network.Main).ToString()) || bitcoinAddressesHash.Contains(secretKey.PubKey.Compress().GetAddress(Network.Main).ToString()))
                {
                    string str = secretKey.ToString() + " UN-Compressed : " + secretKey.PubKey.GetAddress(Network.Main).ToString() + "    Compressed: " + secretKey.PubKey.Compress().GetAddress(Network.Main).ToString();
                    Console.WriteLine(str);
                    bitcoinAddressesFound.Add(str);
                    System.IO.File.WriteAllText(PrivateKeyFoundSaveLocation + "BitcoinAddres-" + secretKey.PubKey.GetAddress(Network.Main).ToString() + ".txt", str);
                }

                count++;
                if (count > 999)
                {
                    totalAddressesCounted = totalAddressesCounted + count;
                    Console.WriteLine("Total Search " + totalAddressesCounted);
                    count = 0;
                }

                Thread.Sleep(ThreadDelay);
            }
        }


        public static BitcoinSecret getNextAddress()
        {
            if (currentAddress[31] == 0xFF)
                generateBitcoinPrivateKey();
            else 
                currentAddress[31] = (byte)(currentAddress[31] + 1);

            //Console.WriteLine(Hex.BytesToHex(currentAddress));

            string str2 = convertBitcoinHaxAddressToWIF(currentAddress);
            return new BitcoinSecret(str2);
        }


        public static void generateBitcoinPrivateKey()
        {
            currentAddress = GenerateRandomCryptographicKey(33);
            currentAddress[31] = 0;                 // make the first byte 0 so that we can iterate from 0 to FF
            currentAddress[0] = 0x80;               // bitcoin address is 32 bytes and first byte should be 0x80     
        }

        static byte[] GenerateRandomCryptographicKey(int keyLength)
        {
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[keyLength];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return randomBytes;
        }

        public static string convertBitcoinHaxAddressToWIF(byte[] secret)
        {
            Array.Copy(secret, tempBitcoinAddressArrayWithChecksum, secret.Length);
            Array.Copy(Secp256k1.SHA256.DoubleHashCheckSum(secret), 0, tempBitcoinAddressArrayWithChecksum, secret.Length, 4);
            return Base58.Encode(tempBitcoinAddressArrayWithChecksum);
        }

    }
}