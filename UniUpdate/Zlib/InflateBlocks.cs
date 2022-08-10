using System;

namespace Ionic.Zlib
{
	// Token: 0x02000024 RID: 36
	internal sealed class InflateBlocks
	{
		// Token: 0x060001DB RID: 475 RVA: 0x00010D68 File Offset: 0x0000EF68
		internal InflateBlocks(ZlibCodec z, object checkfn, int w)
		{
			this.hufts = new int[4320];
			this.window = new byte[w];
			this.end = w;
			this.checkfn = checkfn;
			this.mode = 0;
			this.Reset(z, null);
		}

		// Token: 0x060001DC RID: 476 RVA: 0x00010DEC File Offset: 0x0000EFEC
		internal void Reset(ZlibCodec z, long[] c)
		{
			if (c != null)
			{
				c[0] = this.check;
			}
			if (this.mode == 4 || this.mode == 5)
			{
			}
			if (this.mode == 6)
			{
			}
			this.mode = 0;
			this.bitk = 0;
			this.bitb = 0;
			this.read = (this.write = 0);
			if (this.checkfn != null)
			{
				z._Adler32 = (this.check = Adler.Adler32(0L, null, 0, 0));
			}
		}

		// Token: 0x060001DD RID: 477 RVA: 0x00010E9C File Offset: 0x0000F09C
		internal int Process(ZlibCodec z, int r)
		{
			int num = z.NextIn;
			int num2 = z.AvailableBytesIn;
			int num3 = this.bitb;
			int i = this.bitk;
			int num4 = this.write;
			int num5 = (num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4);
			int num6;
			for (;;)
			{
				int[] array;
				int[] array2;
				switch (this.mode)
				{
				case 0:
					while (i < 3)
					{
						if (num2 == 0)
						{
							goto IL_B0;
						}
						r = 0;
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					num6 = (num3 & 7);
					this.last = (num6 & 1);
					switch (SharedUtils.URShift(num6, 1))
					{
					case 0:
						num3 = SharedUtils.URShift(num3, 3);
						i -= 3;
						num6 = (i & 7);
						num3 = SharedUtils.URShift(num3, num6);
						i -= num6;
						this.mode = 1;
						break;
					case 1:
					{
						array = new int[1];
						array2 = new int[1];
						int[][] array3 = new int[1][];
						int[][] array4 = new int[1][];
						InfTree.inflate_trees_fixed(array, array2, array3, array4, z);
						this.codes.Init(array[0], array2[0], array3[0], 0, array4[0], 0, z);
						num3 = SharedUtils.URShift(num3, 3);
						i -= 3;
						this.mode = 6;
						break;
					}
					case 2:
						num3 = SharedUtils.URShift(num3, 3);
						i -= 3;
						this.mode = 3;
						break;
					case 3:
						goto IL_212;
					}
					continue;
				case 1:
					while (i < 32)
					{
						if (num2 == 0)
						{
							goto IL_2A2;
						}
						r = 0;
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					if ((SharedUtils.URShift(~num3, 16) & 65535) != (num3 & 65535))
					{
						goto Block_8;
					}
					this.left = (num3 & 65535);
					i = (num3 = 0);
					this.mode = ((this.left != 0) ? 2 : ((this.last != 0) ? 7 : 0));
					continue;
				case 2:
					if (num2 == 0)
					{
						goto Block_11;
					}
					if (num5 == 0)
					{
						if (num4 == this.end && this.read != 0)
						{
							num4 = 0;
							num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
						}
						if (num5 == 0)
						{
							this.write = num4;
							r = this.Flush(z, r);
							num4 = this.write;
							num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
							if (num4 == this.end && this.read != 0)
							{
								num4 = 0;
								num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
							}
							if (num5 == 0)
							{
								goto Block_21;
							}
						}
					}
					r = 0;
					num6 = this.left;
					if (num6 > num2)
					{
						num6 = num2;
					}
					if (num6 > num5)
					{
						num6 = num5;
					}
					Array.Copy(z.InputBuffer, num, this.window, num4, num6);
					num += num6;
					num2 -= num6;
					num4 += num6;
					num5 -= num6;
					if ((this.left -= num6) != 0)
					{
						continue;
					}
					this.mode = ((this.last != 0) ? 7 : 0);
					continue;
				case 3:
					while (i < 14)
					{
						if (num2 == 0)
						{
							goto IL_662;
						}
						r = 0;
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					num6 = (this.table = (num3 & 16383));
					if ((num6 & 31) > 29 || (num6 >> 5 & 31) > 29)
					{
						goto Block_29;
					}
					num6 = 258 + (num6 & 31) + (num6 >> 5 & 31);
					if (this.blens == null || this.blens.Length < num6)
					{
						this.blens = new int[num6];
					}
					else
					{
						for (int j = 0; j < num6; j++)
						{
							this.blens[j] = 0;
						}
					}
					num3 = SharedUtils.URShift(num3, 14);
					i -= 14;
					this.index = 0;
					this.mode = 4;
					goto IL_814;
				case 4:
					goto IL_814;
				case 5:
					goto IL_9FB;
				case 6:
					goto IL_E9F;
				case 7:
					goto IL_F83;
				case 8:
					goto IL_1032;
				case 9:
					goto IL_107E;
				}
				break;
				continue;
				IL_E9F:
				this.bitb = num3;
				this.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				this.write = num4;
				if ((r = this.codes.Process(this, z, r)) != 1)
				{
					goto Block_53;
				}
				r = 0;
				num = z.NextIn;
				num2 = z.AvailableBytesIn;
				num3 = this.bitb;
				i = this.bitk;
				num4 = this.write;
				num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
				if (this.last == 0)
				{
					this.mode = 0;
					continue;
				}
				goto IL_F77;
				IL_9FB:
				for (;;)
				{
					num6 = this.table;
					if (this.index >= 258 + (num6 & 31) + (num6 >> 5 & 31))
					{
						break;
					}
					num6 = this.bb[0];
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_A5A;
						}
						r = 0;
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					if (this.tb[0] == -1)
					{
					}
					num6 = this.hufts[(this.tb[0] + (num3 & InflateBlocks.inflate_mask[num6])) * 3 + 1];
					int num7 = this.hufts[(this.tb[0] + (num3 & InflateBlocks.inflate_mask[num6])) * 3 + 2];
					if (num7 < 16)
					{
						num3 = SharedUtils.URShift(num3, num6);
						i -= num6;
						this.blens[this.index++] = num7;
					}
					else
					{
						int j = (num7 == 18) ? 7 : (num7 - 14);
						int num8 = (num7 == 18) ? 11 : 3;
						while (i < num6 + j)
						{
							if (num2 == 0)
							{
								goto IL_BB8;
							}
							r = 0;
							num2--;
							num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
							i += 8;
						}
						num3 = SharedUtils.URShift(num3, num6);
						i -= num6;
						num8 += (num3 & InflateBlocks.inflate_mask[j]);
						num3 = SharedUtils.URShift(num3, j);
						i -= j;
						j = this.index;
						num6 = this.table;
						if (j + num8 > 258 + (num6 & 31) + (num6 >> 5 & 31) || (num7 == 16 && j < 1))
						{
							goto Block_48;
						}
						num7 = ((num7 == 16) ? this.blens[j - 1] : 0);
						do
						{
							this.blens[j++] = num7;
						}
						while (--num8 != 0);
						this.index = j;
					}
				}
				this.tb[0] = -1;
				array = new int[]
				{
					9
				};
				array2 = new int[]
				{
					6
				};
				int[] array5 = new int[1];
				int[] array6 = new int[1];
				num6 = this.table;
				num6 = this.inftree.inflate_trees_dynamic(257 + (num6 & 31), 1 + (num6 >> 5 & 31), this.blens, array, array2, array5, array6, this.hufts, z);
				if (num6 != 0)
				{
					goto Block_51;
				}
				this.codes.Init(array[0], array2[0], this.hufts, array5[0], this.hufts, array6[0], z);
				this.mode = 6;
				goto IL_E9F;
				IL_814:
				while (this.index < 4 + SharedUtils.URShift(this.table, 10))
				{
					while (i < 3)
					{
						if (num2 == 0)
						{
							goto IL_837;
						}
						r = 0;
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					this.blens[InflateBlocks.border[this.index++]] = (num3 & 7);
					num3 = SharedUtils.URShift(num3, 3);
					i -= 3;
				}
				while (this.index < 19)
				{
					this.blens[InflateBlocks.border[this.index++]] = 0;
				}
				this.bb[0] = 7;
				num6 = this.inftree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, z);
				if (num6 != 0)
				{
					goto Block_36;
				}
				this.index = 0;
				this.mode = 5;
				goto IL_9FB;
			}
			r = -2;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_B0:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_212:
			num3 = SharedUtils.URShift(num3, 3);
			i -= 3;
			this.mode = 9;
			z.Message = "invalid block type";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_2A2:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_8:
			this.mode = 9;
			z.Message = "invalid stored block lengths";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_11:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_21:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_662:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_29:
			this.mode = 9;
			z.Message = "too many length or distance symbols";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_837:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_36:
			r = num6;
			if (r == -3)
			{
				this.blens = null;
				this.mode = 9;
			}
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_A5A:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_BB8:
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_48:
			this.blens = null;
			this.mode = 9;
			z.Message = "invalid bit length repeat";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_51:
			if (num6 == -3)
			{
				this.blens = null;
				this.mode = 9;
			}
			r = num6;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			Block_53:
			return this.Flush(z, r);
			IL_F77:
			this.mode = 7;
			IL_F83:
			this.write = num4;
			r = this.Flush(z, r);
			num4 = this.write;
			int num9 = (num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4);
			if (this.read != this.write)
			{
				this.bitb = num3;
				this.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				this.write = num4;
				return this.Flush(z, r);
			}
			this.mode = 8;
			IL_1032:
			r = 1;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
			IL_107E:
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			this.write = num4;
			return this.Flush(z, r);
		}

		// Token: 0x060001DE RID: 478 RVA: 0x00011FD0 File Offset: 0x000101D0
		internal void Free(ZlibCodec z)
		{
			this.Reset(z, null);
			this.window = null;
			this.hufts = null;
		}

		// Token: 0x060001DF RID: 479 RVA: 0x00011FEC File Offset: 0x000101EC
		internal void SetDictionary(byte[] d, int start, int n)
		{
			Array.Copy(d, start, this.window, 0, n);
			this.write = n;
			this.read = n;
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x00012020 File Offset: 0x00010220
		internal int SyncPoint()
		{
			return (this.mode == 1) ? 1 : 0;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0001204C File Offset: 0x0001024C
		internal int Flush(ZlibCodec z, int r)
		{
			int num = z.NextOut;
			int num2 = this.read;
			int num3 = ((num2 <= this.write) ? this.write : this.end) - num2;
			if (num3 > z.AvailableBytesOut)
			{
				num3 = z.AvailableBytesOut;
			}
			if (num3 != 0 && r == -5)
			{
				r = 0;
			}
			z.AvailableBytesOut -= num3;
			z.TotalBytesOut += (long)num3;
			if (this.checkfn != null)
			{
				z._Adler32 = (this.check = Adler.Adler32(this.check, this.window, num2, num3));
			}
			Array.Copy(this.window, num2, z.OutputBuffer, num, num3);
			num += num3;
			num2 += num3;
			if (num2 == this.end)
			{
				num2 = 0;
				if (this.write == this.end)
				{
					this.write = 0;
				}
				num3 = this.write - num2;
				if (num3 > z.AvailableBytesOut)
				{
					num3 = z.AvailableBytesOut;
				}
				if (num3 != 0 && r == -5)
				{
					r = 0;
				}
				z.AvailableBytesOut -= num3;
				z.TotalBytesOut += (long)num3;
				if (this.checkfn != null)
				{
					z._Adler32 = (this.check = Adler.Adler32(this.check, this.window, num2, num3));
				}
				Array.Copy(this.window, num2, z.OutputBuffer, num, num3);
				num += num3;
				num2 += num3;
			}
			z.NextOut = num;
			this.read = num2;
			return r;
		}

		// Token: 0x04000140 RID: 320
		private const int MANY = 1440;

		// Token: 0x04000141 RID: 321
		private const int TYPE = 0;

		// Token: 0x04000142 RID: 322
		private const int LENS = 1;

		// Token: 0x04000143 RID: 323
		private const int STORED = 2;

		// Token: 0x04000144 RID: 324
		private const int TABLE = 3;

		// Token: 0x04000145 RID: 325
		private const int BTREE = 4;

		// Token: 0x04000146 RID: 326
		private const int DTREE = 5;

		// Token: 0x04000147 RID: 327
		private const int CODES = 6;

		// Token: 0x04000148 RID: 328
		private const int DRY = 7;

		// Token: 0x04000149 RID: 329
		private const int DONE = 8;

		// Token: 0x0400014A RID: 330
		private const int BAD = 9;

		// Token: 0x0400014B RID: 331
		private static readonly int[] inflate_mask = new int[]
		{
			0,
			1,
			3,
			7,
			15,
			31,
			63,
			127,
			255,
			511,
			1023,
			2047,
			4095,
			8191,
			16383,
			32767,
			65535
		};

		// Token: 0x0400014C RID: 332
		internal static readonly int[] border = new int[]
		{
			16,
			17,
			18,
			0,
			8,
			7,
			9,
			6,
			10,
			5,
			11,
			4,
			12,
			3,
			13,
			2,
			14,
			1,
			15
		};

		// Token: 0x0400014D RID: 333
		internal int mode;

		// Token: 0x0400014E RID: 334
		internal int left;

		// Token: 0x0400014F RID: 335
		internal int table;

		// Token: 0x04000150 RID: 336
		internal int index;

		// Token: 0x04000151 RID: 337
		internal int[] blens;

		// Token: 0x04000152 RID: 338
		internal int[] bb = new int[1];

		// Token: 0x04000153 RID: 339
		internal int[] tb = new int[1];

		// Token: 0x04000154 RID: 340
		internal InflateCodes codes = new InflateCodes();

		// Token: 0x04000155 RID: 341
		internal int last;

		// Token: 0x04000156 RID: 342
		internal int bitk;

		// Token: 0x04000157 RID: 343
		internal int bitb;

		// Token: 0x04000158 RID: 344
		internal int[] hufts;

		// Token: 0x04000159 RID: 345
		internal byte[] window;

		// Token: 0x0400015A RID: 346
		internal int end;

		// Token: 0x0400015B RID: 347
		internal int read;

		// Token: 0x0400015C RID: 348
		internal int write;

		// Token: 0x0400015D RID: 349
		internal object checkfn;

		// Token: 0x0400015E RID: 350
		internal long check;

		// Token: 0x0400015F RID: 351
		internal InfTree inftree = new InfTree();
	}
}
