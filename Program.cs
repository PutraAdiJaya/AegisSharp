using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AegisSharp
{
    internal class Program
    {
        static short[] constant =
        {
            0x01, 0x02, 0x03, 0x05, 0x08, 0x0d, 0x15, 0x22, 0x37, 0x59, 0x90, 0xe9, 0x79, 0x62, 0xdb, 0x3d, 0x18, 0x55,
            0x6d, 0xc2, 0x2f, 0xf1, 0x20, 0x11, 0x31, 0x42, 0x73, 0xb5, 0x28, 0xdd
        };


        public static short[][] paddByteArray(short[] array)
        {
            short[] ar;
            int Length = array.Length;
            if (Length % 16 != 0)
            {
                ar = new short[Length + (16 - Length % 16)];
                System.Array.Copy(array, 0, ar, 0, array.Length);
                for (int i = array.Length - 1; i < ar.Length; i++)
                {
                    ar[i] = 0;
                }

                return createMessages(ar);
            }

            return createMessages(array);
        }

        /**
 * message divide Length/16
 * @return 
 */
        public static short[][] createMessages(short[] ar)
        {
            short[][] result;
            result = new short[ar.Length / 16][];
            for (int i = 0; i < ar.Length / 16; i++)
            {
                result[i] = new short[2];
            }

            int k = 0;
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[i].Length; j++)
                {
                    result[i][j] = ar[k];
                    k++;
                }
            }

            return result;
        }

        /**
     * IV vector
     */
        public static short[] generateIV()
        {
            short[] arrayIV = new short[16];
            Random r = new Random();
            for (int i = 0; i < arrayIV.Length; i++)
            {
                arrayIV[i] = (short) r.Next();
            }

            return arrayIV;
        }

        /*
        Key generator  from string 128 bit key using SHA-1
        */
        public static short[] generateKey(String keyPhrase)
        {
            byte[] key = Encoding.UTF8.GetBytes(keyPhrase);
            key = Hash(key);
            Array.Resize<byte>(ref key, 16);
            short[] temp = new short[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                temp[i] = key[i];
            }

            return temp;
        }

        public static short[] xor(short[] a, short[] b)
        {
            short[] result = new short[Math.Min(a.Length, b.Length)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (short) ((a[i]) ^ (b[i]));
            }

            return result;
        }

        /**
     * 
     * @param key - input key (16 byte Length)
     * @param Message  - input message - mi (16 byte Length)
     */
        public static void initializationAegis(short[] key, short[] Message, short[] IV)
        {
            short[][] S = new short[5][];
            for (int i = 0; i < 5; i++)
            {
                S[i] = new short[32];
            }

            S[0] = xor(key, Message);


            for (int i = 16; i < constant.Length - 1; i++)
            {
                S[1][i] = constant[i];
            }

            for (int i = 0; i < 16; i++)
            {
                S[2][i] = constant[i];
            }

            S[3] = xor(key, S[2]);
            S[4] = xor(key, S[1]);

            short[][] M = new short[11][];
            for (int i = 0; i < 11; i++)
            {
                M[i] = new short[128];
            }

            for (int i = 4; i >= 0; i--)
            {
                M[i + 6] = key;
                M[i + 1 + 5] = xor(key, IV);
            }
        }

        static byte[] Hash(byte[] input)
        {
            var hash = new SHA1Managed().ComputeHash(input);
            return Encoding.UTF8.GetBytes(string.Concat(hash.Select(b => b.ToString("x2"))));
        }

        // TAMPILKAN BYTE DLAM BENTUK HEXA
        public static void printBytes(short[][] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                for (int k = 0; k < data[i].Length; k++)
                {
                    Console.Write("0x{0:X2} ", data[i][k]);
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }


        // TAMPILKAN BYTE DLAM BENTUK HEXA
        public static void printBytes(short[] data)
        {
            for (int i = 0, loopTo = data.Length - 1; i <= loopTo; i++)
                Console.Write("0x{0:X2} ", data[i]);
            Console.WriteLine();
            Console.WriteLine();
        }

        // PROSES GRAIN PADA DATA BYTE // XOR KEY DGN DATA
        public static short[] processBytes(short[] inS, int inOff, int len, short[] outS, int outOff,
            short[] KeyStream)
        {
            // PROSES XOR INPUT DAN KEY
            for (int i = 0, loopTo = len - 1; i <= loopTo; i++)
                outS[outOff + i] = (short) (inS[inOff + i] ^ KeyStream[i % KeyStream.Length]);
            return outS;
        }

        private static void Main(string[] argvx)
        {
            string PESAN = "ABCD1234~!@#$%^&";
            string KEY = "KEY123";
            short[] data = Array.ConvertAll(Encoding.ASCII.GetBytes(PESAN), q => Convert.ToInt16(q));

            Console.WriteLine("===================== DATA =====================");
            printBytes(data);
            short[][] fixData = paddByteArray(data);
            short[] keyStream = generateKey(KEY);
            short[] iV = generateIV();

            initializationAegis(keyStream, data, iV);


            Console.WriteLine("===================== KEY STREAM =====================");
            printBytes(keyStream);
            short[] cipher = new short[data.Length];
            short[] clear = new short[data.Length];

            cipher = processBytes(data, 0, data.Length, cipher, 0, keyStream);
            Console.WriteLine("===================== CIPER TEXT =====================");
            printBytes(cipher);


            clear = processBytes(cipher, 0, data.Length, clear, 0, keyStream);
            Console.WriteLine("===================== DATA RESULT =====================");
            printBytes(clear);
        }
    }
}