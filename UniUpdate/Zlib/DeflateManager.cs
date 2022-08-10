using System;

namespace Ionic.Zlib
{
	// Token: 0x02000023 RID: 35
	internal sealed class DeflateManager
	{
		// Token: 0x060001B1 RID: 433 RVA: 0x0000E0E4 File Offset: 0x0000C2E4
		internal DeflateManager()
		{
			this.dyn_ltree = new short[DeflateManager.HEAP_SIZE * 2];
			this.dyn_dtree = new short[122];
			this.bl_tree = new short[78];
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0000E190 File Offset: 0x0000C390
		private void _InitializeLazyMatch()
		{
			this.window_size = 2 * this.w_size;
			this.head[this.hash_size - 1] = 0;
			for (int i = 0; i < this.hash_size - 1; i++)
			{
				this.head[i] = 0;
			}
			this.max_lazy_match = DeflateManager.configTable[(int)this.compressionLevel].max_lazy;
			this.good_match = DeflateManager.configTable[(int)this.compressionLevel].good_length;
			this.nice_match = DeflateManager.configTable[(int)this.compressionLevel].nice_length;
			this.max_chain_length = DeflateManager.configTable[(int)this.compressionLevel].max_chain;
			this.strstart = 0;
			this.block_start = 0;
			this.lookahead = 0;
			this.match_length = (this.prev_length = 2);
			this.match_available = 0;
			this.ins_h = 0;
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x0000E274 File Offset: 0x0000C474
		private void _InitializeTreeData()
		{
			this.l_desc.dyn_tree = this.dyn_ltree;
			this.l_desc.stat_desc = StaticTree.static_l_desc;
			this.d_desc.dyn_tree = this.dyn_dtree;
			this.d_desc.stat_desc = StaticTree.static_d_desc;
			this.bl_desc.dyn_tree = this.bl_tree;
			this.bl_desc.stat_desc = StaticTree.static_bl_desc;
			this.bi_buf = 0;
			this.bi_valid = 0;
			this.last_eob_len = 8;
			this._InitializeBlocks();
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x0000E308 File Offset: 0x0000C508
		internal void _InitializeBlocks()
		{
			for (int i = 0; i < DeflateManager.L_CODES; i++)
			{
				this.dyn_ltree[i * 2] = 0;
			}
			for (int i = 0; i < 30; i++)
			{
				this.dyn_dtree[i * 2] = 0;
			}
			for (int i = 0; i < 19; i++)
			{
				this.bl_tree[i * 2] = 0;
			}
			this.dyn_ltree[512] = 1;
			this.opt_len = (this.static_len = 0);
			this.last_lit = (this.matches = 0);
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000E3A8 File Offset: 0x0000C5A8
		internal void pqdownheap(short[] tree, int k)
		{
			int num = this.heap[k];
			for (int i = k << 1; i <= this.heap_len; i <<= 1)
			{
				if (i < this.heap_len && DeflateManager._IsSmaller(tree, this.heap[i + 1], this.heap[i], this.depth))
				{
					i++;
				}
				if (DeflateManager._IsSmaller(tree, num, this.heap[i], this.depth))
				{
					break;
				}
				this.heap[k] = this.heap[i];
				k = i;
			}
			this.heap[k] = num;
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0000E468 File Offset: 0x0000C668
		internal static bool _IsSmaller(short[] tree, int n, int m, sbyte[] depth)
		{
			short num = tree[n * 2];
			short num2 = tree[m * 2];
			return num < num2 || (num == num2 && depth[n] <= depth[m]);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x0000E4B4 File Offset: 0x0000C6B4
		internal void scan_tree(short[] tree, int max_code)
		{
			int num = -1;
			int num2 = (int)tree[1];
			int num3 = 0;
			int num4 = 7;
			int num5 = 4;
			if (num2 == 0)
			{
				num4 = 138;
				num5 = 3;
			}
			tree[(max_code + 1) * 2 + 1] = short.MaxValue;
			for (int i = 0; i <= max_code; i++)
			{
				int num6 = num2;
				num2 = (int)tree[(i + 1) * 2 + 1];
				if (++num3 >= num4 || num6 != num2)
				{
					if (num3 < num5)
					{
						this.bl_tree[num6 * 2] = (short)((int)this.bl_tree[num6 * 2] + num3);
					}
					else if (num6 != 0)
					{
						if (num6 != num)
						{
							short[] array = this.bl_tree;
							int num7 = num6 * 2;
							array[num7] += 1;
						}
						short[] array2 = this.bl_tree;
						int num8 = 32;
						array2[num8] += 1;
					}
					else if (num3 <= 10)
					{
						short[] array3 = this.bl_tree;
						int num9 = 34;
						array3[num9] += 1;
					}
					else
					{
						short[] array4 = this.bl_tree;
						int num10 = 36;
						array4[num10] += 1;
					}
					num3 = 0;
					num = num6;
					if (num2 == 0)
					{
						num4 = 138;
						num5 = 3;
					}
					else if (num6 == num2)
					{
						num4 = 6;
						num5 = 3;
					}
					else
					{
						num4 = 7;
						num5 = 4;
					}
				}
			}
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x0000E658 File Offset: 0x0000C858
		internal int build_bl_tree()
		{
			this.scan_tree(this.dyn_ltree, this.l_desc.max_code);
			this.scan_tree(this.dyn_dtree, this.d_desc.max_code);
			this.bl_desc.build_tree(this);
			int i;
			for (i = 18; i >= 3; i--)
			{
				if (this.bl_tree[(int)(Tree.bl_order[i] * 2 + 1)] != 0)
				{
					break;
				}
			}
			this.opt_len += 3 * (i + 1) + 5 + 5 + 4;
			return i;
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0000E704 File Offset: 0x0000C904
		internal void send_all_trees(int lcodes, int dcodes, int blcodes)
		{
			this.send_bits(lcodes - 257, 5);
			this.send_bits(dcodes - 1, 5);
			this.send_bits(blcodes - 4, 4);
			for (int i = 0; i < blcodes; i++)
			{
				this.send_bits((int)this.bl_tree[(int)(Tree.bl_order[i] * 2 + 1)], 3);
			}
			this.send_tree(this.dyn_ltree, lcodes - 1);
			this.send_tree(this.dyn_dtree, dcodes - 1);
		}

		// Token: 0x060001BA RID: 442 RVA: 0x0000E78C File Offset: 0x0000C98C
		internal void send_tree(short[] tree, int max_code)
		{
			int num = -1;
			int num2 = (int)tree[1];
			int num3 = 0;
			int num4 = 7;
			int num5 = 4;
			if (num2 == 0)
			{
				num4 = 138;
				num5 = 3;
			}
			for (int i = 0; i <= max_code; i++)
			{
				int num6 = num2;
				num2 = (int)tree[(i + 1) * 2 + 1];
				if (++num3 >= num4 || num6 != num2)
				{
					if (num3 < num5)
					{
						do
						{
							this.send_code(num6, this.bl_tree);
						}
						while (--num3 != 0);
					}
					else if (num6 != 0)
					{
						if (num6 != num)
						{
							this.send_code(num6, this.bl_tree);
							num3--;
						}
						this.send_code(16, this.bl_tree);
						this.send_bits(num3 - 3, 2);
					}
					else if (num3 <= 10)
					{
						this.send_code(17, this.bl_tree);
						this.send_bits(num3 - 3, 3);
					}
					else
					{
						this.send_code(18, this.bl_tree);
						this.send_bits(num3 - 11, 7);
					}
					num3 = 0;
					num = num6;
					if (num2 == 0)
					{
						num4 = 138;
						num5 = 3;
					}
					else if (num6 == num2)
					{
						num4 = 6;
						num5 = 3;
					}
					else
					{
						num4 = 7;
						num5 = 4;
					}
				}
			}
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0000E940 File Offset: 0x0000CB40
		private void put_byte(byte[] p, int start, int len)
		{
			Array.Copy(p, start, this.pending, this.pendingCount, len);
			this.pendingCount += len;
		}

		// Token: 0x060001BC RID: 444 RVA: 0x0000E968 File Offset: 0x0000CB68
		private void put_byte(byte c)
		{
			this.pending[this.pendingCount++] = c;
		}

		// Token: 0x060001BD RID: 445 RVA: 0x0000E994 File Offset: 0x0000CB94
		internal void put_short(int w)
		{
			this.put_byte((byte)w);
			this.put_byte((byte)SharedUtils.URShift(w, 8));
		}

		// Token: 0x060001BE RID: 446 RVA: 0x0000E9B0 File Offset: 0x0000CBB0
		internal void putShortMSB(int b)
		{
			this.put_byte((byte)(b >> 8));
			this.put_byte((byte)b);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x0000E9C8 File Offset: 0x0000CBC8
		internal void send_code(int c, short[] tree)
		{
			int num = c * 2;
			this.send_bits((int)tree[num] & 65535, (int)tree[num + 1] & 65535);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x0000E9FC File Offset: 0x0000CBFC
		internal void send_bits(int value_Renamed, int length)
		{
			if (this.bi_valid > 16 - length)
			{
				this.bi_buf |= (short)(value_Renamed << this.bi_valid & 65535);
				this.put_short((int)this.bi_buf);
				this.bi_buf = (short)SharedUtils.URShift(value_Renamed, 16 - this.bi_valid);
				this.bi_valid += length - 16;
			}
			else
			{
				this.bi_buf |= (short)(value_Renamed << this.bi_valid & 65535);
				this.bi_valid += length;
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000EAB4 File Offset: 0x0000CCB4
		internal void _tr_align()
		{
			this.send_bits(2, 3);
			this.send_code(256, StaticTree.static_ltree);
			this.bi_flush();
			if (1 + this.last_eob_len + 10 - this.bi_valid < 9)
			{
				this.send_bits(2, 3);
				this.send_code(256, StaticTree.static_ltree);
				this.bi_flush();
			}
			this.last_eob_len = 7;
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000EB34 File Offset: 0x0000CD34
		internal bool _tr_tally(int dist, int lc)
		{
			this.pending[this.d_buf + this.last_lit * 2] = (byte)SharedUtils.URShift(dist, 8);
			this.pending[this.d_buf + this.last_lit * 2 + 1] = (byte)dist;
			this.pending[this.l_buf + this.last_lit] = (byte)lc;
			this.last_lit++;
			if (dist == 0)
			{
				short[] array = this.dyn_ltree;
				int num = lc * 2;
				array[num] += 1;
			}
			else
			{
				this.matches++;
				dist--;
				short[] array2 = this.dyn_ltree;
				int num2 = ((int)Tree._length_code[lc] + 256 + 1) * 2;
				array2[num2] += 1;
				short[] array3 = this.dyn_dtree;
				int num3 = Tree.d_code(dist) * 2;
				array3[num3] += 1;
			}
			if ((this.last_lit & 8191) == 0 && this.compressionLevel > CompressionLevel.LEVEL2)
			{
				int num4 = this.last_lit * 8;
				int num5 = this.strstart - this.block_start;
				for (int i = 0; i < 30; i++)
				{
					num4 = (int)((long)num4 + (long)this.dyn_dtree[i * 2] * (5L + (long)Tree.extra_dbits[i]));
				}
				num4 = SharedUtils.URShift(num4, 3);
				if (this.matches < this.last_lit / 2 && num4 < num5 / 2)
				{
					return true;
				}
			}
			return this.last_lit == this.lit_bufsize - 1;
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x0000ECE4 File Offset: 0x0000CEE4
		internal void compress_block(short[] ltree, short[] dtree)
		{
			int num = 0;
			if (this.last_lit != 0)
			{
				do
				{
					int num2 = ((int)this.pending[this.d_buf + num * 2] << 8 & 65280) | (int)(this.pending[this.d_buf + num * 2 + 1] & byte.MaxValue);
					int num3 = (int)(this.pending[this.l_buf + num] & byte.MaxValue);
					num++;
					if (num2 == 0)
					{
						this.send_code(num3, ltree);
					}
					else
					{
						int num4 = (int)Tree._length_code[num3];
						this.send_code(num4 + 256 + 1, ltree);
						int num5 = Tree.extra_lbits[num4];
						if (num5 != 0)
						{
							num3 -= Tree.base_length[num4];
							this.send_bits(num3, num5);
						}
						num2--;
						num4 = Tree.d_code(num2);
						this.send_code(num4, dtree);
						num5 = Tree.extra_dbits[num4];
						if (num5 != 0)
						{
							num2 -= Tree.base_dist[num4];
							this.send_bits(num2, num5);
						}
					}
				}
				while (num < this.last_lit);
			}
			this.send_code(256, ltree);
			this.last_eob_len = (int)ltree[513];
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x0000EE30 File Offset: 0x0000D030
		internal void set_data_type()
		{
			int i = 0;
			int num = 0;
			int num2 = 0;
			while (i < 7)
			{
				num2 += (int)this.dyn_ltree[i * 2];
				i++;
			}
			while (i < 128)
			{
				num += (int)this.dyn_ltree[i * 2];
				i++;
			}
			while (i < 256)
			{
				num2 += (int)this.dyn_ltree[i * 2];
				i++;
			}
			this.data_type = (sbyte)((num2 > SharedUtils.URShift(num, 2)) ? 0 : 1);
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000EECC File Offset: 0x0000D0CC
		internal void bi_flush()
		{
			if (this.bi_valid == 16)
			{
				this.put_short((int)this.bi_buf);
				this.bi_buf = 0;
				this.bi_valid = 0;
			}
			else if (this.bi_valid >= 8)
			{
				this.put_byte((byte)this.bi_buf);
				this.bi_buf = (short)SharedUtils.URShift((int)this.bi_buf, 8);
				this.bi_valid -= 8;
			}
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000EF58 File Offset: 0x0000D158
		internal void bi_windup()
		{
			if (this.bi_valid > 8)
			{
				this.put_short((int)this.bi_buf);
			}
			else if (this.bi_valid > 0)
			{
				this.put_byte((byte)this.bi_buf);
			}
			this.bi_buf = 0;
			this.bi_valid = 0;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x0000EFC4 File Offset: 0x0000D1C4
		internal void copy_block(int buf, int len, bool header)
		{
			this.bi_windup();
			this.last_eob_len = 8;
			if (header)
			{
				this.put_short((int)((short)len));
				this.put_short((int)((short)(~(short)len)));
			}
			this.put_byte(this.window, buf, len);
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0000F014 File Offset: 0x0000D214
		internal void flush_block_only(bool eof)
		{
			this._tr_flush_block((this.block_start >= 0) ? this.block_start : -1, this.strstart - this.block_start, eof);
			this.block_start = this.strstart;
			this.strm.flush_pending();
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x0000F06C File Offset: 0x0000D26C
		internal int DeflateNone(int flush)
		{
			int num = 65535;
			if (num > this.pending.Length - 5)
			{
				num = this.pending.Length - 5;
			}
			for (;;)
			{
				if (this.lookahead <= 1)
				{
					this._fillWindow();
					if (this.lookahead == 0 && flush == 0)
					{
						break;
					}
					if (this.lookahead == 0)
					{
						goto Block_5;
					}
				}
				this.strstart += this.lookahead;
				this.lookahead = 0;
				int num2 = this.block_start + num;
				if (this.strstart == 0 || this.strstart >= num2)
				{
					this.lookahead = this.strstart - num2;
					this.strstart = num2;
					this.flush_block_only(false);
					if (this.strm.AvailableBytesOut == 0)
					{
						goto Block_8;
					}
				}
				if (this.strstart - this.block_start >= this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					this.flush_block_only(false);
					if (this.strm.AvailableBytesOut == 0)
					{
						goto Block_10;
					}
				}
			}
			return 0;
			Block_5:
			this.flush_block_only(flush == 4);
			if (this.strm.AvailableBytesOut == 0)
			{
				return (flush == 4) ? 2 : 0;
			}
			return (flush == 4) ? 3 : 1;
			Block_8:
			return 0;
			Block_10:
			return 0;
		}

		// Token: 0x060001CA RID: 458 RVA: 0x0000F228 File Offset: 0x0000D428
		internal void _tr_stored_block(int buf, int stored_len, bool eof)
		{
			this.send_bits(eof ? 1 : 0, 3);
			this.copy_block(buf, stored_len, true);
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000F24C File Offset: 0x0000D44C
		internal void _tr_flush_block(int buf, int stored_len, bool eof)
		{
			int num = 0;
			int num2;
			int num3;
			if (this.compressionLevel > CompressionLevel.NONE)
			{
				if (this.data_type == 2)
				{
					this.set_data_type();
				}
				this.l_desc.build_tree(this);
				this.d_desc.build_tree(this);
				num = this.build_bl_tree();
				num2 = SharedUtils.URShift(this.opt_len + 3 + 7, 3);
				num3 = SharedUtils.URShift(this.static_len + 3 + 7, 3);
				if (num3 <= num2)
				{
					num2 = num3;
				}
			}
			else
			{
				num3 = (num2 = stored_len + 5);
			}
			if (stored_len + 4 <= num2 && buf != -1)
			{
				this._tr_stored_block(buf, stored_len, eof);
			}
			else if (num3 == num2)
			{
				this.send_bits(2 + (eof ? 1 : 0), 3);
				this.compress_block(StaticTree.static_ltree, StaticTree.static_dtree);
			}
			else
			{
				this.send_bits(4 + (eof ? 1 : 0), 3);
				this.send_all_trees(this.l_desc.max_code + 1, this.d_desc.max_code + 1, num + 1);
				this.compress_block(this.dyn_ltree, this.dyn_dtree);
			}
			this._InitializeBlocks();
			if (eof)
			{
				this.bi_windup();
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x0000F3C0 File Offset: 0x0000D5C0
		private void _fillWindow()
		{
			do
			{
				int num = this.window_size - this.lookahead - this.strstart;
				int num2;
				if (num == 0 && this.strstart == 0 && this.lookahead == 0)
				{
					num = this.w_size;
				}
				else if (num == -1)
				{
					num--;
				}
				else if (this.strstart >= this.w_size + this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					Array.Copy(this.window, this.w_size, this.window, 0, this.w_size);
					this.match_start -= this.w_size;
					this.strstart -= this.w_size;
					this.block_start -= this.w_size;
					num2 = this.hash_size;
					int num3 = num2;
					do
					{
						int num4 = (int)this.head[--num3] & 65535;
						this.head[num3] = (short)((num4 >= this.w_size) ? (num4 - this.w_size) : 0);
					}
					while (--num2 != 0);
					num2 = this.w_size;
					num3 = num2;
					do
					{
						int num4 = (int)this.prev[--num3] & 65535;
						this.prev[num3] = (short)((num4 >= this.w_size) ? (num4 - this.w_size) : 0);
					}
					while (--num2 != 0);
					num += this.w_size;
				}
				if (this.strm.AvailableBytesIn == 0)
				{
					break;
				}
				num2 = this.strm.read_buf(this.window, this.strstart + this.lookahead, num);
				this.lookahead += num2;
				if (this.lookahead >= 3)
				{
					this.ins_h = (int)(this.window[this.strstart] & byte.MaxValue);
					this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 1] & byte.MaxValue)) & this.hash_mask);
				}
			}
			while (this.lookahead < DeflateManager.MIN_LOOKAHEAD && this.strm.AvailableBytesIn != 0);
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000F63C File Offset: 0x0000D83C
		internal int DeflateFast(int flush)
		{
			int num = 0;
			for (;;)
			{
				if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
				{
					this._fillWindow();
					if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == 0)
					{
						break;
					}
					if (this.lookahead == 0)
					{
						goto Block_4;
					}
				}
				if (this.lookahead >= 3)
				{
					this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 2] & byte.MaxValue)) & this.hash_mask);
					num = ((int)this.head[this.ins_h] & 65535);
					this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
					this.head[this.ins_h] = (short)this.strstart;
				}
				if ((long)num != 0L && (this.strstart - num & 65535) <= this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					if (this.compressionStrategy != CompressionStrategy.HUFFMAN_ONLY)
					{
						this.match_length = this.longest_match(num);
					}
				}
				bool flag;
				if (this.match_length >= 3)
				{
					flag = this._tr_tally(this.strstart - this.match_start, this.match_length - 3);
					this.lookahead -= this.match_length;
					if (this.match_length <= this.max_lazy_match && this.lookahead >= 3)
					{
						this.match_length--;
						do
						{
							this.strstart++;
							this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 2] & byte.MaxValue)) & this.hash_mask);
							num = ((int)this.head[this.ins_h] & 65535);
							this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
							this.head[this.ins_h] = (short)this.strstart;
						}
						while (--this.match_length != 0);
						this.strstart++;
					}
					else
					{
						this.strstart += this.match_length;
						this.match_length = 0;
						this.ins_h = (int)(this.window[this.strstart] & byte.MaxValue);
						this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 1] & byte.MaxValue)) & this.hash_mask);
					}
				}
				else
				{
					flag = this._tr_tally(0, (int)(this.window[this.strstart] & byte.MaxValue));
					this.lookahead--;
					this.strstart++;
				}
				if (flag)
				{
					this.flush_block_only(false);
					if (this.strm.AvailableBytesOut == 0)
					{
						goto Block_14;
					}
				}
			}
			return 0;
			Block_4:
			this.flush_block_only(flush == 4);
			if (this.strm.AvailableBytesOut != 0)
			{
				return (flush == 4) ? 3 : 1;
			}
			if (flush == 4)
			{
				return 2;
			}
			return 0;
			Block_14:
			return 0;
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000FA0C File Offset: 0x0000DC0C
		internal int DeflateSlow(int flush)
		{
			int num = 0;
			for (;;)
			{
				if (this.lookahead < DeflateManager.MIN_LOOKAHEAD)
				{
					this._fillWindow();
					if (this.lookahead < DeflateManager.MIN_LOOKAHEAD && flush == 0)
					{
						break;
					}
					if (this.lookahead == 0)
					{
						goto Block_4;
					}
				}
				if (this.lookahead >= 3)
				{
					this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 2] & byte.MaxValue)) & this.hash_mask);
					num = ((int)this.head[this.ins_h] & 65535);
					this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
					this.head[this.ins_h] = (short)this.strstart;
				}
				this.prev_length = this.match_length;
				this.prev_match = this.match_start;
				this.match_length = 2;
				if (num != 0 && this.prev_length < this.max_lazy_match && (this.strstart - num & 65535) <= this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					if (this.compressionStrategy != CompressionStrategy.HUFFMAN_ONLY)
					{
						this.match_length = this.longest_match(num);
					}
					if (this.match_length <= 5 && (this.compressionStrategy == CompressionStrategy.FILTERED || (this.match_length == 3 && this.strstart - this.match_start > 4096)))
					{
						this.match_length = 2;
					}
				}
				if (this.prev_length >= 3 && this.match_length <= this.prev_length)
				{
					int num2 = this.strstart + this.lookahead - 3;
					bool flag = this._tr_tally(this.strstart - 1 - this.prev_match, this.prev_length - 3);
					this.lookahead -= this.prev_length - 1;
					this.prev_length -= 2;
					do
					{
						if (++this.strstart <= num2)
						{
							this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[this.strstart + 2] & byte.MaxValue)) & this.hash_mask);
							num = ((int)this.head[this.ins_h] & 65535);
							this.prev[this.strstart & this.w_mask] = this.head[this.ins_h];
							this.head[this.ins_h] = (short)this.strstart;
						}
					}
					while (--this.prev_length != 0);
					this.match_available = 0;
					this.match_length = 2;
					this.strstart++;
					if (flag)
					{
						this.flush_block_only(false);
						if (this.strm.AvailableBytesOut == 0)
						{
							goto Block_19;
						}
					}
				}
				else if (this.match_available != 0)
				{
					bool flag = this._tr_tally(0, (int)(this.window[this.strstart - 1] & byte.MaxValue));
					if (flag)
					{
						this.flush_block_only(false);
					}
					this.strstart++;
					this.lookahead--;
					if (this.strm.AvailableBytesOut == 0)
					{
						goto Block_22;
					}
				}
				else
				{
					this.match_available = 1;
					this.strstart++;
					this.lookahead--;
				}
			}
			return 0;
			Block_4:
			if (this.match_available != 0)
			{
				bool flag = this._tr_tally(0, (int)(this.window[this.strstart - 1] & byte.MaxValue));
				this.match_available = 0;
			}
			this.flush_block_only(flush == 4);
			if (this.strm.AvailableBytesOut != 0)
			{
				return (flush == 4) ? 3 : 1;
			}
			if (flush == 4)
			{
				return 2;
			}
			return 0;
			Block_19:
			return 0;
			Block_22:
			return 0;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000FEC4 File Offset: 0x0000E0C4
		internal int longest_match(int cur_match)
		{
			int num = this.max_chain_length;
			int num2 = this.strstart;
			int num3 = this.prev_length;
			int num4 = (this.strstart > this.w_size - DeflateManager.MIN_LOOKAHEAD) ? (this.strstart - (this.w_size - DeflateManager.MIN_LOOKAHEAD)) : 0;
			int num5 = this.nice_match;
			int num6 = this.w_mask;
			int num7 = this.strstart + 258;
			byte b = this.window[num2 + num3 - 1];
			byte b2 = this.window[num2 + num3];
			if (this.prev_length >= this.good_match)
			{
				num >>= 2;
			}
			if (num5 > this.lookahead)
			{
				num5 = this.lookahead;
			}
			do
			{
				int num8 = cur_match;
				if (this.window[num8 + num3] == b2 && this.window[num8 + num3 - 1] == b && this.window[num8] == this.window[num2] && this.window[++num8] == this.window[num2 + 1])
				{
					num2 += 2;
					num8++;
					while (this.window[++num2] == this.window[++num8] && this.window[++num2] == this.window[++num8] && this.window[++num2] == this.window[++num8] && this.window[++num2] == this.window[++num8] && this.window[++num2] == this.window[++num8] && this.window[++num2] == this.window[++num8] && this.window[++num2] == this.window[++num8] && this.window[++num2] == this.window[++num8] && num2 < num7)
					{
					}
					int num9 = 258 - (num7 - num2);
					num2 = num7 - 258;
					if (num9 > num3)
					{
						this.match_start = cur_match;
						num3 = num9;
						if (num9 >= num5)
						{
							break;
						}
						b = this.window[num2 + num3 - 1];
						b2 = this.window[num2 + num3];
					}
				}
			}
			while ((cur_match = ((int)this.prev[cur_match & num6] & 65535)) > num4 && --num != 0);
			int result;
			if (num3 <= this.lookahead)
			{
				result = num3;
			}
			else
			{
				result = this.lookahead;
			}
			return result;
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060001D0 RID: 464 RVA: 0x000101D4 File Offset: 0x0000E3D4
		// (set) Token: 0x060001D1 RID: 465 RVA: 0x000101F4 File Offset: 0x0000E3F4
		internal bool WantRfc1950HeaderBytes
		{
			get
			{
				return this._WantRfc1950HeaderBytes;
			}
			set
			{
				this._WantRfc1950HeaderBytes = value;
			}
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x00010200 File Offset: 0x0000E400
		internal int Initialize(ZlibCodec strm, CompressionLevel level, int bits)
		{
			return this.Initialize(strm, level, 8, bits, 8, CompressionStrategy.DEFAULT);
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00010228 File Offset: 0x0000E428
		internal int Initialize(ZlibCodec strm, CompressionLevel level)
		{
			return this.Initialize(strm, level, 15);
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0001024C File Offset: 0x0000E44C
		internal int Initialize(ZlibCodec stream, CompressionLevel level, int method, int windowBits, int memLevel, CompressionStrategy strategy)
		{
			stream.Message = null;
			if (windowBits < 9 || windowBits > 15)
			{
				throw new ZlibException("windowBits must be in the range 9..15.");
			}
			if (memLevel < 1 || memLevel > 9)
			{
				throw new ZlibException(string.Format("memLevel must be in the range 1.. {0}", 9));
			}
			if (method != 8)
			{
				throw new ZlibException("Unexpected value for method: it must be Z_DEFLATED.");
			}
			stream.dstate = this;
			this.w_bits = windowBits;
			this.w_size = 1 << this.w_bits;
			this.w_mask = this.w_size - 1;
			this.hash_bits = memLevel + 7;
			this.hash_size = 1 << this.hash_bits;
			this.hash_mask = this.hash_size - 1;
			this.hash_shift = (this.hash_bits + 3 - 1) / 3;
			this.window = new byte[this.w_size * 2];
			this.prev = new short[this.w_size];
			this.head = new short[this.hash_size];
			this.lit_bufsize = 1 << memLevel + 6;
			this.pending = new byte[this.lit_bufsize * 4];
			this.d_buf = this.lit_bufsize / 2;
			this.l_buf = 3 * this.lit_bufsize;
			this.compressionLevel = level;
			this.compressionStrategy = strategy;
			this.method = (sbyte)method;
			return this.Reset(stream);
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x000103DC File Offset: 0x0000E5DC
		internal int Reset(ZlibCodec strm)
		{
			strm.TotalBytesIn = (strm.TotalBytesOut = 0L);
			strm.Message = null;
			this.pendingCount = 0;
			this.nextPending = 0;
			this.Rfc1950BytesEmitted = false;
			this.status = (this.WantRfc1950HeaderBytes ? 42 : 113);
			strm._Adler32 = Adler.Adler32(0L, null, 0, 0);
			this.last_flush = 0;
			this._InitializeTreeData();
			this._InitializeLazyMatch();
			return 0;
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x00010464 File Offset: 0x0000E664
		internal int End()
		{
			int result;
			if (this.status != 42 && this.status != 113 && this.status != 666)
			{
				result = -2;
			}
			else
			{
				this.pending = null;
				this.head = null;
				this.prev = null;
				this.window = null;
				result = ((this.status == 113) ? -3 : 0);
			}
			return result;
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x000104EC File Offset: 0x0000E6EC
		internal int SetParams(ZlibCodec strm, CompressionLevel _level, CompressionStrategy _strategy)
		{
			int result = 0;
			if (DeflateManager.configTable[(int)this.compressionLevel].func != DeflateManager.configTable[(int)_level].func && strm.TotalBytesIn != 0L)
			{
				result = strm.Deflate(1);
			}
			if (this.compressionLevel != _level)
			{
				this.compressionLevel = _level;
				this.max_lazy_match = DeflateManager.configTable[(int)this.compressionLevel].max_lazy;
				this.good_match = DeflateManager.configTable[(int)this.compressionLevel].good_length;
				this.nice_match = DeflateManager.configTable[(int)this.compressionLevel].nice_length;
				this.max_chain_length = DeflateManager.configTable[(int)this.compressionLevel].max_chain;
			}
			this.compressionStrategy = _strategy;
			return result;
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x000105C8 File Offset: 0x0000E7C8
		internal int SetDictionary(ZlibCodec strm, byte[] dictionary)
		{
			int num = dictionary.Length;
			int sourceIndex = 0;
			if (dictionary == null || this.status != 42)
			{
				throw new ZlibException("Stream error.");
			}
			strm._Adler32 = Adler.Adler32(strm._Adler32, dictionary, 0, dictionary.Length);
			int result;
			if (num < 3)
			{
				result = 0;
			}
			else
			{
				if (num > this.w_size - DeflateManager.MIN_LOOKAHEAD)
				{
					num = this.w_size - DeflateManager.MIN_LOOKAHEAD;
					sourceIndex = dictionary.Length - num;
				}
				Array.Copy(dictionary, sourceIndex, this.window, 0, num);
				this.strstart = num;
				this.block_start = num;
				this.ins_h = (int)(this.window[0] & byte.MaxValue);
				this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[1] & byte.MaxValue)) & this.hash_mask);
				for (int i = 0; i <= num - 3; i++)
				{
					this.ins_h = ((this.ins_h << this.hash_shift ^ (int)(this.window[i + 2] & byte.MaxValue)) & this.hash_mask);
					this.prev[i & this.w_mask] = this.head[this.ins_h];
					this.head[this.ins_h] = (short)i;
				}
				result = 0;
			}
			return result;
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x00010744 File Offset: 0x0000E944
		internal int Deflate(ZlibCodec strm, int flush)
		{
			if (flush > 4 || flush < 0)
			{
				throw new ZlibException(string.Format("Flush value is invalid. Should be 0 < x < {0}", 4));
			}
			if (strm.OutputBuffer == null || (strm.InputBuffer == null && strm.AvailableBytesIn != 0) || (this.status == 666 && flush != 4))
			{
				strm.Message = DeflateManager.z_errmsg[4];
				throw new ZlibException(string.Format("Something is fishy. [{0}]", strm.Message));
			}
			if (strm.AvailableBytesOut == 0)
			{
				strm.Message = DeflateManager.z_errmsg[7];
				throw new ZlibException("OutputBuffer is full (AvailableBytesOut == 0)");
			}
			this.strm = strm;
			int num = this.last_flush;
			this.last_flush = flush;
			if (this.status == 42)
			{
				int num2 = 8 + (this.w_bits - 8 << 4) << 8;
				int num3 = (this.compressionLevel - CompressionLevel.BEST_SPEED & 255) >> 1;
				if (num3 > 3)
				{
					num3 = 3;
				}
				num2 |= num3 << 6;
				if (this.strstart != 0)
				{
					num2 |= 32;
				}
				num2 += 31 - num2 % 31;
				this.status = 113;
				this.putShortMSB(num2);
				if (this.strstart != 0)
				{
					this.putShortMSB((int)SharedUtils.URShift(strm._Adler32, 16));
					this.putShortMSB((int)(strm._Adler32 & 65535L));
				}
				strm._Adler32 = Adler.Adler32(0L, null, 0, 0);
			}
			if (this.pendingCount != 0)
			{
				strm.flush_pending();
				if (strm.AvailableBytesOut == 0)
				{
					this.last_flush = -1;
					return 0;
				}
			}
			else if (strm.AvailableBytesIn == 0 && flush <= num && flush != 4)
			{
				strm.Message = DeflateManager.z_errmsg[7];
				throw new ZlibException("AvailableBytesOut == 0 && flush<=old_flush && flush != ZlibConstants.Z_FINISH");
			}
			if (this.status == 666 && strm.AvailableBytesIn != 0)
			{
				strm.Message = DeflateManager.z_errmsg[7];
				throw new ZlibException("status == FINISH_STATE && strm.AvailableBytesIn != 0");
			}
			if (strm.AvailableBytesIn != 0 || this.lookahead != 0 || (flush != 0 && this.status != 666))
			{
				int num4 = -1;
				switch (DeflateManager.configTable[(int)this.compressionLevel].func)
				{
				case 0:
					num4 = this.DeflateNone(flush);
					break;
				case 1:
					num4 = this.DeflateFast(flush);
					break;
				case 2:
					num4 = this.DeflateSlow(flush);
					break;
				}
				if (num4 == 2 || num4 == 3)
				{
					this.status = 666;
				}
				if (num4 == 0 || num4 == 2)
				{
					if (strm.AvailableBytesOut == 0)
					{
						this.last_flush = -1;
					}
					return 0;
				}
				if (num4 == 1)
				{
					if (flush == 1)
					{
						this._tr_align();
					}
					else
					{
						this._tr_stored_block(0, 0, false);
						if (flush == 3)
						{
							for (int i = 0; i < this.hash_size; i++)
							{
								this.head[i] = 0;
							}
						}
					}
					strm.flush_pending();
					if (strm.AvailableBytesOut == 0)
					{
						this.last_flush = -1;
						return 0;
					}
				}
			}
			int result;
			if (flush != 4)
			{
				result = 0;
			}
			else if (!this.WantRfc1950HeaderBytes || this.Rfc1950BytesEmitted)
			{
				result = 1;
			}
			else
			{
				this.putShortMSB((int)SharedUtils.URShift(strm._Adler32, 16));
				this.putShortMSB((int)(strm._Adler32 & 65535L));
				strm.flush_pending();
				this.Rfc1950BytesEmitted = true;
				result = ((this.pendingCount != 0) ? 0 : 1);
			}
			return result;
		}

		// Token: 0x060001DA RID: 474 RVA: 0x00010BE8 File Offset: 0x0000EDE8
		static DeflateManager()
		{
			DeflateManager.configTable = new DeflateManager.Config[10];
			DeflateManager.configTable[0] = new DeflateManager.Config(0, 0, 0, 0, 0);
			DeflateManager.configTable[1] = new DeflateManager.Config(4, 4, 8, 4, 1);
			DeflateManager.configTable[2] = new DeflateManager.Config(4, 5, 16, 8, 1);
			DeflateManager.configTable[3] = new DeflateManager.Config(4, 6, 32, 32, 1);
			DeflateManager.configTable[4] = new DeflateManager.Config(4, 4, 16, 16, 2);
			DeflateManager.configTable[5] = new DeflateManager.Config(8, 16, 32, 32, 2);
			DeflateManager.configTable[6] = new DeflateManager.Config(8, 16, 128, 128, 2);
			DeflateManager.configTable[7] = new DeflateManager.Config(8, 32, 128, 256, 2);
			DeflateManager.configTable[8] = new DeflateManager.Config(32, 128, 258, 1024, 2);
			DeflateManager.configTable[9] = new DeflateManager.Config(32, 258, 258, 4096, 2);
		}

		// Token: 0x040000E2 RID: 226
		private const int MEM_LEVEL_MAX = 9;

		// Token: 0x040000E3 RID: 227
		private const int MEM_LEVEL_DEFAULT = 8;

		// Token: 0x040000E4 RID: 228
		private const int STORED = 0;

		// Token: 0x040000E5 RID: 229
		private const int FAST = 1;

		// Token: 0x040000E6 RID: 230
		private const int SLOW = 2;

		// Token: 0x040000E7 RID: 231
		private const int NeedMore = 0;

		// Token: 0x040000E8 RID: 232
		private const int BlockDone = 1;

		// Token: 0x040000E9 RID: 233
		private const int FinishStarted = 2;

		// Token: 0x040000EA RID: 234
		private const int FinishDone = 3;

		// Token: 0x040000EB RID: 235
		private const int PRESET_DICT = 32;

		// Token: 0x040000EC RID: 236
		private const int INIT_STATE = 42;

		// Token: 0x040000ED RID: 237
		private const int BUSY_STATE = 113;

		// Token: 0x040000EE RID: 238
		private const int FINISH_STATE = 666;

		// Token: 0x040000EF RID: 239
		private const int Z_DEFLATED = 8;

		// Token: 0x040000F0 RID: 240
		private const int STORED_BLOCK = 0;

		// Token: 0x040000F1 RID: 241
		private const int STATIC_TREES = 1;

		// Token: 0x040000F2 RID: 242
		private const int DYN_TREES = 2;

		// Token: 0x040000F3 RID: 243
		private const int Z_BINARY = 0;

		// Token: 0x040000F4 RID: 244
		private const int Z_ASCII = 1;

		// Token: 0x040000F5 RID: 245
		private const int Z_UNKNOWN = 2;

		// Token: 0x040000F6 RID: 246
		private const int Buf_size = 16;

		// Token: 0x040000F7 RID: 247
		private const int REP_3_6 = 16;

		// Token: 0x040000F8 RID: 248
		private const int REPZ_3_10 = 17;

		// Token: 0x040000F9 RID: 249
		private const int REPZ_11_138 = 18;

		// Token: 0x040000FA RID: 250
		private const int MIN_MATCH = 3;

		// Token: 0x040000FB RID: 251
		private const int MAX_MATCH = 258;

		// Token: 0x040000FC RID: 252
		private const int MAX_BITS = 15;

		// Token: 0x040000FD RID: 253
		private const int D_CODES = 30;

		// Token: 0x040000FE RID: 254
		private const int BL_CODES = 19;

		// Token: 0x040000FF RID: 255
		private const int LENGTH_CODES = 29;

		// Token: 0x04000100 RID: 256
		private const int LITERALS = 256;

		// Token: 0x04000101 RID: 257
		private const int END_BLOCK = 256;

		// Token: 0x04000102 RID: 258
		private static DeflateManager.Config[] configTable;

		// Token: 0x04000103 RID: 259
		private static readonly string[] z_errmsg = new string[]
		{
			"need dictionary",
			"stream end",
			"",
			"file error",
			"stream error",
			"data error",
			"insufficient memory",
			"buffer error",
			"incompatible version",
			""
		};

		// Token: 0x04000104 RID: 260
		private static readonly int MIN_LOOKAHEAD = 262;

		// Token: 0x04000105 RID: 261
		private static readonly int L_CODES = 286;

		// Token: 0x04000106 RID: 262
		private static readonly int HEAP_SIZE = 2 * DeflateManager.L_CODES + 1;

		// Token: 0x04000107 RID: 263
		internal ZlibCodec strm;

		// Token: 0x04000108 RID: 264
		internal int status;

		// Token: 0x04000109 RID: 265
		internal byte[] pending;

		// Token: 0x0400010A RID: 266
		internal int nextPending;

		// Token: 0x0400010B RID: 267
		internal int pendingCount;

		// Token: 0x0400010C RID: 268
		internal sbyte data_type;

		// Token: 0x0400010D RID: 269
		internal sbyte method;

		// Token: 0x0400010E RID: 270
		internal int last_flush;

		// Token: 0x0400010F RID: 271
		internal int w_size;

		// Token: 0x04000110 RID: 272
		internal int w_bits;

		// Token: 0x04000111 RID: 273
		internal int w_mask;

		// Token: 0x04000112 RID: 274
		internal byte[] window;

		// Token: 0x04000113 RID: 275
		internal int window_size;

		// Token: 0x04000114 RID: 276
		internal short[] prev;

		// Token: 0x04000115 RID: 277
		internal short[] head;

		// Token: 0x04000116 RID: 278
		internal int ins_h;

		// Token: 0x04000117 RID: 279
		internal int hash_size;

		// Token: 0x04000118 RID: 280
		internal int hash_bits;

		// Token: 0x04000119 RID: 281
		internal int hash_mask;

		// Token: 0x0400011A RID: 282
		internal int hash_shift;

		// Token: 0x0400011B RID: 283
		internal int block_start;

		// Token: 0x0400011C RID: 284
		internal int match_length;

		// Token: 0x0400011D RID: 285
		internal int prev_match;

		// Token: 0x0400011E RID: 286
		internal int match_available;

		// Token: 0x0400011F RID: 287
		internal int strstart;

		// Token: 0x04000120 RID: 288
		internal int match_start;

		// Token: 0x04000121 RID: 289
		internal int lookahead;

		// Token: 0x04000122 RID: 290
		internal int prev_length;

		// Token: 0x04000123 RID: 291
		internal int max_chain_length;

		// Token: 0x04000124 RID: 292
		internal int max_lazy_match;

		// Token: 0x04000125 RID: 293
		internal CompressionLevel compressionLevel;

		// Token: 0x04000126 RID: 294
		internal CompressionStrategy compressionStrategy;

		// Token: 0x04000127 RID: 295
		internal int good_match;

		// Token: 0x04000128 RID: 296
		internal int nice_match;

		// Token: 0x04000129 RID: 297
		internal short[] dyn_ltree;

		// Token: 0x0400012A RID: 298
		internal short[] dyn_dtree;

		// Token: 0x0400012B RID: 299
		internal short[] bl_tree;

		// Token: 0x0400012C RID: 300
		internal Tree l_desc = new Tree();

		// Token: 0x0400012D RID: 301
		internal Tree d_desc = new Tree();

		// Token: 0x0400012E RID: 302
		internal Tree bl_desc = new Tree();

		// Token: 0x0400012F RID: 303
		internal short[] bl_count = new short[16];

		// Token: 0x04000130 RID: 304
		internal int[] heap = new int[2 * DeflateManager.L_CODES + 1];

		// Token: 0x04000131 RID: 305
		internal int heap_len;

		// Token: 0x04000132 RID: 306
		internal int heap_max;

		// Token: 0x04000133 RID: 307
		internal sbyte[] depth = new sbyte[2 * DeflateManager.L_CODES + 1];

		// Token: 0x04000134 RID: 308
		internal int l_buf;

		// Token: 0x04000135 RID: 309
		internal int lit_bufsize;

		// Token: 0x04000136 RID: 310
		internal int last_lit;

		// Token: 0x04000137 RID: 311
		internal int d_buf;

		// Token: 0x04000138 RID: 312
		internal int opt_len;

		// Token: 0x04000139 RID: 313
		internal int static_len;

		// Token: 0x0400013A RID: 314
		internal int matches;

		// Token: 0x0400013B RID: 315
		internal int last_eob_len;

		// Token: 0x0400013C RID: 316
		internal short bi_buf;

		// Token: 0x0400013D RID: 317
		internal int bi_valid;

		// Token: 0x0400013E RID: 318
		private bool Rfc1950BytesEmitted = false;

		// Token: 0x0400013F RID: 319
		private bool _WantRfc1950HeaderBytes = true;

		// Token: 0x02000038 RID: 56
		internal class Config
		{
			// Token: 0x0600026C RID: 620 RVA: 0x00016FA0 File Offset: 0x000151A0
			internal Config(int good_length, int max_lazy, int nice_length, int max_chain, int func)
			{
				this.good_length = good_length;
				this.max_lazy = max_lazy;
				this.nice_length = nice_length;
				this.max_chain = max_chain;
				this.func = func;
			}

			// Token: 0x04000222 RID: 546
			internal int good_length;

			// Token: 0x04000223 RID: 547
			internal int max_lazy;

			// Token: 0x04000224 RID: 548
			internal int nice_length;

			// Token: 0x04000225 RID: 549
			internal int max_chain;

			// Token: 0x04000226 RID: 550
			internal int func;
		}
	}
}
