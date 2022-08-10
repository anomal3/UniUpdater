using System;
using System.IO;
using System.Security.Cryptography;

namespace Ionic.Zip
{
	// Token: 0x02000004 RID: 4
	internal class WinZipAesCipherStream : Stream
	{
		// Token: 0x06000012 RID: 18 RVA: 0x0000264C File Offset: 0x0000084C
		internal WinZipAesCipherStream(Stream s, WinZipAesCrypto cryptoParams, long length, CryptoMode mode) : this(s, cryptoParams, mode)
		{
			this._length = length;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002664 File Offset: 0x00000864
		internal WinZipAesCipherStream(Stream s, WinZipAesCrypto cryptoParams, CryptoMode mode)
		{
			this._params = cryptoParams;
			this._s = s;
			this._mode = mode;
			this._nonce = 1;
			if (this._params == null)
			{
				throw new BadPasswordException("Supply a password to use AES encryption.");
			}
			int num = this._params.KeyBytes.Length * 8;
			if (num != 256 && num != 128 && num != 192)
			{
				throw new Exception("Invalid key size");
			}
			this._mac = new HMACSHA1(this._params.MacIv);
			this._aesCipher = new RijndaelManaged();
			this._aesCipher.BlockSize = 128;
			this._aesCipher.KeySize = num;
			this._aesCipher.Mode = CipherMode.ECB;
			this._aesCipher.Padding = PaddingMode.None;
			byte[] rgbIV = new byte[16];
			this._xform = this._aesCipher.CreateEncryptor(this._params.KeyBytes, rgbIV);
			if (this._mode == CryptoMode.Encrypt)
			{
				this._PendingWriteBuffer = new byte[16];
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000027D4 File Offset: 0x000009D4
		private int ProcessOneBlockWriting(byte[] buffer, int offset, int last)
		{
			if (this._finalBlock)
			{
				throw new Exception("The final block has already been transformed.");
			}
			int num = last - offset;
			int num2 = (num > 16) ? 16 : num;
			Array.Copy(BitConverter.GetBytes(this._nonce++), 0, this.counter, 0, 4);
			if (num2 == last - offset)
			{
				if (this._NextXformWillBeFinal)
				{
					this.counterOut = this._xform.TransformFinalBlock(this.counter, 0, 16);
					this._finalBlock = true;
				}
				else if (buffer != this._PendingWriteBuffer || num2 != 16)
				{
					Array.Copy(buffer, offset, this._PendingWriteBuffer, this._pendingCount, num2);
					this._pendingCount += num2;
					this._nonce--;
					return 0;
				}
			}
			if (!this._finalBlock)
			{
				this._xform.TransformBlock(this.counter, 0, 16, this.counterOut, 0);
			}
			for (int i = 0; i < num2; i++)
			{
				buffer[offset + i] = (byte)(this.counterOut[i] ^ buffer[offset + i]);
			}
			if (this._finalBlock)
			{
				this._mac.TransformFinalBlock(buffer, offset, num2);
			}
			else
			{
				this._mac.TransformBlock(buffer, offset, num2, null, 0);
			}
			return num2;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002984 File Offset: 0x00000B84
		private int ProcessOneBlockReading(byte[] buffer, int offset, int count)
		{
			if (this._finalBlock)
			{
				throw new NotSupportedException();
			}
			int num = count - offset;
			int num2 = (num > 16) ? 16 : num;
			if (this._length > 0L)
			{
				if (this._totalBytesXferred + (long)count == this._length && num2 == num)
				{
					this._NextXformWillBeFinal = true;
				}
			}
			Array.Copy(BitConverter.GetBytes(this._nonce++), 0, this.counter, 0, 4);
			if (this._NextXformWillBeFinal && num2 == count - offset)
			{
				this._mac.TransformFinalBlock(buffer, offset, num2);
				this.counterOut = this._xform.TransformFinalBlock(this.counter, 0, 16);
				this._finalBlock = true;
			}
			else
			{
				this._mac.TransformBlock(buffer, offset, num2, null, 0);
				this._xform.TransformBlock(this.counter, 0, 16, this.counterOut, 0);
			}
			for (int i = 0; i < num2; i++)
			{
				buffer[offset + i] = (byte)(this.counterOut[i] ^ buffer[offset + i]);
			}
			return num2;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002AF0 File Offset: 0x00000CF0
		public void NotifyFinal()
		{
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002AF4 File Offset: 0x00000CF4
		private void TransformInPlace(byte[] buffer, int offset, int count)
		{
			int num = offset;
			while (num < buffer.Length && num < count + offset)
			{
				if (this._mode == CryptoMode.Encrypt)
				{
					this.ProcessOneBlockWriting(buffer, num, count + offset);
				}
				else
				{
					this.ProcessOneBlockReading(buffer, num, count + offset);
				}
				num += 16;
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002B60 File Offset: 0x00000D60
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._mode == CryptoMode.Encrypt)
			{
				throw new NotSupportedException();
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || count < 0)
			{
				throw new ArgumentException("Invalid parameters");
			}
			if (buffer.Length < offset + count)
			{
				throw new ArgumentException("The buffer is too small");
			}
			int count2 = count;
			int result;
			if (this._totalBytesXferred >= this._length)
			{
				result = 0;
			}
			else
			{
				long num = this._length - this._totalBytesXferred;
				if (num < (long)count)
				{
					count2 = (int)num;
				}
				int num2 = this._s.Read(buffer, offset, count2);
				this.TransformInPlace(buffer, offset, count2);
				this._totalBytesXferred += (long)num2;
				result = num2;
			}
			return result;
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002C5C File Offset: 0x00000E5C
		public byte[] FinalAuthentication
		{
			get
			{
				byte[] result;
				if (!this._finalBlock)
				{
					if (this._totalBytesXferred != 0L)
					{
						throw new Exception("The final hash has not been computed.");
					}
					byte[] array = new byte[10];
					result = array;
				}
				else
				{
					byte[] array2 = new byte[10];
					Array.Copy(this._mac.Hash, 0, array2, 0, 10);
					result = array2;
				}
				return result;
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002CD0 File Offset: 0x00000ED0
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this._mode == CryptoMode.Decrypt)
			{
				throw new NotSupportedException();
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || count < 0)
			{
				throw new ArgumentException("Invalid parameters");
			}
			if (buffer.Length < offset + count)
			{
				throw new ArgumentException("The offset and count are too large");
			}
			if (count != 0)
			{
				if (this._pendingCount != 0)
				{
					if (count + this._pendingCount <= 16)
					{
						Array.Copy(buffer, offset, this._PendingWriteBuffer, this._pendingCount, count);
						this._pendingCount += count;
						return;
					}
					int num = 16 - this._pendingCount;
					Array.Copy(buffer, offset, this._PendingWriteBuffer, this._pendingCount, num);
					this._pendingCount = 0;
					offset += num;
					count -= num;
					this.ProcessOneBlockWriting(this._PendingWriteBuffer, 0, 16);
					this._s.Write(this._PendingWriteBuffer, 0, 16);
					this._totalBytesXferred += 16L;
				}
				this.TransformInPlace(buffer, offset, count);
				this._s.Write(buffer, offset, count - this._pendingCount);
				this._totalBytesXferred += (long)(count - this._pendingCount);
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002E54 File Offset: 0x00001054
		public override void Close()
		{
			if (this._pendingCount != 0)
			{
				this._NextXformWillBeFinal = true;
				this.ProcessOneBlockWriting(this._PendingWriteBuffer, 0, this._pendingCount);
				this._s.Write(this._PendingWriteBuffer, 0, this._pendingCount);
				this._totalBytesXferred += (long)this._pendingCount;
			}
			this._s.Close();
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600001C RID: 28 RVA: 0x00002ECC File Offset: 0x000010CC
		public override bool CanRead
		{
			get
			{
				return this._mode == CryptoMode.Decrypt;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002F00 File Offset: 0x00001100
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600001E RID: 30 RVA: 0x00002F1C File Offset: 0x0000111C
		public override bool CanWrite
		{
			get
			{
				return this._mode == CryptoMode.Encrypt;
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002F40 File Offset: 0x00001140
		public override void Flush()
		{
			this._s.Flush();
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002F50 File Offset: 0x00001150
		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002F58 File Offset: 0x00001158
		// (set) Token: 0x06000022 RID: 34 RVA: 0x00002F60 File Offset: 0x00001160
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

		// Token: 0x06000023 RID: 35 RVA: 0x00002F68 File Offset: 0x00001168
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002F70 File Offset: 0x00001170
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400000E RID: 14
		private const int BLOCK_SIZE_IN_BYTES = 16;

		// Token: 0x0400000F RID: 15
		private WinZipAesCrypto _params;

		// Token: 0x04000010 RID: 16
		private Stream _s;

		// Token: 0x04000011 RID: 17
		private CryptoMode _mode;

		// Token: 0x04000012 RID: 18
		private int _nonce;

		// Token: 0x04000013 RID: 19
		private bool _finalBlock = false;

		// Token: 0x04000014 RID: 20
		private bool _NextXformWillBeFinal = false;

		// Token: 0x04000015 RID: 21
		internal HMACSHA1 _mac;

		// Token: 0x04000016 RID: 22
		internal RijndaelManaged _aesCipher;

		// Token: 0x04000017 RID: 23
		internal ICryptoTransform _xform;

		// Token: 0x04000018 RID: 24
		private byte[] counter = new byte[16];

		// Token: 0x04000019 RID: 25
		private byte[] counterOut = new byte[16];

		// Token: 0x0400001A RID: 26
		private long _length;

		// Token: 0x0400001B RID: 27
		private long _totalBytesXferred = 0L;

		// Token: 0x0400001C RID: 28
		private byte[] _PendingWriteBuffer;

		// Token: 0x0400001D RID: 29
		private int _pendingCount = 0;
	}
}
