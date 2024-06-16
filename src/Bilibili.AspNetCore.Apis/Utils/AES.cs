using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Bilibili.AspNetCore.Apis.Utils
{
    class AES
    {
        #region AES

        /// <summary>  
        /// AES encrypt
        /// </summary>  
        /// <param name="data">Raw data</param>  
        /// <param name="key">Key, requires 32 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Encrypted string</returns>  
        public static string Encrypt(string data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] plainBytes = Encoding.UTF8.GetBytes(data);

            var encryptBytes = Encrypt(plainBytes, key, vector);
            if (encryptBytes == null)
            {
                return null;
            }
            return Convert.ToBase64String(encryptBytes);
        }

        /// <summary>
        /// AES encrypt
        /// </summary>
        /// <param name="data">Raw data</param>  
        /// <param name="key">Key, requires 16 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Encrypted byte array</returns>  
        public static byte[] Encrypt(byte[] data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] bKey = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length, (char)0x00)), bKey, bKey.Length);

            byte[] bVector = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(bVector.Length, (char)0x00)), bVector, bVector.Length);

            return Encrypt(data, bKey, bVector);
        }

        /// <summary>
        /// AES encrypt
        /// </summary>
        /// <param name="data">Raw data</param>
        /// <param name="key">Key, requires 16 bits</param>
        /// <param name="vector">IV,requires 16 bits</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] vector)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (key == null || key.Length != 16)
            {
                throw new ArgumentException(nameof(key));
            }
            if (vector == null || vector.Length != 16)
            {
                throw new ArgumentException(nameof(vector));
            }

            byte[] encryptData = null;
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                try
                {
                    using (MemoryStream Memory = new MemoryStream())
                    {
                        using (CryptoStream Encryptor = new CryptoStream(Memory, aes.CreateEncryptor(key, vector), CryptoStreamMode.Write))
                        {
                            Encryptor.Write(data, 0, data.Length);
                            Encryptor.FlushFinalBlock();

                            encryptData = Memory.ToArray();
                        }
                    }
                }
                catch
                {
                    encryptData = null;
                }
                return encryptData;
            }
        }

        /// <summary>  
        ///  AES decrypt
        /// </summary>  
        /// <param name="data">Encrypted data</param>  
        /// <param name="key">Key, requires 32 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Decrypted string</returns>  
        public static string Decrypt(string data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] encryptedBytes = Convert.FromBase64String(data);
            byte[] decryptBytes = Decrypt(encryptedBytes, key, vector);

            if (decryptBytes == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(decryptBytes);
        }

        /// <summary>  
        ///  AES decrypt
        /// </summary>  
        /// <param name="data">Encrypted data</param>  
        /// <param name="key">Key, requires 16 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Decrypted byte array</returns>  

        public static byte[] Decrypt(byte[] data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] bKey = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length, (char)0x00)), bKey, bKey.Length);

            byte[] bVector = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(bVector.Length, (char)0x00)), bVector, bVector.Length);

            return Decrypt(data, bKey, bVector);
        }

        /// <summary>
        /// AES decrypt
        /// </summary>
        /// <param name="data">Encrypted data</param>
        /// <param name="key">Key, requires 16 bits</param>
        /// <param name="vector">IV,requires 16 bits</param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] vector)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (key == null || key.Length != 16)
            {
                throw new ArgumentException(nameof(key));
            }
            if (vector == null || vector.Length != 16)
            {
                throw new ArgumentException(nameof(vector));
            }

            byte[] decryptedData = null; // decrypted data
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                try
                {
                    using (MemoryStream Memory = new MemoryStream(data))
                    {
                        using (CryptoStream Decryptor = new CryptoStream(Memory, aes.CreateDecryptor(key, vector), CryptoStreamMode.Read))
                        {
                            using (MemoryStream tempMemory = new MemoryStream())
                            {
                                byte[] Buffer = new byte[1024];
                                Int32 readBytes = 0;
                                while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
                                {
                                    tempMemory.Write(Buffer, 0, readBytes);
                                }

                                decryptedData = tempMemory.ToArray();
                            }
                        }
                    }
                }
                catch
                {
                    decryptedData = null;
                }
                return decryptedData;
            }
        }

        public static bool TryDecrypt(string data, string key, string vector, out string result)
        {
            result = null;
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    return false;
                }
                result = Decrypt(data, key, vector);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// 验证
    /// </summary>
    internal class Verify
    {
        internal Verify()
        {
        }

        public static void IsNotEmpty(Guid argument, string argumentName)
        {
            if (argument == Guid.Empty)
            {
                throw new ArgumentException(string.Format("\"{0}\" 不能为空Guid.", argumentName), argumentName);
            }
        }

        public static void IsNotEmpty(string argument, string argumentName)
        {
            if (string.IsNullOrEmpty((argument ?? string.Empty).Trim()))
            {
                throw new ArgumentException(string.Format("\"{0}\" 不能为空.", argumentName), argumentName);
            }
        }

        public static void IsNotOutOfLength(string argument, int length, string argumentName)
        {
            if (argument.Trim().Length > length)
            {
                throw new ArgumentException(string.Format("\"{0}\" 不能超过 {1} 字符.", argumentName, length), argumentName);
            }
        }

        public static void IsNotNull(object argument, string argumentName, string message = "")
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName, message);
            }
        }

        public static void IsNotNegative(int argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(int argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegative(long argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
        public static void IsNotNegativeOrZero(long argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegative(float argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(float argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
        public static void IsNotNegative(decimal argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(decimal argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotInvalidDate(DateTime argument, string argumentName)
        {
            DateTime MinDate = new DateTime(1900, 1, 1);
            DateTime MaxDate = new DateTime(9999, 12, 31, 23, 59, 59, 999);

            if (!((argument >= MinDate) && (argument <= MaxDate)))
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotInPast(DateTime argument, string argumentName)
        {
            if (argument < DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotInFuture(DateTime argument, string argumentName)
        {
            if (argument > DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegative(TimeSpan argument, string argumentName)
        {
            if (argument < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(TimeSpan argument, string argumentName)
        {
            if (argument <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotEmpty<T>(ICollection<T> argument, string argumentName)
        {
            IsNotNull(argument, argumentName, "集合不能为Null");

            if (argument.Count == 0)
            {
                throw new ArgumentException("集合不能为空.", argumentName);
            }
        }
        public static void IsNotOutOfRange(int argument, int min, int max, string argumentName)
        {
            if ((argument < min) || (argument > max))
            {
                throw new ArgumentOutOfRangeException(argumentName, string.Format("{0} 必须在此区间 \"{1}\"-\"{2}\".", argumentName, min, max));
            }
        }

        public static void IsNotExistsFile(string argument, string argumentName)
        {
            IsNotEmpty(argument, argumentName);

            if (!File.Exists(argument))
            {
                throw new ArgumentException(string.Format("\"{0}\" 文件不存在", argumentName), argumentName);
            }
        }

        public static void IsNotExistsDirectory(string argument, string argumentName)
        {
            IsNotEmpty(argument, argumentName);

            if (!Directory.Exists(argument))
            {
                throw new ArgumentException(string.Format("\"{0}\" 目录不存在", argumentName), argumentName);
            }
        }
    }
}
