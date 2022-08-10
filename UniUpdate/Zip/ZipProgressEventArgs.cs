using System;

namespace Ionic.Zip
{
	// Token: 0x02000019 RID: 25
	public class ZipProgressEventArgs : EventArgs
	{
		// Token: 0x060000F5 RID: 245 RVA: 0x000095FC File Offset: 0x000077FC
		internal ZipProgressEventArgs()
		{
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00009608 File Offset: 0x00007808
		internal ZipProgressEventArgs(string archiveName, ZipProgressEventType flavor)
		{
			this._archiveName = archiveName;
			this._flavor = flavor;
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000F7 RID: 247 RVA: 0x00009624 File Offset: 0x00007824
		// (set) Token: 0x060000F8 RID: 248 RVA: 0x00009644 File Offset: 0x00007844
		public int EntriesTotal
		{
			get
			{
				return this._entriesTotal;
			}
			set
			{
				this._entriesTotal = value;
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x00009650 File Offset: 0x00007850
		// (set) Token: 0x060000FA RID: 250 RVA: 0x00009670 File Offset: 0x00007870
		public ZipEntry CurrentEntry
		{
			get
			{
				return this._latestEntry;
			}
			set
			{
				this._latestEntry = value;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000FB RID: 251 RVA: 0x0000967C File Offset: 0x0000787C
		// (set) Token: 0x060000FC RID: 252 RVA: 0x0000969C File Offset: 0x0000789C
		public bool Cancel
		{
			get
			{
				return this._cancel;
			}
			set
			{
				this._cancel = (this._cancel || value);
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000FD RID: 253 RVA: 0x000096B8 File Offset: 0x000078B8
		// (set) Token: 0x060000FE RID: 254 RVA: 0x000096D8 File Offset: 0x000078D8
		public ZipProgressEventType EventType
		{
			get
			{
				return this._flavor;
			}
			set
			{
				this._flavor = value;
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000FF RID: 255 RVA: 0x000096E4 File Offset: 0x000078E4
		// (set) Token: 0x06000100 RID: 256 RVA: 0x00009704 File Offset: 0x00007904
		public string ArchiveName
		{
			get
			{
				return this._archiveName;
			}
			set
			{
				this._archiveName = value;
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000101 RID: 257 RVA: 0x00009710 File Offset: 0x00007910
		// (set) Token: 0x06000102 RID: 258 RVA: 0x00009730 File Offset: 0x00007930
		public long BytesTransferred
		{
			get
			{
				return this._bytesTransferred;
			}
			set
			{
				this._bytesTransferred = value;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000103 RID: 259 RVA: 0x0000973C File Offset: 0x0000793C
		// (set) Token: 0x06000104 RID: 260 RVA: 0x0000975C File Offset: 0x0000795C
		public long TotalBytesToTransfer
		{
			get
			{
				return this._totalBytesToTransfer;
			}
			set
			{
				this._totalBytesToTransfer = value;
			}
		}

		// Token: 0x04000099 RID: 153
		private int _entriesTotal;

		// Token: 0x0400009A RID: 154
		private bool _cancel;

		// Token: 0x0400009B RID: 155
		private ZipEntry _latestEntry;

		// Token: 0x0400009C RID: 156
		private ZipProgressEventType _flavor;

		// Token: 0x0400009D RID: 157
		private string _archiveName;

		// Token: 0x0400009E RID: 158
		private long _bytesTransferred;

		// Token: 0x0400009F RID: 159
		private long _totalBytesToTransfer;
	}
}
