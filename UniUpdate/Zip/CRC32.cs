using System;
using System.IO;

namespace Ionic.Zip
{
	// Token: 0x02000006 RID: 6
	public class CRC32
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000025 RID: 37 RVA: 0x00002F78 File Offset: 0x00001178
		public long TotalBytesRead
		{
			get
			{
				return this._TotalBytesRead;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000026 RID: 38 RVA: 0x00002F98 File Offset: 0x00001198
		public int Crc32Result
		{
			get
			{
				return (int)(~(int)this._RunningCrc32Result);
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002FB8 File Offset: 0x000011B8
		public int GetCrc32(Stream input)
		{
			return this.GetCrc32AndCopy(input, null);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002FDC File Offset: 0x000011DC
		public int GetCrc32AndCopy(Stream input, Stream output)
		{
			if (input == null)
			{
				throw new ZipException("bad input.", new ArgumentException("The input stream must not be null.", "input"));
			}
			byte[] array = new byte[8192];
			int count = 8192;
			this._TotalBytesRead = 0L;
			int i = input.Read(array, 0, count);
			if (output != null)
			{
				output.Write(array, 0, i);
			}
			this._TotalBytesRead += (long)i;
			while (i > 0)
			{
				this.SlurpBlock(array, 0, i);
				i = input.Read(array, 0, count);
				if (output != null)
				{
					output.Write(array, 0, i);
				}
				this._TotalBytesRead += (long)i;
			}
			return (int)(~(int)this._RunningCrc32Result);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000030B8 File Offset: 0x000012B8
		public int ComputeCrc32(int W, byte B)
		{
			return this.ComputeCrc32((uint)W, B);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000030DC File Offset: 0x000012DC
		internal int ComputeCrc32(uint W, byte B)
		{
			return (int)(CRC32.crc32Table[(int)((UIntPtr)((W ^ (uint)B) & 255U))] ^ W >> 8);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000310C File Offset: 0x0000130C
		public void SlurpBlock(byte[] block, int offset, int count)
		{
			if (block == null)
			{
				throw new ZipException("Bad buffer.", new ArgumentException("The data buffer must not be null.", "block"));
			}
			for (int i = 0; i < count; i++)
			{
				int num = offset + i;
				this._RunningCrc32Result = (this._RunningCrc32Result >> 8 ^ CRC32.crc32Table[(int)((UIntPtr)((uint)block[num] ^ (this._RunningCrc32Result & 255U)))]);
			}
			this._TotalBytesRead += (long)count;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003194 File Offset: 0x00001394
		static CRC32()
		{
			uint num = 3988292384U;
			CRC32.crc32Table = new uint[256];
			for (uint num2 = 0U; num2 < 256U; num2 += 1U)
			{
				uint num3 = num2;
				for (uint num4 = 8U; num4 > 0U; num4 -= 1U)
				{
					if ((num3 & 1U) == 1U)
					{
						num3 = (num3 >> 1 ^ num);
					}
					else
					{
						num3 >>= 1;
					}
				}
				CRC32.crc32Table[(int)((UIntPtr)num2)] = num3;
			}
		}

		// Token: 0x0400002A RID: 42
		private const int BUFFER_SIZE = 8192;

		// Token: 0x0400002B RID: 43
		private long _TotalBytesRead;

		// Token: 0x0400002C RID: 44
		private static uint[] crc32Table;

		// Token: 0x0400002D RID: 45
		private uint _RunningCrc32Result = uint.MaxValue;
	}
}
