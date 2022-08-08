using System.Security.Cryptography;
using System.Text;

namespace GSRecordMining.Services
{
    public class EncodeService
    {
        private readonly string key = "ramdomkey";
        public string GetRandomString(int length)
        {
            var allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";

            var chars = new char[length];
            var rd = new Random();

            for (var i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new String(chars);
        }

        public string PBKDF2Encode(string input)
        {
            return Convert.ToBase64String(new Rfc2898DeriveBytes(input, Encoding.UTF8.GetBytes(key), iterations: 5000).GetBytes(20));
        }
        public byte[] PBKDF2Hash(string input)
        {
            return new Rfc2898DeriveBytes(password: input, salt: Encoding.UTF8.GetBytes(key), iterations: 5000).GetBytes(20);
        }
        public bool PBKDF2Verify(string storedString, string inputString)
        {
            return storedString == PBKDF2Encode(inputString);
        }
        public bool PBKDF2Verify(byte[] storedHash, string inputString)
        {
            ReadOnlySpan<byte> a1 = storedHash;
            ReadOnlySpan<byte> a2 = PBKDF2Hash(inputString);
            return a1.SequenceEqual(a2);
        }
        public string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString(); 
        }

        public string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.ASCII.GetString(bytes); 
        }
    }
}
