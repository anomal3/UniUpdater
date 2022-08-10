using System;

namespace Ionic.Zlib
{
	// Token: 0x02000025 RID: 37
	internal sealed class InflateCodes
	{
		// Token: 0x060001E3 RID: 483 RVA: 0x00012260 File Offset: 0x00010460
		internal InflateCodes()
		{
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x00012274 File Offset: 0x00010474
		internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, ZlibCodec z)
		{
			this.mode = 0;
			this.lbits = (byte)bl;
			this.dbits = (byte)bd;
			this.ltree = tl;
			this.ltree_index = tl_index;
			this.dtree = td;
			this.dtree_index = td_index;
			this.tree = null;
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x000122B4 File Offset: 0x000104B4
		internal int Process(InflateBlocks blocks, ZlibCodec z, int r)
		{
			int num = z.NextIn;
			int num2 = z.AvailableBytesIn;
			int num3 = blocks.bitb;
			int i = blocks.bitk;
			int num4 = blocks.write;
			int num5 = (num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4);
			for (;;)
			{
				int num6;
				switch (this.mode)
				{
				case 0:
					if (num5 >= 258 && num2 >= 10)
					{
						blocks.bitb = num3;
						blocks.bitk = i;
						z.AvailableBytesIn = num2;
						z.TotalBytesIn += (long)(num - z.NextIn);
						z.NextIn = num;
						blocks.write = num4;
						r = this.InflateFast((int)this.lbits, (int)this.dbits, this.ltree, this.ltree_index, this.dtree, this.dtree_index, blocks, z);
						num = z.NextIn;
						num2 = z.AvailableBytesIn;
						num3 = blocks.bitb;
						i = blocks.bitk;
						num4 = blocks.write;
						num5 = ((num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4));
						if (r != 0)
						{
							this.mode = ((r == 1) ? 7 : 9);
							continue;
						}
					}
					this.need = (int)this.lbits;
					this.tree = this.ltree;
					this.tree_index = this.ltree_index;
					this.mode = 1;
					goto IL_1C3;
				case 1:
					goto IL_1C3;
				case 2:
					num6 = this.get_Renamed;
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_3E3;
						}
						r = 0;
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					this.len += (num3 & InflateCodes.inflate_mask[num6]);
					num3 >>= num6;
					i -= num6;
					this.need = (int)this.dbits;
					this.tree = this.dtree;
					this.tree_index = this.dtree_index;
					this.mode = 3;
					goto IL_4B2;
				case 3:
					goto IL_4B2;
				case 4:
					num6 = this.get_Renamed;
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_684;
						}
						r = 0;
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					this.dist += (num3 & InflateCodes.inflate_mask[num6]);
					num3 >>= num6;
					i -= num6;
					this.mode = 5;
					goto IL_72F;
				case 5:
					goto IL_72F;
				case 6:
					if (num5 == 0)
					{
						if (num4 == blocks.end && blocks.read != 0)
						{
							num4 = 0;
							num5 = ((num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4));
						}
						if (num5 == 0)
						{
							blocks.write = num4;
							r = blocks.Flush(z, r);
							num4 = blocks.write;
							num5 = ((num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4));
							if (num4 == blocks.end && blocks.read != 0)
							{
								num4 = 0;
								num5 = ((num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4));
							}
							if (num5 == 0)
							{
								goto Block_44;
							}
						}
					}
					r = 0;
					blocks.window[num4++] = (byte)this.lit;
					num5--;
					this.mode = 0;
					continue;
				case 7:
					goto IL_AC0;
				case 8:
					goto IL_B8D;
				case 9:
					goto IL_BD9;
				}
				break;
				continue;
				IL_1C3:
				num6 = this.need;
				while (i < num6)
				{
					if (num2 == 0)
					{
						goto IL_1E6;
					}
					r = 0;
					num2--;
					num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
					i += 8;
				}
				int num7 = (this.tree_index + (num3 & InflateCodes.inflate_mask[num6])) * 3;
				num3 = SharedUtils.URShift(num3, this.tree[num7 + 1]);
				i -= this.tree[num7 + 1];
				int num8 = this.tree[num7];
				if (num8 == 0)
				{
					this.lit = this.tree[num7 + 2];
					this.mode = 6;
					continue;
				}
				if ((num8 & 16) != 0)
				{
					this.get_Renamed = (num8 & 15);
					this.len = this.tree[num7 + 2];
					this.mode = 2;
					continue;
				}
				if ((num8 & 64) == 0)
				{
					this.need = num8;
					this.tree_index = num7 / 3 + this.tree[num7 + 2];
					continue;
				}
				if ((num8 & 32) != 0)
				{
					this.mode = 7;
					continue;
				}
				goto IL_360;
				IL_4B2:
				num6 = this.need;
				while (i < num6)
				{
					if (num2 == 0)
					{
						goto IL_4D5;
					}
					r = 0;
					num2--;
					num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
					i += 8;
				}
				num7 = (this.tree_index + (num3 & InflateCodes.inflate_mask[num6])) * 3;
				num3 >>= this.tree[num7 + 1];
				i -= this.tree[num7 + 1];
				num8 = this.tree[num7];
				if ((num8 & 16) != 0)
				{
					this.get_Renamed = (num8 & 15);
					this.dist = this.tree[num7 + 2];
					this.mode = 4;
					continue;
				}
				if ((num8 & 64) == 0)
				{
					this.need = num8;
					this.tree_index = num7 / 3 + this.tree[num7 + 2];
					continue;
				}
				goto IL_601;
				IL_72F:
				int j;
				for (j = num4 - this.dist; j < 0; j += blocks.end)
				{
				}
				while (this.len != 0)
				{
					if (num5 == 0)
					{
						if (num4 == blocks.end && blocks.read != 0)
						{
							num4 = 0;
							num5 = ((num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4));
						}
						if (num5 == 0)
						{
							blocks.write = num4;
							r = blocks.Flush(z, r);
							num4 = blocks.write;
							num5 = ((num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4));
							if (num4 == blocks.end && blocks.read != 0)
							{
								num4 = 0;
								num5 = ((num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4));
							}
							if (num5 == 0)
							{
								goto Block_32;
							}
						}
					}
					blocks.window[num4++] = blocks.window[j++];
					num5--;
					if (j == blocks.end)
					{
						j = 0;
					}
					this.len--;
				}
				this.mode = 0;
			}
			r = -2;
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_1E6:
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_360:
			this.mode = 9;
			z.Message = "invalid literal/length code";
			r = -3;
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_3E3:
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_4D5:
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_601:
			this.mode = 9;
			z.Message = "invalid distance code";
			r = -3;
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_684:
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			Block_32:
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			Block_44:
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_AC0:
			if (i > 7)
			{
				i -= 8;
				num2++;
				num--;
			}
			blocks.write = num4;
			r = blocks.Flush(z, r);
			num4 = blocks.write;
			int num9 = (num4 < blocks.read) ? (blocks.read - num4 - 1) : (blocks.end - num4);
			if (blocks.read != blocks.write)
			{
				blocks.bitb = num3;
				blocks.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				blocks.write = num4;
				return blocks.Flush(z, r);
			}
			this.mode = 8;
			IL_B8D:
			r = 1;
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
			IL_BD9:
			r = -3;
			blocks.bitb = num3;
			blocks.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			blocks.write = num4;
			return blocks.Flush(z, r);
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00012F44 File Offset: 0x00011144
		internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z)
		{
			int num = z.NextIn;
			int num2 = z.AvailableBytesIn;
			int num3 = s.bitb;
			int i = s.bitk;
			int num4 = s.write;
			int num5 = (num4 < s.read) ? (s.read - num4 - 1) : (s.end - num4);
			int num6 = InflateCodes.inflate_mask[bl];
			int num7 = InflateCodes.inflate_mask[bd];
			int num10;
			int num11;
			for (;;)
			{
				while (i < 20)
				{
					num2--;
					num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
					i += 8;
				}
				int num8 = num3 & num6;
				int num9 = (tl_index + num8) * 3;
				if ((num10 = tl[num9]) == 0)
				{
					num3 >>= tl[num9 + 1];
					i -= tl[num9 + 1];
					s.window[num4++] = (byte)tl[num9 + 2];
					num5--;
				}
				else
				{
					for (;;)
					{
						num3 >>= tl[num9 + 1];
						i -= tl[num9 + 1];
						if ((num10 & 16) != 0)
						{
							goto Block_4;
						}
						if ((num10 & 64) != 0)
						{
							goto IL_5C7;
						}
						num8 += tl[num9 + 2];
						num8 += (num3 & InflateCodes.inflate_mask[num10]);
						num9 = (tl_index + num8) * 3;
						if ((num10 = tl[num9]) == 0)
						{
							goto Block_20;
						}
					}
					IL_6E6:
					goto IL_6E7;
					Block_20:
					num3 >>= tl[num9 + 1];
					i -= tl[num9 + 1];
					s.window[num4++] = (byte)tl[num9 + 2];
					num5--;
					goto IL_6E6;
					Block_4:
					num10 &= 15;
					num11 = tl[num9 + 2] + (num3 & InflateCodes.inflate_mask[num10]);
					num3 >>= num10;
					for (i -= num10; i < 15; i += 8)
					{
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
					}
					num8 = (num3 & num7);
					num9 = (td_index + num8) * 3;
					num10 = td[num9];
					for (;;)
					{
						num3 >>= td[num9 + 1];
						i -= td[num9 + 1];
						if ((num10 & 16) != 0)
						{
							break;
						}
						if ((num10 & 64) != 0)
						{
							goto IL_4A0;
						}
						num8 += td[num9 + 2];
						num8 += (num3 & InflateCodes.inflate_mask[num10]);
						num9 = (td_index + num8) * 3;
						num10 = td[num9];
					}
					num10 &= 15;
					while (i < num10)
					{
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					int num12 = td[num9 + 2] + (num3 & InflateCodes.inflate_mask[num10]);
					num3 >>= num10;
					i -= num10;
					num5 -= num11;
					int num13;
					if (num4 >= num12)
					{
						num13 = num4 - num12;
						if (num4 - num13 > 0 && 2 > num4 - num13)
						{
							s.window[num4++] = s.window[num13++];
							s.window[num4++] = s.window[num13++];
							num11 -= 2;
						}
						else
						{
							Array.Copy(s.window, num13, s.window, num4, 2);
							num4 += 2;
							num13 += 2;
							num11 -= 2;
						}
					}
					else
					{
						num13 = num4 - num12;
						do
						{
							num13 += s.end;
						}
						while (num13 < 0);
						num10 = s.end - num13;
						if (num11 > num10)
						{
							num11 -= num10;
							if (num4 - num13 > 0 && num10 > num4 - num13)
							{
								do
								{
									s.window[num4++] = s.window[num13++];
								}
								while (--num10 != 0);
							}
							else
							{
								Array.Copy(s.window, num13, s.window, num4, num10);
								num4 += num10;
								num13 += num10;
							}
							num13 = 0;
						}
					}
					if (num4 - num13 > 0 && num11 > num4 - num13)
					{
						do
						{
							s.window[num4++] = s.window[num13++];
						}
						while (--num11 != 0);
					}
					else
					{
						Array.Copy(s.window, num13, s.window, num4, num11);
						num4 += num11;
						num13 += num11;
					}
				}
				IL_6E7:
				if (num5 < 258 || num2 < 10)
				{
					goto Block_25;
				}
			}
			IL_4A0:
			z.Message = "invalid distance code";
			num11 = z.AvailableBytesIn - num2;
			num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
			num2 += num11;
			num -= num11;
			i -= num11 << 3;
			s.bitb = num3;
			s.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			s.write = num4;
			return -3;
			IL_5C7:
			if ((num10 & 32) != 0)
			{
				num11 = z.AvailableBytesIn - num2;
				num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
				num2 += num11;
				num -= num11;
				i -= num11 << 3;
				s.bitb = num3;
				s.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				s.write = num4;
				return 1;
			}
			z.Message = "invalid literal/length code";
			num11 = z.AvailableBytesIn - num2;
			num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
			num2 += num11;
			num -= num11;
			i -= num11 << 3;
			s.bitb = num3;
			s.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			s.write = num4;
			return -3;
			Block_25:
			num11 = z.AvailableBytesIn - num2;
			num11 = ((i >> 3 < num11) ? (i >> 3) : num11);
			num2 += num11;
			num -= num11;
			i -= num11 << 3;
			s.bitb = num3;
			s.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			s.write = num4;
			return 0;
		}

		// Token: 0x04000160 RID: 352
		private const int START = 0;

		// Token: 0x04000161 RID: 353
		private const int LEN = 1;

		// Token: 0x04000162 RID: 354
		private const int LENEXT = 2;

		// Token: 0x04000163 RID: 355
		private const int DIST = 3;

		// Token: 0x04000164 RID: 356
		private const int DISTEXT = 4;

		// Token: 0x04000165 RID: 357
		private const int COPY = 5;

		// Token: 0x04000166 RID: 358
		private const int LIT = 6;

		// Token: 0x04000167 RID: 359
		private const int WASH = 7;

		// Token: 0x04000168 RID: 360
		private const int END = 8;

		// Token: 0x04000169 RID: 361
		private const int BADCODE = 9;

		// Token: 0x0400016A RID: 362
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

		// Token: 0x0400016B RID: 363
		internal int mode;

		// Token: 0x0400016C RID: 364
		internal int len;

		// Token: 0x0400016D RID: 365
		internal int[] tree;

		// Token: 0x0400016E RID: 366
		internal int tree_index = 0;

		// Token: 0x0400016F RID: 367
		internal int need;

		// Token: 0x04000170 RID: 368
		internal int lit;

		// Token: 0x04000171 RID: 369
		internal int get_Renamed;

		// Token: 0x04000172 RID: 370
		internal int dist;

		// Token: 0x04000173 RID: 371
		internal byte lbits;

		// Token: 0x04000174 RID: 372
		internal byte dbits;

		// Token: 0x04000175 RID: 373
		internal int[] ltree;

		// Token: 0x04000176 RID: 374
		internal int ltree_index;

		// Token: 0x04000177 RID: 375
		internal int[] dtree;

		// Token: 0x04000178 RID: 376
		internal int dtree_index;
	}
}
