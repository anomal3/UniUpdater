using System;

namespace Ionic.Zip
{
	// Token: 0x02000018 RID: 24
	public enum ZipProgressEventType
	{
		// Token: 0x04000086 RID: 134
		Reading_Started,
		// Token: 0x04000087 RID: 135
		Reading_BeforeReadEntry,
		// Token: 0x04000088 RID: 136
		Reading_AfterReadEntry,
		// Token: 0x04000089 RID: 137
		Reading_Completed,
		// Token: 0x0400008A RID: 138
		Reading_ArchiveBytesRead,
		// Token: 0x0400008B RID: 139
		Saving_Started,
		// Token: 0x0400008C RID: 140
		Saving_BeforeWriteEntry,
		// Token: 0x0400008D RID: 141
		Saving_AfterWriteEntry,
		// Token: 0x0400008E RID: 142
		Saving_Completed,
		// Token: 0x0400008F RID: 143
		Saving_AfterSaveTempArchive,
		// Token: 0x04000090 RID: 144
		Saving_BeforeRenameTempArchive,
		// Token: 0x04000091 RID: 145
		Saving_AfterRenameTempArchive,
		// Token: 0x04000092 RID: 146
		Saving_AfterCompileSelfExtractor,
		// Token: 0x04000093 RID: 147
		Saving_EntryBytesRead,
		// Token: 0x04000094 RID: 148
		Extracting_BeforeExtractEntry,
		// Token: 0x04000095 RID: 149
		Extracting_AfterExtractEntry,
		// Token: 0x04000096 RID: 150
		Extracting_EntryBytesWritten,
		// Token: 0x04000097 RID: 151
		Extracting_BeforeExtractAll,
		// Token: 0x04000098 RID: 152
		Extracting_AfterExtractAll
	}
}
