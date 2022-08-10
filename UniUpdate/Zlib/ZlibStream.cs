using System;
using System.IO;

namespace Ionic.Zlib
{
	// Token: 0x02000031 RID: 49
	public class ZlibStream : Stream
	{
		// Token: 0x06000228 RID: 552 RVA: 0x00015D14 File Offset: 0x00013F14
		public ZlibStream(Stream stream, CompressionMode mode) : this(stream, mode, CompressionLevel.DEFAULT, false)
		{
		}

		// Token: 0x06000229 RID: 553 RVA: 0x00015D24 File Offset: 0x00013F24
		public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level) : this(stream, mode, level, false)
		{
		}

		// Token: 0x0600022A RID: 554 RVA: 0x00015D34 File Offset: 0x00013F34
		public ZlibStream(Stream stream, CompressionMode mode, bool leaveOpen) : this(stream, mode, CompressionLevel.DEFAULT, leaveOpen)
		{
		}

		// Token: 0x0600022B RID: 555 RVA: 0x00015D44 File Offset: 0x00013F44
		public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
		{
			this._baseStream = new ZlibBaseStream(stream, mode, level, true, leaveOpen);
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600022C RID: 556 RVA: 0x00015D60 File Offset: 0x00013F60
		// (set) Token: 0x0600022D RID: 557 RVA: 0x00015D84 File Offset: 0x00013F84
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

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600022E RID: 558 RVA: 0x00015D94 File Offset: 0x00013F94
		// (set) Token: 0x0600022F RID: 559 RVA: 0x00015DBC File Offset: 0x00013FBC
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

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000230 RID: 560 RVA: 0x00015E04 File Offset: 0x00014004
		public virtual long TotalIn
		{
			get
			{
				return this._baseStream._z.TotalBytesIn;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000231 RID: 561 RVA: 0x00015E30 File Offset: 0x00014030
		public virtual long TotalOut
		{
			get
			{
				return this._baseStream._z.TotalBytesOut;
			}
		}

		// Token: 0x06000232 RID: 562 RVA: 0x00015E5C File Offset: 0x0001405C
		public override void Close()
		{
			this._baseStream.Close();
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000233 RID: 563 RVA: 0x00015E6C File Offset: 0x0001406C
		public override bool CanRead
		{
			get
			{
				return this._baseStream._stream.CanRead;
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000234 RID: 564 RVA: 0x00015E98 File Offset: 0x00014098
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000235 RID: 565 RVA: 0x00015EB4 File Offset: 0x000140B4
		public override bool CanWrite
		{
			get
			{
				return this._baseStream._stream.CanWrite;
			}
		}

		// Token: 0x06000236 RID: 566 RVA: 0x00015EE0 File Offset: 0x000140E0
		public override void Flush()
		{
			this._baseStream.Flush();
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x06000237 RID: 567 RVA: 0x00015EF0 File Offset: 0x000140F0
		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000238 RID: 568 RVA: 0x00015EF8 File Offset: 0x000140F8
		// (set) Token: 0x06000239 RID: 569 RVA: 0x00015F00 File Offset: 0x00014100
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

		// Token: 0x0600023A RID: 570 RVA: 0x00015F08 File Offset: 0x00014108
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this._baseStream.Read(buffer, offset, count);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x00015F30 File Offset: 0x00014130
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600023C RID: 572 RVA: 0x00015F38 File Offset: 0x00014138
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600023D RID: 573 RVA: 0x00015F40 File Offset: 0x00014140
		public override void Write(byte[] buffer, int offset, int count)
		{
			this._baseStream.Write(buffer, offset, count);
		}

		// Token: 0x040001EE RID: 494
		internal ZlibBaseStream _baseStream;
	}
}
