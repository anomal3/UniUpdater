using System;

namespace Ionic.Zip
{
	// Token: 0x0200001C RID: 28
	public class ExtractProgressEventArgs : ZipProgressEventArgs
	{
		// Token: 0x06000113 RID: 275 RVA: 0x00009964 File Offset: 0x00007B64
		internal ExtractProgressEventArgs(string archiveName, bool before, int entriesTotal, int entriesExtracted, ZipEntry entry, string extractLocation, bool wantOverwrite) : base(archiveName, before ? ZipProgressEventType.Extracting_BeforeExtractEntry : ZipProgressEventType.Extracting_AfterExtractEntry)
		{
			base.EntriesTotal = entriesTotal;
			base.CurrentEntry = entry;
			this._entriesExtracted = entriesExtracted;
			this._overwrite = wantOverwrite;
			this._target = extractLocation;
		}

		// Token: 0x06000114 RID: 276 RVA: 0x000099B8 File Offset: 0x00007BB8
		internal ExtractProgressEventArgs(string archiveName, ZipProgressEventType flavor) : base(archiveName, flavor)
		{
		}

		// Token: 0x06000115 RID: 277 RVA: 0x000099C8 File Offset: 0x00007BC8
		internal ExtractProgressEventArgs()
		{
		}

		// Token: 0x06000116 RID: 278 RVA: 0x000099D4 File Offset: 0x00007BD4
		internal static ExtractProgressEventArgs BeforeExtractEntry(string archiveName, ZipEntry entry, string extractLocation, bool wantOverwrite)
		{
			return new ExtractProgressEventArgs
			{
				ArchiveName = archiveName,
				EventType = ZipProgressEventType.Extracting_BeforeExtractEntry,
				CurrentEntry = entry,
				_target = extractLocation,
				_overwrite = wantOverwrite
			};
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00009A1C File Offset: 0x00007C1C
		internal static ExtractProgressEventArgs AfterExtractEntry(string archiveName, ZipEntry entry, string extractLocation, bool wantOverwrite)
		{
			return new ExtractProgressEventArgs
			{
				ArchiveName = archiveName,
				EventType = ZipProgressEventType.Extracting_AfterExtractEntry,
				CurrentEntry = entry,
				_target = extractLocation,
				_overwrite = wantOverwrite
			};
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00009A64 File Offset: 0x00007C64
		internal static ExtractProgressEventArgs ExtractAllStarted(string archiveName, string extractLocation, bool wantOverwrite)
		{
			return new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_BeforeExtractAll)
			{
				_overwrite = wantOverwrite,
				_target = extractLocation
			};
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00009A98 File Offset: 0x00007C98
		internal static ExtractProgressEventArgs ExtractAllCompleted(string archiveName, string extractLocation, bool wantOverwrite)
		{
			return new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_AfterExtractAll)
			{
				_overwrite = wantOverwrite,
				_target = extractLocation
			};
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00009ACC File Offset: 0x00007CCC
		internal static ExtractProgressEventArgs ByteUpdate(string archiveName, ZipEntry entry, long bytesWritten, long totalBytes)
		{
			return new ExtractProgressEventArgs(archiveName, ZipProgressEventType.Extracting_EntryBytesWritten)
			{
				ArchiveName = archiveName,
				CurrentEntry = entry,
				BytesTransferred = bytesWritten,
				TotalBytesToTransfer = totalBytes
			};
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x0600011B RID: 283 RVA: 0x00009B10 File Offset: 0x00007D10
		public int EntriesExtracted
		{
			get
			{
				return this._entriesExtracted;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600011C RID: 284 RVA: 0x00009B30 File Offset: 0x00007D30
		public bool Overwrite
		{
			get
			{
				return this._overwrite;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x0600011D RID: 285 RVA: 0x00009B50 File Offset: 0x00007D50
		public string ExtractLocation
		{
			get
			{
				return this._target;
			}
		}

		// Token: 0x040000A1 RID: 161
		private int _entriesExtracted;

		// Token: 0x040000A2 RID: 162
		private bool _overwrite;

		// Token: 0x040000A3 RID: 163
		private string _target;
	}
}
