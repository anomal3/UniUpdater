using System;
using System.IO;
using System.Security.Cryptography;

namespace Ionic.Zip
{
	// Token: 0x02000002 RID: 2
	internal class WinZipAesCrypto
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
		private WinZipAesCrypto(string password, int KeyStrengthInBits)
		{
			this._Password = password;
			this._KeyStrengthInBits = KeyStrengthInBits;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002074 File Offset: 0x00000274
		private WinZipAesCrypto()
		{
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002094 File Offset: 0x00000294
		public static WinZipAesCrypto Generate(string password, int KeyStrengthInBits)
		{
			WinZipAesCrypto winZipAesCrypto = new WinZipAesCrypto(password, KeyStrengthInBits);
			int num = winZipAesCrypto._KeyStrengthInBytes / 2;
			winZipAesCrypto._Salt = new byte[num];
			Random random = new Random();
			random.NextBytes(winZipAesCrypto._Salt);
			return winZipAesCrypto;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020E0 File Offset: 0x000002E0
		public static WinZipAesCrypto ReadFromStream(string password, int KeyStrengthInBits, Stream s)
		{
			WinZipAesCrypto winZipAesCrypto = new WinZipAesCrypto(password, KeyStrengthInBits);
			int num = winZipAesCrypto._KeyStrengthInBytes / 2;
			winZipAesCrypto._Salt = new byte[num];
			winZipAesCrypto._providedPv = new byte[2];
			int num2 = s.Read(winZipAesCrypto._Salt, 0, winZipAesCrypto._Salt.Length);
			num2 = s.Read(winZipAesCrypto._providedPv, 0, winZipAesCrypto._providedPv.Length);
			winZipAesCrypto.PasswordVerificationStored = (short)((int)winZipAesCrypto._providedPv[0] + (int)winZipAesCrypto._providedPv[1] * 256);
			if (password != null)
			{
				winZipAesCrypto.PasswordVerificationGenerated = (short)((int)winZipAesCrypto.GeneratedPV[0] + (int)winZipAesCrypto.GeneratedPV[1] * 256);
				if (winZipAesCrypto.PasswordVerificationGenerated != winZipAesCrypto.PasswordVerificationStored)
				{
					throw new BadPasswordException("bad password");
				}
			}
			return winZipAesCrypto;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000005 RID: 5 RVA: 0x000021BC File Offset: 0x000003BC
		public byte[] GeneratedPV
		{
			get
			{
				if (!this._cryptoGenerated)
				{
					this._GenerateCryptoBytes();
				}
				return this._generatedPv;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000006 RID: 6 RVA: 0x000021F0 File Offset: 0x000003F0
		public byte[] Salt
		{
			get
			{
				return this._Salt;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000007 RID: 7 RVA: 0x00002210 File Offset: 0x00000410
		private int _KeyStrengthInBytes
		{
			get
			{
				return this._KeyStrengthInBits / 8;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002234 File Offset: 0x00000434
		public int SizeOfEncryptionMetadata
		{
			get
			{
				return this._KeyStrengthInBytes / 2 + 10 + 2;
			}
		}

		// Token: 0x17000005 RID: 5
		// (set) Token: 0x06000009 RID: 9 RVA: 0x0000225C File Offset: 0x0000045C
		public string Password
		{
			set
			{
				this._Password = value;
				if (this._Password != null)
				{
					this.PasswordVerificationGenerated = (short)((int)this.GeneratedPV[0] + (int)this.GeneratedPV[1] * 256);
					if (this.PasswordVerificationGenerated != this.PasswordVerificationStored)
					{
						throw new Exception("bad password");
					}
				}
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000022C8 File Offset: 0x000004C8
		private void _GenerateCryptoBytes()
		{
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(this._Password, this.Salt, this.Rfc2898KeygenIterations);
			this._keyBytes = rfc2898DeriveBytes.GetBytes(this._KeyStrengthInBytes);
			this._MacInitializationVector = rfc2898DeriveBytes.GetBytes(this._KeyStrengthInBytes);
			this._generatedPv = rfc2898DeriveBytes.GetBytes(2);
			this._cryptoGenerated = true;
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000B RID: 11 RVA: 0x0000232C File Offset: 0x0000052C
		public byte[] KeyBytes
		{
			get
			{
				if (!this._cryptoGenerated)
				{
					this._GenerateCryptoBytes();
				}
				return this._keyBytes;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002360 File Offset: 0x00000560
		public byte[] MacIv
		{
			get
			{
				if (!this._cryptoGenerated)
				{
					this._GenerateCryptoBytes();
				}
				return this._MacInitializationVector;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600000D RID: 13 RVA: 0x00002394 File Offset: 0x00000594
		public byte[] StoredMac
		{
			get
			{
				return this._StoredMac;
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000023B4 File Offset: 0x000005B4
		public void ReadAndVerifyMac(Stream s)
		{
			bool flag = false;
			long position = s.Position;
			this._StoredMac = new byte[10];
			int num = s.Read(this._StoredMac, 0, this._StoredMac.Length);
			if (this._StoredMac.Length != this.CalculatedMac.Length)
			{
				flag = true;
			}
			if (!flag)
			{
				for (int i = 0; i < this._StoredMac.Length; i++)
				{
					if (this._StoredMac[i] != this.CalculatedMac[i])
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				throw new Exception(string.Format("The MAC does not match '{0}' != '{1}'", Util.FormatByteArray(this._StoredMac), Util.FormatByteArray(this.CalculatedMac)));
			}
		}

		// Token: 0x04000001 RID: 1
		internal byte[] _Salt;

		// Token: 0x04000002 RID: 2
		internal byte[] _providedPv;

		// Token: 0x04000003 RID: 3
		internal byte[] _generatedPv;

		// Token: 0x04000004 RID: 4
		internal int _KeyStrengthInBits;

		// Token: 0x04000005 RID: 5
		private byte[] _MacInitializationVector;

		// Token: 0x04000006 RID: 6
		private byte[] _StoredMac;

		// Token: 0x04000007 RID: 7
		private byte[] _keyBytes;

		// Token: 0x04000008 RID: 8
		private short PasswordVerificationStored;

		// Token: 0x04000009 RID: 9
		private short PasswordVerificationGenerated;

		// Token: 0x0400000A RID: 10
		private int Rfc2898KeygenIterations = 1000;

		// Token: 0x0400000B RID: 11
		private string _Password;

		// Token: 0x0400000C RID: 12
		private bool _cryptoGenerated = false;

		// Token: 0x0400000D RID: 13
		public byte[] CalculatedMac;
	}
}
