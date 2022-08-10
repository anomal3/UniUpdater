using System;

namespace Ionic.Zip
{
	// Token: 0x0200001B RID: 27
	public class SaveProgressEventArgs : ZipProgressEventArgs
	{
		// Token: 0x0600010C RID: 268 RVA: 0x00009868 File Offset: 0x00007A68
		internal SaveProgressEventArgs(string archiveName, bool before, int entriesTotal, int entriesSaved, ZipEntry entry) : base(archiveName, before ? ZipProgressEventType.Saving_BeforeWriteEntry : ZipProgressEventType.Saving_AfterWriteEntry)
		{
			base.EntriesTotal = entriesTotal;
			base.CurrentEntry = entry;
			this._entriesSaved = entriesSaved;
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000989C File Offset: 0x00007A9C
		internal SaveProgressEventArgs()
		{
		}

		// Token: 0x0600010E RID: 270 RVA: 0x000098A8 File Offset: 0x00007AA8
		internal SaveProgressEventArgs(string archiveName, ZipProgressEventType flavor) : base(archiveName, flavor)
		{
		}

		// Token: 0x0600010F RID: 271 RVA: 0x000098B8 File Offset: 0x00007AB8
		internal static SaveProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, long bytesXferred, long totalBytes)
		{
			return new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_EntryBytesRead)
			{
				ArchiveName = archiveName,
				CurrentEntry = entry,
				BytesTransferred = bytesXferred,
				TotalBytesToTransfer = totalBytes
			};
		}

		// Token: 0x06000110 RID: 272 RVA: 0x000098FC File Offset: 0x00007AFC
		internal static SaveProgressEventArgs Started(string archiveName)
		{
			return new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_Started);
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00009920 File Offset: 0x00007B20
		internal static SaveProgressEventArgs Completed(string archiveName)
		{
			return new SaveProgressEventArgs(archiveName, ZipProgressEventType.Saving_Completed);
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00009944 File Offset: 0x00007B44
		public int EntriesSaved
		{
			get
			{
				return this._entriesSaved;
			}
		}

		// Token: 0x040000A0 RID: 160
		private int _entriesSaved;
	}
}
