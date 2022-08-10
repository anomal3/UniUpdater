using System;
using System.IO;

namespace Ionic.Zip
{
	// Token: 0x02000012 RID: 18
	internal class ZipCipherStream : Stream
	{
		// Token: 0x06000080 RID: 128 RVA: 0x00004384 File Offset: 0x00002584
		public ZipCipherStream(Stream s, ZipCrypto cipher, CryptoMode mode)
		{
			this._cipher = cipher;
			this._s = s;
			this._mode = mode;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000043A4 File Offset: 0x000025A4
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._mode == CryptoMode.Encrypt)
			{
				throw new NotImplementedException();
			}
			byte[] array = new byte[count];
			int num = this._s.Read(array, 0, count);
			byte[] array2 = this._cipher.DecryptMessage(array, num);
			for (int i = 0; i < num; i++)
			{
				buffer[offset + i] = array2[i];
			}
			return num;
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00004420 File Offset: 0x00002620
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this._mode == CryptoMode.Decrypt)
			{
				throw new NotImplementedException();
			}
			if (count != 0)
			{
				byte[] array;
				if (offset != 0)
				{
					array = new byte[count];
					for (int i = 0; i < count; i++)
					{
						array[i] = buffer[offset + i];
					}
				}
				else
				{
					array = buffer;
				}
				byte[] array2 = this._cipher.EncryptMessage(array, count);
				this._s.Write(array2, 0, array2.Length);
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000083 RID: 131 RVA: 0x000044B8 File Offset: 0x000026B8
		public override bool CanRead
		{
			get
			{
				return this._mode == CryptoMode.Decrypt;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000084 RID: 132 RVA: 0x000044DC File Offset: 0x000026DC
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000085 RID: 133 RVA: 0x000044F8 File Offset: 0x000026F8
		public override bool CanWrite
		{
			get
			{
				return this._mode == CryptoMode.Encrypt;
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x0000451C File Offset: 0x0000271C
		public override void Flush()
		{
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000087 RID: 135 RVA: 0x00004520 File Offset: 0x00002720
		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000088 RID: 136 RVA: 0x00004528 File Offset: 0x00002728
		// (set) Token: 0x06000089 RID: 137 RVA: 0x00004530 File Offset: 0x00002730
		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00004538 File Offset: 0x00002738
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00004540 File Offset: 0x00002740
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400003C RID: 60
		private ZipCrypto _cipher;

		// Token: 0x0400003D RID: 61
		private Stream _s;

		// Token: 0x0400003E RID: 62
		private CryptoMode _mode;
	}
}
