using System;
using System.Extensions;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bilibili.Settings;

namespace Bilibili.Api {
	internal static class ApiUtils {
		private static readonly MD5 _md5 = MD5.Create();

		public static ulong GetTimeStamp() {
			TimeSpan timeSpan;

			timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
			return (ulong)timeSpan.TotalSeconds;
		}

		public static void SortAndSign(this QueryCollection queries) {
			queries.Sort((x, y) => x.Key[0] - y.Key[0]);
			queries.Add("sign", ComputeSign(queries.ToQueryString()));
		}

		private static string ComputeSign(string text) {
			byte[] hash;
			StringBuilder sb;

			hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(text + GlobalSettings.Bilibili.AppSecret));
			sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
				sb.Append(hash[i].ToString("x2"));
			return sb.ToString();
		}

		public static string RsaEncrypt(string text, RSAParameters rsaParameters) {
			using (RSA rsa = RSA.Create()) {
				rsa.ImportParameters(rsaParameters);
				return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.Pkcs1));
			}
		}

		public static RSAParameters ParsePublicKey(string publicKey) {
			if (string.IsNullOrEmpty(publicKey))
				throw new ArgumentNullException(nameof(publicKey));

			publicKey = publicKey.Replace("\n", string.Empty);
			publicKey = publicKey.Substring(26, publicKey.Length - 50);
			using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(publicKey))) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					ushort i16;
					byte[] oid;
					byte i8;
					byte low = 0x00;
					byte high = 0x00;
					int modulusLength;
					byte[] modulus;
					int exponentLength;
					byte[] exponent;

					i16 = reader.ReadUInt16();
					if (i16 == 0x8130)
						reader.ReadByte();
					else if (i16 == 0x8230)
						reader.ReadInt16();
					else
						throw new ArgumentException(nameof(publicKey));
					oid = reader.ReadBytes(15);
					if (!oid.SequenceEqual(new byte[] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 }))
						throw new ArgumentException(nameof(publicKey));
					i16 = reader.ReadUInt16();
					if (i16 == 0x8103)
						reader.ReadByte();
					else if (i16 == 0x8203)
						reader.ReadInt16();
					else
						throw new ArgumentException(nameof(publicKey));
					i8 = reader.ReadByte();
					if (i8 != 0x00)
						throw new ArgumentException(nameof(publicKey));
					i16 = reader.ReadUInt16();
					if (i16 == 0x8130)
						reader.ReadByte();
					else if (i16 == 0x8230)
						reader.ReadInt16();
					else
						throw new ArgumentException(nameof(publicKey));
					i16 = reader.ReadUInt16();
					if (i16 == 0x8102)
						low = reader.ReadByte();
					else if (i16 == 0x8202) {
						high = reader.ReadByte();
						low = reader.ReadByte();
					}
					else
						throw new ArgumentException(nameof(publicKey));
					modulusLength = BitConverter.ToInt32(new byte[] { low, high, 0x00, 0x00 }, 0);
					if (reader.PeekChar() == 0x00) {
						reader.ReadByte();
						modulusLength -= 1;
					}
					modulus = reader.ReadBytes(modulusLength);
					if (reader.ReadByte() != 0x02)
						throw new ArgumentException(nameof(publicKey));
					exponentLength = reader.ReadByte();
					exponent = reader.ReadBytes(exponentLength);
					return new RSAParameters {
						Modulus = modulus,
						Exponent = exponent
					};
				}
			}
		}
	}
}
