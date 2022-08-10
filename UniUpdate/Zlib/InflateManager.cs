using System;

namespace Ionic.Zlib
{
	// Token: 0x02000026 RID: 38
	internal sealed class InflateManager
	{
		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060001E8 RID: 488 RVA: 0x000136F8 File Offset: 0x000118F8
		// (set) Token: 0x060001E9 RID: 489 RVA: 0x00013718 File Offset: 0x00011918
		internal bool HandleRfc1950HeaderBytes
		{
			get
			{
				return this._handleRfc1950HeaderBytes;
			}
			set
			{
				this._handleRfc1950HeaderBytes = value;
			}
		}

		// Token: 0x060001EA RID: 490 RVA: 0x00013724 File Offset: 0x00011924
		public InflateManager()
		{
		}

		// Token: 0x060001EB RID: 491 RVA: 0x00013744 File Offset: 0x00011944
		public InflateManager(bool expectRfc1950HeaderBytes)
		{
			this._handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0001376C File Offset: 0x0001196C
		internal int Reset(ZlibCodec z)
		{
			if (z == null)
			{
				throw new ZlibException("Codec is null.");
			}
			if (z.istate == null)
			{
				throw new ZlibException("InflateManager is null.");
			}
			z.TotalBytesIn = (z.TotalBytesOut = 0L);
			z.Message = null;
			z.istate.mode = (z.istate.HandleRfc1950HeaderBytes ? 0 : 7);
			z.istate.blocks.Reset(z, null);
			return 0;
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0001380C File Offset: 0x00011A0C
		internal int End(ZlibCodec z)
		{
			if (this.blocks != null)
			{
				this.blocks.Free(z);
			}
			this.blocks = null;
			return 0;
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0001384C File Offset: 0x00011A4C
		internal int Initialize(ZlibCodec z, int w)
		{
			z.Message = null;
			this.blocks = null;
			if (w < 8 || w > 15)
			{
				this.End(z);
				throw new ZlibException("Bad window size.");
			}
			this.wbits = w;
			z.istate.blocks = new InflateBlocks(z, z.istate.HandleRfc1950HeaderBytes ? this : null, 1 << w);
			this.Reset(z);
			return 0;
		}

		// Token: 0x060001EF RID: 495 RVA: 0x000138E4 File Offset: 0x00011AE4
		internal int Inflate(ZlibCodec z, int f)
		{
			if (z == null)
			{
				throw new ZlibException("Codec is null. ");
			}
			if (z.istate == null)
			{
				throw new ZlibException("InflateManager is null. ");
			}
			if (z.InputBuffer == null)
			{
				throw new ZlibException("InputBuffer is null. ");
			}
			f = ((f == 4) ? -5 : 0);
			int num = -5;
			for (;;)
			{
				switch (z.istate.mode)
				{
				case 0:
					if (z.AvailableBytesIn == 0)
					{
						goto Block_6;
					}
					num = f;
					z.AvailableBytesIn--;
					z.TotalBytesIn += 1L;
					if (((z.istate.method = (int)z.InputBuffer[z.NextIn++]) & 15) != 8)
					{
						z.istate.mode = 13;
						z.Message = string.Format("unknown compression method (0x{0:X2})", z.istate.method);
						z.istate.marker = 5;
						continue;
					}
					if ((z.istate.method >> 4) + 8 > z.istate.wbits)
					{
						z.istate.mode = 13;
						z.Message = string.Format("invalid window size ({0})", (z.istate.method >> 4) + 8);
						z.istate.marker = 5;
						continue;
					}
					z.istate.mode = 1;
					goto IL_1E8;
				case 1:
					goto IL_1E8;
				case 2:
					goto IL_2BF;
				case 3:
					goto IL_33E;
				case 4:
					goto IL_3C5;
				case 5:
					goto IL_44B;
				case 6:
					goto IL_4DC;
				case 7:
					num = z.istate.blocks.Process(z, num);
					if (num == -3)
					{
						z.istate.mode = 13;
						z.istate.marker = 0;
						continue;
					}
					if (num == 0)
					{
						num = f;
					}
					if (num != 1)
					{
						goto Block_18;
					}
					num = f;
					z.istate.blocks.Reset(z, z.istate.was);
					if (!z.istate.HandleRfc1950HeaderBytes)
					{
						z.istate.mode = 12;
						continue;
					}
					z.istate.mode = 8;
					goto IL_5C0;
				case 8:
					goto IL_5C0;
				case 9:
					goto IL_640;
				case 10:
					goto IL_6C8;
				case 11:
					goto IL_74F;
				case 12:
					goto IL_81B;
				case 13:
					goto IL_822;
				}
				break;
				continue;
				IL_1E8:
				if (z.AvailableBytesIn == 0)
				{
					goto Block_9;
				}
				num = f;
				z.AvailableBytesIn--;
				z.TotalBytesIn += 1L;
				int num2 = (int)(z.InputBuffer[z.NextIn++] & byte.MaxValue);
				if (((z.istate.method << 8) + num2) % 31 != 0)
				{
					z.istate.mode = 13;
					z.Message = "incorrect header check";
					z.istate.marker = 5;
					continue;
				}
				if ((num2 & 32) == 0)
				{
					z.istate.mode = 7;
					continue;
				}
				goto IL_2AE;
				IL_74F:
				if (z.AvailableBytesIn == 0)
				{
					goto Block_23;
				}
				num = f;
				z.AvailableBytesIn--;
				z.TotalBytesIn += 1L;
				z.istate.need += (long)((ulong)z.InputBuffer[z.NextIn++] & 255UL);
				if ((int)z.istate.was[0] != (int)z.istate.need)
				{
					z.istate.mode = 13;
					z.Message = "incorrect data check";
					z.istate.marker = 5;
					continue;
				}
				goto IL_809;
				IL_6C8:
				if (z.AvailableBytesIn == 0)
				{
					goto Block_22;
				}
				num = f;
				z.AvailableBytesIn--;
				z.TotalBytesIn += 1L;
				z.istate.need += ((long)((long)(z.InputBuffer[z.NextIn++] & byte.MaxValue) << 8) & 65280L);
				z.istate.mode = 11;
				goto IL_74F;
				IL_640:
				if (z.AvailableBytesIn == 0)
				{
					goto Block_21;
				}
				num = f;
				z.AvailableBytesIn--;
				z.TotalBytesIn += 1L;
				z.istate.need += ((long)((long)(z.InputBuffer[z.NextIn++] & byte.MaxValue) << 16) & 16711680L);
				z.istate.mode = 10;
				goto IL_6C8;
				IL_5C0:
				if (z.AvailableBytesIn == 0)
				{
					goto Block_20;
				}
				num = f;
				z.AvailableBytesIn--;
				z.TotalBytesIn += 1L;
				z.istate.need = (long)((int)(z.InputBuffer[z.NextIn++] & byte.MaxValue) << 24 & -16777216);
				z.istate.mode = 9;
				goto IL_640;
			}
			throw new ZlibException("Stream error.");
			Block_6:
			return num;
			Block_9:
			return num;
			IL_2AE:
			z.istate.mode = 2;
			IL_2BF:
			if (z.AvailableBytesIn == 0)
			{
				return num;
			}
			num = f;
			z.AvailableBytesIn--;
			z.TotalBytesIn += 1L;
			z.istate.need = (long)((int)(z.InputBuffer[z.NextIn++] & byte.MaxValue) << 24 & -16777216);
			z.istate.mode = 3;
			IL_33E:
			if (z.AvailableBytesIn == 0)
			{
				return num;
			}
			num = f;
			z.AvailableBytesIn--;
			z.TotalBytesIn += 1L;
			z.istate.need += ((long)((long)(z.InputBuffer[z.NextIn++] & byte.MaxValue) << 16) & 16711680L);
			z.istate.mode = 4;
			IL_3C5:
			if (z.AvailableBytesIn == 0)
			{
				return num;
			}
			num = f;
			z.AvailableBytesIn--;
			z.TotalBytesIn += 1L;
			z.istate.need += ((long)((long)(z.InputBuffer[z.NextIn++] & byte.MaxValue) << 8) & 65280L);
			z.istate.mode = 5;
			IL_44B:
			if (z.AvailableBytesIn == 0)
			{
				return num;
			}
			z.AvailableBytesIn--;
			z.TotalBytesIn += 1L;
			z.istate.need += (long)((ulong)z.InputBuffer[z.NextIn++] & 255UL);
			z._Adler32 = z.istate.need;
			z.istate.mode = 6;
			return 2;
			IL_4DC:
			z.istate.mode = 13;
			z.Message = "need dictionary";
			z.istate.marker = 0;
			return -2;
			Block_18:
			return num;
			Block_20:
			return num;
			Block_21:
			return num;
			Block_22:
			return num;
			Block_23:
			return num;
			IL_809:
			z.istate.mode = 12;
			IL_81B:
			return 1;
			IL_822:
			throw new ZlibException(string.Format("Bad state ({0})", z.Message));
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x00014144 File Offset: 0x00012344
		internal int SetDictionary(ZlibCodec z, byte[] dictionary)
		{
			int start = 0;
			int num = dictionary.Length;
			if (z == null || z.istate == null || z.istate.mode != 6)
			{
				throw new ZlibException("Stream error.");
			}
			int result;
			if (Adler.Adler32(1L, dictionary, 0, dictionary.Length) != z._Adler32)
			{
				result = -3;
			}
			else
			{
				z._Adler32 = Adler.Adler32(0L, null, 0, 0);
				if (num >= 1 << z.istate.wbits)
				{
					num = (1 << z.istate.wbits) - 1;
					start = dictionary.Length - num;
				}
				z.istate.blocks.SetDictionary(dictionary, start, num);
				z.istate.mode = 7;
				result = 0;
			}
			return result;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x00014224 File Offset: 0x00012424
		internal int Sync(ZlibCodec z)
		{
			int result;
			if (z == null || z.istate == null)
			{
				result = -2;
			}
			else
			{
				if (z.istate.mode != 13)
				{
					z.istate.mode = 13;
					z.istate.marker = 0;
				}
				int num;
				if ((num = z.AvailableBytesIn) == 0)
				{
					result = -5;
				}
				else
				{
					int num2 = z.NextIn;
					int num3 = z.istate.marker;
					while (num != 0 && num3 < 4)
					{
						if (z.InputBuffer[num2] == InflateManager.mark[num3])
						{
							num3++;
						}
						else if (z.InputBuffer[num2] != 0)
						{
							num3 = 0;
						}
						else
						{
							num3 = 4 - num3;
						}
						num2++;
						num--;
					}
					z.TotalBytesIn += (long)(num2 - z.NextIn);
					z.NextIn = num2;
					z.AvailableBytesIn = num;
					z.istate.marker = num3;
					if (num3 != 4)
					{
						result = -3;
					}
					else
					{
						long totalBytesIn = z.TotalBytesIn;
						long totalBytesOut = z.TotalBytesOut;
						this.Reset(z);
						z.TotalBytesIn = totalBytesIn;
						z.TotalBytesOut = totalBytesOut;
						z.istate.mode = 7;
						result = 0;
					}
				}
			}
			return result;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x000143B4 File Offset: 0x000125B4
		internal int SyncPoint(ZlibCodec z)
		{
			int result;
			if (z == null || z.istate == null || z.istate.blocks == null)
			{
				result = -2;
			}
			else
			{
				result = z.istate.blocks.SyncPoint();
			}
			return result;
		}

		// Token: 0x04000179 RID: 377
		private const int PRESET_DICT = 32;

		// Token: 0x0400017A RID: 378
		private const int Z_DEFLATED = 8;

		// Token: 0x0400017B RID: 379
		private const int METHOD = 0;

		// Token: 0x0400017C RID: 380
		private const int FLAG = 1;

		// Token: 0x0400017D RID: 381
		private const int DICT4 = 2;

		// Token: 0x0400017E RID: 382
		private const int DICT3 = 3;

		// Token: 0x0400017F RID: 383
		private const int DICT2 = 4;

		// Token: 0x04000180 RID: 384
		private const int DICT1 = 5;

		// Token: 0x04000181 RID: 385
		private const int DICT0 = 6;

		// Token: 0x04000182 RID: 386
		private const int BLOCKS = 7;

		// Token: 0x04000183 RID: 387
		private const int CHECK4 = 8;

		// Token: 0x04000184 RID: 388
		private const int CHECK3 = 9;

		// Token: 0x04000185 RID: 389
		private const int CHECK2 = 10;

		// Token: 0x04000186 RID: 390
		private const int CHECK1 = 11;

		// Token: 0x04000187 RID: 391
		private const int DONE = 12;

		// Token: 0x04000188 RID: 392
		private const int BAD = 13;

		// Token: 0x04000189 RID: 393
		internal int mode;

		// Token: 0x0400018A RID: 394
		internal int method;

		// Token: 0x0400018B RID: 395
		internal long[] was = new long[1];

		// Token: 0x0400018C RID: 396
		internal long need;

		// Token: 0x0400018D RID: 397
		internal int marker;

		// Token: 0x0400018E RID: 398
		private bool _handleRfc1950HeaderBytes = true;

		// Token: 0x0400018F RID: 399
		internal int wbits;

		// Token: 0x04000190 RID: 400
		internal InflateBlocks blocks;

		// Token: 0x04000191 RID: 401
		private static byte[] mark = new byte[]
		{
			0,
			0,
			byte.MaxValue,
			byte.MaxValue
		};
	}
}
