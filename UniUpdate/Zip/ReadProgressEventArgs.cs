using System;

namespace Ionic.Zip
{
	// Token: 0x0200001A RID: 26
	public class ReadProgressEventArgs : ZipProgressEventArgs
	{
		// Token: 0x06000105 RID: 261 RVA: 0x00009768 File Offset: 0x00007968
		internal ReadProgressEventArgs()
		{
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00009774 File Offset: 0x00007974
		private ReadProgressEventArgs(string archiveName, ZipProgressEventType flavor) : base(archiveName, flavor)
		{
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00009784 File Offset: 0x00007984
		internal static ReadProgressEventArgs Before(string archiveName, int entriesTotal)
		{
			return new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_BeforeReadEntry)
			{
				EntriesTotal = entriesTotal
			};
		}

		// Token: 0x06000108 RID: 264 RVA: 0x000097B0 File Offset: 0x000079B0
		internal static ReadProgressEventArgs After(string archiveName, ZipEntry entry, int entriesTotal)
		{
			return new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_AfterReadEntry)
			{
				EntriesTotal = entriesTotal,
				CurrentEntry = entry
			};
		}

		// Token: 0x06000109 RID: 265 RVA: 0x000097E4 File Offset: 0x000079E4
		internal static ReadProgressEventArgs Started(string archiveName)
		{
			return new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Started);
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00009808 File Offset: 0x00007A08
		internal static ReadProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, long bytesXferred, long totalBytes)
		{
			return new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_ArchiveBytesRead)
			{
				CurrentEntry = entry,
				BytesTransferred = bytesXferred,
				TotalBytesToTransfer = totalBytes
			};
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00009844 File Offset: 0x00007A44
		internal static ReadProgressEventArgs Completed(string archiveName)
		{
			return new ReadProgressEventArgs(archiveName, ZipProgressEventType.Reading_Completed);
		}
	}
}
