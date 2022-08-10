using System;

namespace Ionic.Zlib
{
	// Token: 0x0200002F RID: 47
	internal sealed class Adler
	{
		// Token: 0x0600020F RID: 527 RVA: 0x00015870 File Offset: 0x00013A70
		internal static long Adler32(long adler, byte[] buf, int index, int len)
		{
			long result;
			if (buf == null)
			{
				result = 1L;
			}
			else
			{
				long num = adler & 65535L;
				long num2 = adler >> 16 & 65535L;
				while (len > 0)
				{
					int i = (len < Adler.NMAX) ? len : Adler.NMAX;
					len -= i;
					while (i >= 16)
					{
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						num += (long)(buf[index++] & byte.MaxValue);
						num2 += num;
						i -= 16;
					}
					if (i != 0)
					{
						do
						{
							num += (long)(buf[index++] & byte.MaxValue);
							num2 += num;
						}
						while (--i != 0);
					}
					num %= (long)Adler.BASE;
					num2 %= (long)Adler.BASE;
				}
				result = (num2 << 16 | num);
			}
			return result;
		}

		// Token: 0x040001EB RID: 491
		private static int BASE = 65521;

		// Token: 0x040001EC RID: 492
		private static int NMAX = 5552;
	}
}
