using System;
using System.IO;

namespace Ionic.Zlib
{
	// Token: 0x02000030 RID: 48
	public class DeflateStream : Stream
	{
		// Token: 0x06000212 RID: 530 RVA: 0x00015AD4 File Offset: 0x00013CD4
		public DeflateStream(Stream stream, CompressionMode mode) : this(stream, mode, CompressionLevel.DEFAULT, false)
		{
		}

		// Token: 0x06000213 RID: 531 RVA: 0x00015AE4 File Offset: 0x00013CE4
		public DeflateStream(Stream stream, CompressionMode mode, CompressionLevel level) : this(stream, mode, level, false)
		{
		}

		// Token: 0x06000214 RID: 532 RVA: 0x00015AF4 File Offset: 0x00013CF4
		public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen) : this(stream, mode, CompressionLevel.DEFAULT, leaveOpen)
		{
		}

		// Token: 0x06000215 RID: 533 RVA: 0x00015B04 File Offset: 0x00013D04
		public DeflateStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
			this._baseStream = new ZlibBaseStream(stream, mode, level, false, leaveOpen);
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000216 RID: 534 RVA: 0x00015B20 File Offset: 0x00013D20
		// (set) Token: 0x06000217 RID: 535 RVA: 0x00015B44 File Offset: 0x00013D44
		public virtual int FlushMode
		{
			get
			{
				return this._baseStream._flushMode;
			}
			set
			{
				this._baseStream._flushMode = value;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000218 RID: 536 RVA: 0x00015B54 File Offset: 0x00013D54
		// (set) Token: 0x06000219 RID: 537 RVA: 0x00015B7C File Offset: 0x00013D7C
		public int BufferSize
		{
			get
			{
				return this._baseStream._workingBuffer.Length;
			}
			set
			{
				if (value < this._baseStream.WORKING_BUFFER_SIZE_MIN)
				{
					throw new ZlibException("Don't be silly. Use a bigger buffer.");
				}
				this._baseStream._workingBuffer = new byte[value];
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600021A RID: 538 RVA: 0x00015BC4 File Offset: 0x00013DC4
		public virtual long TotalIn
		{
			get
			{
				return this._baseStream._z.TotalBytesIn;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600021B RID: 539 RVA: 0x00015BF0 File Offset: 0x00013DF0
		public virtual long TotalOut
		{
			get
			{
				return this._baseStream._z.TotalBytesOut;
			}
		}

		// Token: 0x0600021C RID: 540 RVA: 0x00015C1C File Offset: 0x00013E1C
		public override void Close()
		{
			this._baseStream.Close();
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x0600021D RID: 541 RVA: 0x00015C2C File Offset: 0x00013E2C
		public override bool CanRead
		{
			get
			{
				return this._baseStream._stream.CanRead;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x0600021E RID: 542 RVA: 0x00015C58 File Offset: 0x00013E58
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x0600021F RID: 543 RVA: 0x00015C74 File Offset: 0x00013E74
		public override bool CanWrite
		{
			get
			{
				return this._baseStream._stream.CanWrite;
			}
		}

		// Token: 0x06000220 RID: 544 RVA: 0x00015CA0 File Offset: 0x00013EA0
		public override void Flush()
		{
			this._baseStream.Flush();
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000221 RID: 545 RVA: 0x00015CB0 File Offset: 0x00013EB0
		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000222 RID: 546 RVA: 0x00015CB8 File Offset: 0x00013EB8
		// (set) Token: 0x06000223 RID: 547 RVA: 0x00015CC0 File Offset: 0x00013EC0
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

		// Token: 0x06000224 RID: 548 RVA: 0x00015CC8 File Offset: 0x00013EC8
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this._baseStream.Read(buffer, offset, count);
		}

		// Token: 0x06000225 RID: 549 RVA: 0x00015CF0 File Offset: 0x00013EF0
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000226 RID: 550 RVA: 0x00015CF8 File Offset: 0x00013EF8
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000227 RID: 551 RVA: 0x00015D00 File Offset: 0x00013F00
		public override void Write(byte[] buffer, int offset, int count)
		{
			this._baseStream.Write(buffer, offset, count);
		}

		// Token: 0x040001ED RID: 493
		internal ZlibBaseStream _baseStream;
	}
}
