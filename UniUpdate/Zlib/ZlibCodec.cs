using System;

namespace Ionic.Zlib
{
	// Token: 0x02000033 RID: 51
	public sealed class ZlibCodec
	{
		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600024F RID: 591 RVA: 0x00016740 File Offset: 0x00014940
		public long Adler32
		{
			get
			{
				return this._Adler32;
			}
		}

		// Token: 0x06000250 RID: 592 RVA: 0x00016760 File Offset: 0x00014960
		public ZlibCodec()
		{
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0001676C File Offset: 0x0001496C
		public ZlibCodec(CompressionMode mode)
		{
			if (mode == CompressionMode.Compress)
			{
				int num = this.InitializeDeflate();
				if (num != 0)
				{
					throw new ZlibException("Cannot initialize for deflate.");
				}
			}
			else
			{
				if (mode != CompressionMode.Decompress)
				{
					throw new ZlibException("Invalid ZlibStreamFlavor.");
				}
				int num = this.InitializeInflate();
				if (num != 0)
				{
					throw new ZlibException("Cannot initialize for inflate.");
				}
			}
		}

		// Token: 0x06000252 RID: 594 RVA: 0x000167F8 File Offset: 0x000149F8
		public int InitializeInflate()
		{
			return this.InitializeInflate(15);
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0001681C File Offset: 0x00014A1C
		public int InitializeInflate(bool expectRfc1950Header)
		{
			return this.InitializeInflate(15, expectRfc1950Header);
		}

		// Token: 0x06000254 RID: 596 RVA: 0x00016840 File Offset: 0x00014A40
		public int InitializeInflate(int windowBits)
		{
			return this.InitializeInflate(windowBits, true);
		}

		// Token: 0x06000255 RID: 597 RVA: 0x00016864 File Offset: 0x00014A64
		public int InitializeInflate(int windowBits, bool expectRfc1950Header)
		{
			if (this.dstate != null)
			{
				throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
			}
			this.istate = new InflateManager(expectRfc1950Header);
			return this.istate.Initialize(this, windowBits);
		}

		// Token: 0x06000256 RID: 598 RVA: 0x000168B4 File Offset: 0x00014AB4
		public int Inflate(int f)
		{
			if (this.istate == null)
			{
				throw new ZlibException("No Inflate State!");
			}
			return this.istate.Inflate(this, f);
		}

		// Token: 0x06000257 RID: 599 RVA: 0x000168F8 File Offset: 0x00014AF8
		public int EndInflate()
		{
			if (this.istate == null)
			{
				throw new ZlibException("No Inflate State!");
			}
			int result = this.istate.End(this);
			this.istate = null;
			return result;
		}

		// Token: 0x06000258 RID: 600 RVA: 0x00016944 File Offset: 0x00014B44
		public int SyncInflate()
		{
			if (this.istate == null)
			{
				throw new ZlibException("No Inflate State!");
			}
			return this.istate.Sync(this);
		}

		// Token: 0x06000259 RID: 601 RVA: 0x00016988 File Offset: 0x00014B88
		public int InitializeDeflate()
		{
			return this.InitializeDeflate(CompressionLevel.DEFAULT, 15);
		}

		// Token: 0x0600025A RID: 602 RVA: 0x000169AC File Offset: 0x00014BAC
		public int InitializeDeflate(CompressionLevel level)
		{
			return this.InitializeDeflate(level, 15);
		}

		// Token: 0x0600025B RID: 603 RVA: 0x000169D0 File Offset: 0x00014BD0
		public int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
		{
			return this.InitializeDeflate(level, 15, wantRfc1950Header);
		}

		// Token: 0x0600025C RID: 604 RVA: 0x000169F4 File Offset: 0x00014BF4
		public int InitializeDeflate(CompressionLevel level, int bits)
		{
			return this.InitializeDeflate(level, bits, true);
		}

		// Token: 0x0600025D RID: 605 RVA: 0x00016A18 File Offset: 0x00014C18
		public int InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header)
		{
			if (this.istate != null)
			{
				throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
			}
			this.dstate = new DeflateManager();
			this.dstate.WantRfc1950HeaderBytes = wantRfc1950Header;
			return this.dstate.Initialize(this, level, bits);
		}

		// Token: 0x0600025E RID: 606 RVA: 0x00016A74 File Offset: 0x00014C74
		public int Deflate(int flush)
		{
			if (this.dstate == null)
			{
				throw new ZlibException("No Deflate State!");
			}
			return this.dstate.Deflate(this, flush);
		}

		// Token: 0x0600025F RID: 607 RVA: 0x00016AB8 File Offset: 0x00014CB8
		public int EndDeflate()
		{
			if (this.dstate == null)
			{
				throw new ZlibException("No Deflate State!");
			}
			int result = this.dstate.End();
			this.dstate = null;
			return result;
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00016B04 File Offset: 0x00014D04
		public int SetDeflateParams(CompressionLevel level, CompressionStrategy strategy)
		{
			if (this.dstate == null)
			{
				throw new ZlibException("No Deflate State!");
			}
			return this.dstate.SetParams(this, level, strategy);
		}

		// Token: 0x06000261 RID: 609 RVA: 0x00016B4C File Offset: 0x00014D4C
		public int SetDictionary(byte[] dictionary)
		{
			int result;
			if (this.istate != null)
			{
				result = this.istate.SetDictionary(this, dictionary);
			}
			else
			{
				if (this.dstate == null)
				{
					throw new ZlibException("No Inflate or Deflate state!");
				}
				result = this.dstate.SetDictionary(this, dictionary);
			}
			return result;
		}

		// Token: 0x06000262 RID: 610 RVA: 0x00016BB0 File Offset: 0x00014DB0
		internal void flush_pending()
		{
			int num = this.dstate.pendingCount;
			if (num > this.AvailableBytesOut)
			{
				num = this.AvailableBytesOut;
			}
			if (num != 0)
			{
				if (this.dstate.pending.Length <= this.dstate.nextPending || this.OutputBuffer.Length <= this.NextOut || this.dstate.pending.Length < this.dstate.nextPending + num || this.OutputBuffer.Length < this.NextOut + num)
				{
					throw new ZlibException(string.Format("Invalid State. (pending.Length={0}, pendingCount={1})", this.dstate.pending.Length, this.dstate.pendingCount));
				}
				Array.Copy(this.dstate.pending, this.dstate.nextPending, this.OutputBuffer, this.NextOut, num);
				this.NextOut += num;
				this.dstate.nextPending += num;
				this.TotalBytesOut += (long)num;
				this.AvailableBytesOut -= num;
				this.dstate.pendingCount -= num;
				if (this.dstate.pendingCount == 0)
				{
					this.dstate.nextPending = 0;
				}
			}
		}

		// Token: 0x06000263 RID: 611 RVA: 0x00016D40 File Offset: 0x00014F40
		internal int read_buf(byte[] buf, int start, int size)
		{
			int num = this.AvailableBytesIn;
			if (num > size)
			{
				num = size;
			}
			int result;
			if (num == 0)
			{
				result = 0;
			}
			else
			{
				this.AvailableBytesIn -= num;
				if (this.dstate.WantRfc1950HeaderBytes)
				{
					this._Adler32 = Adler.Adler32(this._Adler32, this.InputBuffer, this.NextIn, num);
				}
				Array.Copy(this.InputBuffer, this.NextIn, buf, start, num);
				this.NextIn += num;
				this.TotalBytesIn += (long)num;
				result = num;
			}
			return result;
		}

		// Token: 0x040001FA RID: 506
		public byte[] InputBuffer;

		// Token: 0x040001FB RID: 507
		public int NextIn;

		// Token: 0x040001FC RID: 508
		public int AvailableBytesIn;

		// Token: 0x040001FD RID: 509
		public long TotalBytesIn;

		// Token: 0x040001FE RID: 510
		public byte[] OutputBuffer;

		// Token: 0x040001FF RID: 511
		public int NextOut;

		// Token: 0x04000200 RID: 512
		public int AvailableBytesOut;

		// Token: 0x04000201 RID: 513
		public long TotalBytesOut;

		// Token: 0x04000202 RID: 514
		public string Message;

		// Token: 0x04000203 RID: 515
		internal DeflateManager dstate;

		// Token: 0x04000204 RID: 516
		internal InflateManager istate;

		// Token: 0x04000205 RID: 517
		internal long _Adler32;
	}
}
