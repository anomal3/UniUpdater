using System;
using System.IO;

namespace Ionic.Zip
{
	// Token: 0x0200000F RID: 15
	internal class CountingStream : Stream
	{
		// Token: 0x06000069 RID: 105 RVA: 0x00003E1C File Offset: 0x0000201C
		public CountingStream(Stream s)
		{
			this._s = s;
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600006A RID: 106 RVA: 0x00003E30 File Offset: 0x00002030
		public long BytesWritten
		{
			get
			{
				return this._bytesWritten;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00003E50 File Offset: 0x00002050
		public long BytesRead
		{
			get
			{
				return this._bytesRead;
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00003E70 File Offset: 0x00002070
		public void Adjust(long delta)
		{
			this._bytesWritten -= delta;
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00003E84 File Offset: 0x00002084
		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = this._s.Read(buffer, offset, count);
			this._bytesRead += (long)num;
			return num;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00003EBC File Offset: 0x000020BC
		public override void Write(byte[] buffer, int offset, int count)
		{
			this._s.Write(buffer, offset, count);
			this._bytesWritten += (long)count;
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00003EE0 File Offset: 0x000020E0
		public override bool CanRead
		{
			get
			{
				return this._s.CanRead;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000070 RID: 112 RVA: 0x00003F04 File Offset: 0x00002104
		public override bool CanSeek
		{
			get
			{
				return this._s.CanSeek;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000071 RID: 113 RVA: 0x00003F28 File Offset: 0x00002128
		public override bool CanWrite
		{
			get
			{
				return this._s.CanWrite;
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00003F4C File Offset: 0x0000214C
		public override void Flush()
		{
			this._s.Flush();
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000073 RID: 115 RVA: 0x00003F5C File Offset: 0x0000215C
		public override long Length
		{
			get
			{
				return this._s.Length;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00003F80 File Offset: 0x00002180
		// (set) Token: 0x06000075 RID: 117 RVA: 0x00003FA4 File Offset: 0x000021A4
		public override long Position
		{
			get
			{
				return this._s.Position;
			}
			set
			{
				this._s.Seek(value, SeekOrigin.Begin);
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00003FB8 File Offset: 0x000021B8
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this._s.Seek(offset, origin);
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00003FE0 File Offset: 0x000021E0
		public override void SetLength(long value)
		{
			this._s.SetLength(value);
		}

		// Token: 0x04000034 RID: 52
		private Stream _s;

		// Token: 0x04000035 RID: 53
		private long _bytesWritten;

		// Token: 0x04000036 RID: 54
		private long _bytesRead;
	}
}
