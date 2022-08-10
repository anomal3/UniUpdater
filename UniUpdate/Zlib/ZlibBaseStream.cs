using System;
using System.IO;

namespace Ionic.Zlib
{
	// Token: 0x02000032 RID: 50
	internal class ZlibBaseStream : Stream
	{
		// Token: 0x0600023E RID: 574 RVA: 0x00015F54 File Offset: 0x00014154
		public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, bool wantRfc1950Header, bool leaveOpen)
		{
			this._flushMode = 0;
			this._workingBuffer = new byte[this.WORKING_BUFFER_SIZE_DEFAULT];
			this._stream = stream;
			this._leaveOpen = leaveOpen;
			if (compressionMode == CompressionMode.Decompress)
			{
				this._z.InitializeInflate(wantRfc1950Header);
				this._wantCompress = false;
			}
			else
			{
				this._z.InitializeDeflate(level, wantRfc1950Header);
				this._wantCompress = true;
			}
		}

		// Token: 0x0600023F RID: 575 RVA: 0x00016014 File Offset: 0x00014214
		public override void WriteByte(byte b)
		{
			this._buf1[0] = b;
			this.Write(this._buf1, 0, 1);
		}

		// Token: 0x06000240 RID: 576 RVA: 0x00016030 File Offset: 0x00014230
		public override void Write(byte[] buffer, int offset, int length)
		{
			if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				this._streamMode = ZlibBaseStream.StreamMode.Writer;
			}
			if (this._streamMode != ZlibBaseStream.StreamMode.Writer)
			{
				throw new ZlibException("Cannot Write after Reading.");
			}
			if (length != 0)
			{
				this._z.InputBuffer = buffer;
				this._z.NextIn = offset;
				this._z.AvailableBytesIn = length;
				for (;;)
				{
					this._z.OutputBuffer = this._workingBuffer;
					this._z.NextOut = 0;
					this._z.AvailableBytesOut = this._workingBuffer.Length;
					int num = this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode);
					if (num != 0 && num != 1)
					{
						break;
					}
					this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
					if (this._z.AvailableBytesIn <= 0 && this._z.AvailableBytesOut != 0)
					{
						return;
					}
				}
				throw new ZlibException((this._wantCompress ? "de" : "in") + "flating: " + this._z.Message);
			}
		}

		// Token: 0x06000241 RID: 577 RVA: 0x000161B4 File Offset: 0x000143B4
		private void finish()
		{
			if (this._streamMode == ZlibBaseStream.StreamMode.Writer)
			{
				for (;;)
				{
					this._z.OutputBuffer = this._workingBuffer;
					this._z.NextOut = 0;
					this._z.AvailableBytesOut = this._workingBuffer.Length;
					int num = this._wantCompress ? this._z.Deflate(4) : this._z.Inflate(4);
					if (num != 1 && num != 0)
					{
						break;
					}
					if (this._workingBuffer.Length - this._z.AvailableBytesOut > 0)
					{
						this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
					}
					if (this._z.AvailableBytesIn <= 0 && this._z.AvailableBytesOut != 0)
					{
						goto Block_7;
					}
				}
				throw new ZlibException((this._wantCompress ? "de" : "in") + "flating: " + this._z.Message);
				Block_7:
				this.Flush();
			}
		}

		// Token: 0x06000242 RID: 578 RVA: 0x00016300 File Offset: 0x00014500
		private void end()
		{
			if (this._z != null)
			{
				if (this._wantCompress)
				{
					this._z.EndDeflate();
				}
				else
				{
					this._z.EndInflate();
				}
				this._z = null;
			}
		}

		// Token: 0x06000243 RID: 579 RVA: 0x00016364 File Offset: 0x00014564
		public override void Close()
		{
			try
			{
				this.finish();
			}
			catch (IOException)
			{
			}
			finally
			{
				this.end();
				if (!this._leaveOpen)
				{
					this._stream.Close();
				}
				this._stream = null;
			}
		}

		// Token: 0x06000244 RID: 580 RVA: 0x000163D8 File Offset: 0x000145D8
		public override void Flush()
		{
			this._stream.Flush();
		}

		// Token: 0x06000245 RID: 581 RVA: 0x000163E8 File Offset: 0x000145E8
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000246 RID: 582 RVA: 0x000163F0 File Offset: 0x000145F0
		public override void SetLength(long value)
		{
			this._stream.SetLength(value);
		}

		// Token: 0x06000247 RID: 583 RVA: 0x00016400 File Offset: 0x00014600
		public int Read()
		{
			int result;
			if (this.Read(this._buf1, 0, 1) == -1)
			{
				result = -1;
			}
			else
			{
				result = (int)(this._buf1[0] & byte.MaxValue);
			}
			return result;
		}

		// Token: 0x06000248 RID: 584 RVA: 0x0001644C File Offset: 0x0001464C
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				this._streamMode = ZlibBaseStream.StreamMode.Reader;
				this._z.AvailableBytesIn = 0;
			}
			if (this._streamMode != ZlibBaseStream.StreamMode.Reader)
			{
				throw new ZlibException("Cannot Read after Writing.");
			}
			if (!this._stream.CanRead)
			{
				throw new ZlibException("The stream is not readable.");
			}
			int result;
			if (count == 0)
			{
				result = 0;
			}
			else
			{
				this._z.OutputBuffer = buffer;
				this._z.NextOut = offset;
				this._z.AvailableBytesOut = count;
				this._z.InputBuffer = this._workingBuffer;
				for (;;)
				{
					if (this._z.AvailableBytesIn == 0 && !this.nomoreinput)
					{
						this._z.NextIn = 0;
						this._z.AvailableBytesIn = SharedUtils.ReadInput(this._stream, this._workingBuffer, 0, this._workingBuffer.Length);
						if (this._z.AvailableBytesIn == -1)
						{
							this._z.AvailableBytesIn = 0;
							this.nomoreinput = true;
						}
					}
					int num = this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode);
					if (this.nomoreinput && num == -5)
					{
						break;
					}
					if (num != 0 && num != 1)
					{
						goto Block_12;
					}
					if ((this.nomoreinput || num == 1) && this._z.AvailableBytesOut == count)
					{
						goto Block_15;
					}
					if (this._z.AvailableBytesOut != count || num != 0)
					{
						goto Block_17;
					}
				}
				return -1;
				Block_12:
				throw new ZlibException((this._wantCompress ? "de" : "in") + "flating: " + this._z.Message);
				Block_15:
				return -1;
				Block_17:
				result = count - this._z.AvailableBytesOut;
			}
			return result;
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000249 RID: 585 RVA: 0x000166A0 File Offset: 0x000148A0
		public override bool CanRead
		{
			get
			{
				return this._stream.CanRead;
			}
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x0600024A RID: 586 RVA: 0x000166C4 File Offset: 0x000148C4
		public override bool CanSeek
		{
			get
			{
				return this._stream.CanSeek;
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600024B RID: 587 RVA: 0x000166E8 File Offset: 0x000148E8
		public override bool CanWrite
		{
			get
			{
				return this._stream.CanWrite;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x0600024C RID: 588 RVA: 0x0001670C File Offset: 0x0001490C
		public override long Length
		{
			get
			{
				return this._stream.Length;
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600024D RID: 589 RVA: 0x00016730 File Offset: 0x00014930
		// (set) Token: 0x0600024E RID: 590 RVA: 0x00016738 File Offset: 0x00014938
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

		// Token: 0x040001EF RID: 495
		protected internal ZlibCodec _z = new ZlibCodec();

		// Token: 0x040001F0 RID: 496
		protected internal readonly int WORKING_BUFFER_SIZE_DEFAULT = 16384;

		// Token: 0x040001F1 RID: 497
		protected internal readonly int WORKING_BUFFER_SIZE_MIN = 128;

		// Token: 0x040001F2 RID: 498
		protected internal ZlibBaseStream.StreamMode _streamMode = ZlibBaseStream.StreamMode.Undefined;

		// Token: 0x040001F3 RID: 499
		protected internal int _flushMode;

		// Token: 0x040001F4 RID: 500
		protected internal bool _leaveOpen;

		// Token: 0x040001F5 RID: 501
		protected internal byte[] _workingBuffer;

		// Token: 0x040001F6 RID: 502
		protected internal byte[] _buf1 = new byte[1];

		// Token: 0x040001F7 RID: 503
		protected internal bool _wantCompress;

		// Token: 0x040001F8 RID: 504
		protected internal Stream _stream;

		// Token: 0x040001F9 RID: 505
		private bool nomoreinput = false;

		// Token: 0x02000039 RID: 57
		internal enum StreamMode
		{
			// Token: 0x04000228 RID: 552
			Writer,
			// Token: 0x04000229 RID: 553
			Reader,
			// Token: 0x0400022A RID: 554
			Undefined
		}
	}
}
