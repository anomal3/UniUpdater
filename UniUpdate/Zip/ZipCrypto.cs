using System;
using System.IO;

namespace Ionic.Zip
{
	// Token: 0x02000010 RID: 16
	internal class ZipCrypto
	{
		// Token: 0x06000078 RID: 120 RVA: 0x00003FF0 File Offset: 0x000021F0
		private ZipCrypto()
		{
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00004020 File Offset: 0x00002220
		public static ZipCrypto ForWrite(string password)
		{
			ZipCrypto zipCrypto = new ZipCrypto();
			if (password == null)
			{
				throw new BadPasswordException("This entry requires a password.");
			}
			zipCrypto.InitCipher(password);
			return zipCrypto;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00004064 File Offset: 0x00002264
		public static ZipCrypto ForRead(string password, ZipEntry e)
		{
			Stream archiveStream = e._archiveStream;
			e._WeakEncryptionHeader = new byte[12];
			byte[] weakEncryptionHeader = e._WeakEncryptionHeader;
			ZipCrypto zipCrypto = new ZipCrypto();
			if (password == null)
			{
				throw new BadPasswordException("This entry requires a password.");
			}
			zipCrypto.InitCipher(password);
			ZipEntry.ReadWeakEncryptionHeader(archiveStream, weakEncryptionHeader);
			byte[] array = zipCrypto.DecryptMessage(weakEncryptionHeader, weakEncryptionHeader.Length);
			if (array[11] != (byte)(e._Crc32 >> 24 & 255))
			{
				if ((e._BitField & 8) != 8)
				{
					throw new BadPasswordException("The password did not match.");
				}
				if (array[11] != (byte)(e._TimeBlob >> 8 & 255))
				{
					throw new BadPasswordException("The password did not match.");
				}
			}
			return zipCrypto;
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00004144 File Offset: 0x00002344
		private byte MagicByte
		{
			get
			{
				ushort num = (ushort)((ushort)(this._Keys[2] & 65535U) | 2);
				return (byte)(num * (num ^ 1) >> 8);
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00004178 File Offset: 0x00002378
		public byte[] DecryptMessage(byte[] cipherText, int length)
		{
			if (cipherText == null)
			{
				throw new ZipException("Cannot decrypt.", new ArgumentException("Bad length during Decryption: cipherText must be non-null.", "cipherText"));
			}
			if (length > cipherText.Length)
			{
				throw new ZipException("Cannot decrypt.", new ArgumentException("Bad length during Decryption: the length parameter must be smaller than or equal to the size of the destination array.", "length"));
			}
			byte[] array = new byte[length];
			for (int i = 0; i < length; i++)
			{
				byte b = (byte)(cipherText[i] ^ this.MagicByte);
				this.UpdateKeys(b);
				array[i] = b;
			}
			return array;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x0000421C File Offset: 0x0000241C
		public byte[] EncryptMessage(byte[] plaintext, int length)
		{
			if (plaintext == null)
			{
				throw new ZipException("Cannot encrypt.", new ArgumentException("Bad length during Encryption: the plainText must be non-null.", "plaintext"));
			}
			if (length > plaintext.Length)
			{
				throw new ZipException("Cannot encrypt.", new ArgumentException("Bad length during Encryption: The length parameter must be smaller than or equal to the size of the destination array.", "length"));
			}
			byte[] array = new byte[length];
			for (int i = 0; i < length; i++)
			{
				byte byeValue = plaintext[i];
				array[i] = (byte)(plaintext[i] ^ this.MagicByte);
				this.UpdateKeys(byeValue);
			}
			return array;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000042C0 File Offset: 0x000024C0
		public void InitCipher(string passphrase)
		{
			byte[] array = SharedUtilities.StringToByteArray(passphrase);
			for (int i = 0; i < passphrase.Length; i++)
			{
				this.UpdateKeys(array[i]);
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000042FC File Offset: 0x000024FC
		private void UpdateKeys(byte byeValue)
		{
			this._Keys[0] = (uint)this.crc32.ComputeCrc32(this._Keys[0], byeValue);
			this._Keys[1] = this._Keys[1] + (uint)((byte)this._Keys[0]);
			this._Keys[1] = this._Keys[1] * 134775813U + 1U;
			this._Keys[2] = (uint)this.crc32.ComputeCrc32(this._Keys[2], (byte)(this._Keys[1] >> 24));
		}

		// Token: 0x04000037 RID: 55
		private uint[] _Keys = new uint[]
		{
			305419896U,
			591751049U,
			878082192U
		};

		// Token: 0x04000038 RID: 56
		private CRC32 crc32 = new CRC32();
	}
}
