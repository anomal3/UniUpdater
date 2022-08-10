using System;
using System.IO;

namespace Ionic.Zip
{
	// Token: 0x02000007 RID: 7
	public class CrcCalculatorStream : Stream
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600002E RID: 46 RVA: 0x00003230 File Offset: 0x00001430
		public long TotalBytesSlurped
		{
			get
			{
				return this._Crc32.TotalBytesRead;
			}
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003254 File Offset: 0x00001454
		public CrcCalculatorStream(Stream stream)
		{
			this._InnerStream = stream;
			this._Crc32 = new CRC32();
		}

		// Token: 0x06000030 RID: 48 RVA: 0x0000327C File Offset: 0x0000147C
		public CrcCalculatorStream(Stream stream, long length)
		{
			this._InnerStream = stream;
			this._Crc32 = new CRC32();
			this._length = length;
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000031 RID: 49 RVA: 0x000032A8 File Offset: 0x000014A8
		public int Crc32
		{
			get
			{
				return this._Crc32.Crc32Result;
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000032CC File Offset: 0x000014CC
		public override int Read(byte[] buffer, int offset, int count)
		{
			int count2 = count;
			if (this._length != 0L)
			{
				if (this._Crc32.TotalBytesRead >= this._length)
				{
					return 0;
				}
				long num = this._length - this._Crc32.TotalBytesRead;
				if (num < (long)count)
				{
					count2 = (int)num;
				}
			}
			int num2 = this._InnerStream.Read(buffer, offset, count2);
			if (num2 > 0)
			{
				this._Crc32.SlurpBlock(buffer, offset, num2);
			}
			return num2;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00003374 File Offset: 0x00001574
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count > 0)
			{
				this._Crc32.SlurpBlock(buffer, offset, count);
			}
			this._InnerStream.Write(buffer, offset, count);
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000034 RID: 52 RVA: 0x000033B4 File Offset: 0x000015B4
		public override bool CanRead
		{
			get
			{
				return this._InnerStream.CanRead;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000035 RID: 53 RVA: 0x000033D8 File Offset: 0x000015D8
		public override bool CanSeek
		{
			get
			{
				return this._InnerStream.CanSeek;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000036 RID: 54 RVA: 0x000033FC File Offset: 0x000015FC
		public override bool CanWrite
		{
			get
			{
				return this._InnerStream.CanWrite;
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00003420 File Offset: 0x00001620
		public override void Flush()
		{
			this._InnerStream.Flush();
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00003430 File Offset: 0x00001630
		public override long Length
		{
			get
			{
				if (this._length == 0L)
				{
					throw new NotImplementedException();
				}
				return this._length;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000039 RID: 57 RVA: 0x0000346C File Offset: 0x0000166C
		// (set) Token: 0x0600003A RID: 58 RVA: 0x00003490 File Offset: 0x00001690
		public override long Position
		{
			get
			{
				return this._Crc32.TotalBytesRead;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003498 File Offset: 0x00001698
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000034A0 File Offset: 0x000016A0
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400002E RID: 46
		private Stream _InnerStream;

		// Token: 0x0400002F RID: 47
		private CRC32 _Crc32;

		// Token: 0x04000030 RID: 48
		private long _length = 0L;
	}
}
