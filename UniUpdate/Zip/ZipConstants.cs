using System;

namespace Ionic.Zip
{
	// Token: 0x02000005 RID: 5
	internal static class ZipConstants
	{
		// Token: 0x0400001E RID: 30
		public const uint PackedToRemovableMedia = 808471376U;

		// Token: 0x0400001F RID: 31
		public const uint Zip64EndOfCentralDirectoryRecordSignature = 101075792U;

		// Token: 0x04000020 RID: 32
		public const uint Zip64EndOfCentralDirectoryLocatorSignature = 117853008U;

		// Token: 0x04000021 RID: 33
		public const uint EndOfCentralDirectorySignature = 101010256U;

		// Token: 0x04000022 RID: 34
		public const int ZipEntrySignature = 67324752;

		// Token: 0x04000023 RID: 35
		public const int ZipEntryDataDescriptorSignature = 134695760;

		// Token: 0x04000024 RID: 36
		public const int ZipDirEntrySignature = 33639248;

		// Token: 0x04000025 RID: 37
		public const int AesKeySize = 192;

		// Token: 0x04000026 RID: 38
		public const int AesBlockSize = 128;

		// Token: 0x04000027 RID: 39
		public const ushort AesAlgId128 = 26126;

		// Token: 0x04000028 RID: 40
		public const ushort AesAlgId192 = 26127;

		// Token: 0x04000029 RID: 41
		public const ushort AesAlgId256 = 26128;
	}
}
