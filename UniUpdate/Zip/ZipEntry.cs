using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ionic.Zlib;

namespace Ionic.Zip
{
	// Token: 0x02000013 RID: 19
	public class ZipEntry
	{
		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600008C RID: 140 RVA: 0x00004548 File Offset: 0x00002748
		internal bool AttributesIndicateDirectory
		{
			get
			{
				return this._InternalFileAttrs == 0 && (this._ExternalFileAttrs & 16) == 16;
			}
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00004580 File Offset: 0x00002780
		internal void ResetDirEntry()
		{
			this.__FileDataPosition = -1L;
			this._LengthOfHeader = 0;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00004594 File Offset: 0x00002794
		public static ZipEntry ReadDirEntry(Stream s, Encoding expectedEncoding)
		{
			long position = s.Position;
			int num = SharedUtilities.ReadSignature(s);
			ZipEntry result;
			if (ZipEntry.IsNotValidZipDirEntrySig(num))
			{
				s.Seek(-4L, SeekOrigin.Current);
				if ((long)num != 101010256L && (long)num != 101075792L)
				{
					throw new BadReadException(string.Format("  ZipEntry::ReadDirEntry(): Bad signature (0x{0:X8}) at position 0x{1:X8}", num, s.Position));
				}
				result = null;
			}
			else
			{
				int num2 = 46;
				byte[] array = new byte[42];
				int num3 = s.Read(array, 0, array.Length);
				if (num3 != array.Length)
				{
					result = null;
				}
				else
				{
					int num4 = 0;
					ZipEntry zipEntry = new ZipEntry();
					zipEntry._archiveStream = s;
					zipEntry._cdrPosition = position;
					zipEntry._VersionMadeBy = (short)((int)array[num4++] + (int)array[num4++] * 256);
					zipEntry._VersionNeeded = (short)((int)array[num4++] + (int)array[num4++] * 256);
					zipEntry._BitField = (short)((int)array[num4++] + (int)array[num4++] * 256);
					zipEntry._CompressionMethod = (short)((int)array[num4++] + (int)array[num4++] * 256);
					zipEntry._TimeBlob = (int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256;
					zipEntry._LastModified = SharedUtilities.PackedToDateTime(zipEntry._TimeBlob);
					zipEntry._Crc32 = (int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256;
					zipEntry._CompressedSize = (long)((ulong)((int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256));
					zipEntry._UncompressedSize = (long)((ulong)((int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256));
					zipEntry._filenameLength = (short)((int)array[num4++] + (int)array[num4++] * 256);
					zipEntry._extraFieldLength = (short)((int)array[num4++] + (int)array[num4++] * 256);
					zipEntry._commentLength = (short)((int)array[num4++] + (int)array[num4++] * 256);
					num4 += 2;
					zipEntry._InternalFileAttrs = (short)((int)array[num4++] + (int)array[num4++] * 256);
					zipEntry._ExternalFileAttrs = (int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256;
					zipEntry._RelativeOffsetOfLocalHeader = (long)((ulong)((int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256));
					array = new byte[(int)zipEntry._filenameLength];
					num3 = s.Read(array, 0, array.Length);
					num2 += num3;
					if ((zipEntry._BitField & 2048) == 2048)
					{
						zipEntry._LocalFileName = SharedUtilities.Utf8StringFromBuffer(array, array.Length);
					}
					else
					{
						zipEntry._LocalFileName = SharedUtilities.StringFromBuffer(array, array.Length, expectedEncoding);
					}
					zipEntry._FileNameInArchive = zipEntry._LocalFileName;
					if (zipEntry.AttributesIndicateDirectory)
					{
						zipEntry.MarkAsDirectory();
					}
					if (zipEntry._LocalFileName.EndsWith("/"))
					{
						zipEntry.MarkAsDirectory();
					}
					zipEntry._CompressedFileDataSize = zipEntry._CompressedSize;
					if ((zipEntry._BitField & 1) == 1)
					{
						zipEntry._Encryption = EncryptionAlgorithm.PkzipWeak;
						zipEntry._sourceIsEncrypted = true;
					}
					if (zipEntry._extraFieldLength > 0)
					{
						zipEntry._InputUsesZip64 = ((uint)zipEntry._CompressedSize == uint.MaxValue || (uint)zipEntry._UncompressedSize == uint.MaxValue || (uint)zipEntry._RelativeOffsetOfLocalHeader == uint.MaxValue);
						num2 += zipEntry.ProcessExtraField(zipEntry._extraFieldLength);
						zipEntry._CompressedFileDataSize = zipEntry._CompressedSize;
					}
					if (zipEntry._Encryption == EncryptionAlgorithm.PkzipWeak)
					{
						zipEntry._CompressedFileDataSize -= 12L;
					}
					else if (zipEntry.Encryption == EncryptionAlgorithm.WinZipAes128 || zipEntry.Encryption == EncryptionAlgorithm.WinZipAes256)
					{
						zipEntry._CompressedFileDataSize = zipEntry.CompressedSize - (long)(zipEntry._KeyStrengthInBits / 8 / 2 + 10 + 2);
						zipEntry._LengthOfTrailer = 10;
					}
					if (zipEntry._commentLength > 0)
					{
						array = new byte[(int)zipEntry._commentLength];
						num3 = s.Read(array, 0, array.Length);
						num2 += num3;
						if ((zipEntry._BitField & 2048) == 2048)
						{
							zipEntry._Comment = SharedUtilities.Utf8StringFromBuffer(array, array.Length);
						}
						else
						{
							zipEntry._Comment = SharedUtilities.StringFromBuffer(array, array.Length, expectedEncoding);
						}
					}
					zipEntry._LengthOfDirEntry = num2;
					result = zipEntry;
				}
			}
			return result;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00004C14 File Offset: 0x00002E14
		internal static bool IsNotValidZipDirEntrySig(int signature)
		{
			return signature != 33639248;
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00004C38 File Offset: 0x00002E38
		internal ZipEntry()
		{
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000091 RID: 145 RVA: 0x00004C98 File Offset: 0x00002E98
		// (set) Token: 0x06000092 RID: 146 RVA: 0x00004CB8 File Offset: 0x00002EB8
		public DateTime LastModified
		{
			get
			{
				return this._LastModified;
			}
			set
			{
				this._LastModified = value;
				this._metadataChanged = true;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000093 RID: 147 RVA: 0x00004CCC File Offset: 0x00002ECC
		// (set) Token: 0x06000094 RID: 148 RVA: 0x00004CEC File Offset: 0x00002EEC
		public bool ForceNoCompression
		{
			get
			{
				return this._ForceNoCompression;
			}
			set
			{
				if (value != this._ForceNoCompression)
				{
					if (this._Source == EntrySource.Zipfile && this._sourceIsEncrypted)
					{
						throw new InvalidOperationException("Cannot change compression method on encrypted entries read from archives.");
					}
					this._ForceNoCompression = value;
				}
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00004D4C File Offset: 0x00002F4C
		public string LocalFileName
		{
			get
			{
				return this._LocalFileName;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000096 RID: 150 RVA: 0x00004D6C File Offset: 0x00002F6C
		// (set) Token: 0x06000097 RID: 151 RVA: 0x00004D8C File Offset: 0x00002F8C
		public string FileName
		{
			get
			{
				return this._FileNameInArchive;
			}
			set
			{
				if (value == null || value == "")
				{
					throw new ZipException("The FileName must be non empty and non-null.");
				}
				string text = ZipEntry.NameInArchive(value, null);
				this._FileNameInArchive = value;
				if (this._zipfile != null)
				{
					this._zipfile.NotifyEntryChanged();
				}
				this._metadataChanged = true;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000098 RID: 152 RVA: 0x00004DFC File Offset: 0x00002FFC
		public short VersionNeeded
		{
			get
			{
				return this._VersionNeeded;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000099 RID: 153 RVA: 0x00004E1C File Offset: 0x0000301C
		// (set) Token: 0x0600009A RID: 154 RVA: 0x00004E3C File Offset: 0x0000303C
		public string Comment
		{
			get
			{
				return this._Comment;
			}
			set
			{
				this._Comment = value;
				this._metadataChanged = true;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600009B RID: 155 RVA: 0x00004E50 File Offset: 0x00003050
		public bool? RequiresZip64
		{
			get
			{
				return this._entryRequiresZip64;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600009C RID: 156 RVA: 0x00004E70 File Offset: 0x00003070
		public bool? OutputUsedZip64
		{
			get
			{
				return this._OutputUsesZip64;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600009D RID: 157 RVA: 0x00004E90 File Offset: 0x00003090
		public short BitField
		{
			get
			{
				return this._BitField;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600009E RID: 158 RVA: 0x00004EB0 File Offset: 0x000030B0
		// (set) Token: 0x0600009F RID: 159 RVA: 0x00004ED0 File Offset: 0x000030D0
		public short CompressionMethod
		{
			get
			{
				return this._CompressionMethod;
			}
			set
			{
				if (value != this._CompressionMethod)
				{
					if (value != 0 && value != 8)
					{
						throw new InvalidOperationException("Unsupported compression method. Specify 8 or 0.");
					}
					if (this._Source == EntrySource.Zipfile && this._sourceIsEncrypted)
					{
						throw new InvalidOperationException("Cannot change compression method on encrypted entries read from archives.");
					}
					this._CompressionMethod = value;
					this._ForceNoCompression = (this._CompressionMethod == 0);
					this._restreamRequiredOnSave = true;
				}
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000A0 RID: 160 RVA: 0x00004F68 File Offset: 0x00003168
		public long CompressedSize
		{
			get
			{
				return this._CompressedSize;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x00004F88 File Offset: 0x00003188
		public long UncompressedSize
		{
			get
			{
				return this._UncompressedSize;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000A2 RID: 162 RVA: 0x00004FA8 File Offset: 0x000031A8
		public double CompressionRatio
		{
			get
			{
				double result;
				if (this.UncompressedSize == 0L)
				{
					result = 0.0;
				}
				else
				{
					result = 100.0 * (1.0 - 1.0 * (double)this.CompressedSize / (1.0 * (double)this.UncompressedSize));
				}
				return result;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x0000501C File Offset: 0x0000321C
		public int Crc32
		{
			get
			{
				return this._Crc32;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x0000503C File Offset: 0x0000323C
		public bool IsDirectory
		{
			get
			{
				return this._IsDirectory;
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x0000505C File Offset: 0x0000325C
		public bool UsesEncryption
		{
			get
			{
				return this.Encryption != EncryptionAlgorithm.None;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x00005084 File Offset: 0x00003284
		// (set) Token: 0x060000A7 RID: 167 RVA: 0x000050A4 File Offset: 0x000032A4
		public EncryptionAlgorithm Encryption
		{
			get
			{
				return this._Encryption;
			}
			set
			{
				if (value != this._Encryption)
				{
					this._Encryption = value;
					if (this._Source == EntrySource.Zipfile && this._sourceIsEncrypted)
					{
						throw new InvalidOperationException("You cannot change the encryption method on encrypted entries read from archives.");
					}
					this._restreamRequiredOnSave = true;
					if (value == EncryptionAlgorithm.WinZipAes256)
					{
						this._KeyStrengthInBits = 256;
					}
					else if (value == EncryptionAlgorithm.WinZipAes128)
					{
						this._KeyStrengthInBits = 128;
					}
				}
			}
		}

		// Token: 0x17000037 RID: 55
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x00005140 File Offset: 0x00003340
		public string Password
		{
			set
			{
				this._Password = value;
				if (this._Password == null)
				{
					this._Encryption = EncryptionAlgorithm.None;
				}
				else
				{
					if (this._Source == EntrySource.Zipfile && !this._sourceIsEncrypted)
					{
						this._restreamRequiredOnSave = true;
					}
					if (this.Encryption == EncryptionAlgorithm.None)
					{
						this._Encryption = EncryptionAlgorithm.PkzipWeak;
					}
				}
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x000051C0 File Offset: 0x000033C0
		// (set) Token: 0x060000AA RID: 170 RVA: 0x000051E0 File Offset: 0x000033E0
		public bool OverwriteOnExtract
		{
			get
			{
				return this._OverwriteOnExtract;
			}
			set
			{
				this._OverwriteOnExtract = value;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000AB RID: 171 RVA: 0x000051EC File Offset: 0x000033EC
		// (set) Token: 0x060000AC RID: 172 RVA: 0x0000520C File Offset: 0x0000340C
		public ReReadApprovalCallback WillReadTwiceOnInflation { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00005218 File Offset: 0x00003418
		// (set) Token: 0x060000AE RID: 174 RVA: 0x00005238 File Offset: 0x00003438
		public WantCompressionCallback WantCompression { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00005244 File Offset: 0x00003444
		// (set) Token: 0x060000B0 RID: 176 RVA: 0x00005270 File Offset: 0x00003470
		public bool UseUnicodeAsNecessary
		{
			get
			{
				return this._provisionalAlternateEncoding == Encoding.GetEncoding("UTF-8");
			}
			set
			{
				this._provisionalAlternateEncoding = (value ? Encoding.GetEncoding("UTF-8") : ZipFile.DefaultEncoding);
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00005294 File Offset: 0x00003494
		// (set) Token: 0x060000B2 RID: 178 RVA: 0x000052B4 File Offset: 0x000034B4
		public Encoding ProvisionalAlternateEncoding
		{
			get
			{
				return this._provisionalAlternateEncoding;
			}
			set
			{
				this._provisionalAlternateEncoding = value;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x000052C0 File Offset: 0x000034C0
		public Encoding ActualEncoding
		{
			get
			{
				return this._actualEncoding;
			}
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x000052E0 File Offset: 0x000034E0
		private static bool ReadHeader(ZipEntry ze, Encoding defaultEncoding)
		{
			int num = 0;
			ze._RelativeOffsetOfLocalHeader = ze.ArchiveStream.Position;
			int num2 = SharedUtilities.ReadSignature(ze.ArchiveStream);
			num += 4;
			bool result;
			if (ZipEntry.IsNotValidSig(num2))
			{
				ze.ArchiveStream.Seek(-4L, SeekOrigin.Current);
				if (ZipEntry.IsNotValidZipDirEntrySig(num2) && (long)num2 != 101010256L)
				{
					throw new BadReadException(string.Format("  ZipEntry::ReadHeader(): Bad signature (0x{0:X8}) at position  0x{1:X8}", num2, ze.ArchiveStream.Position));
				}
				result = false;
			}
			else
			{
				byte[] array = new byte[26];
				int num3 = ze.ArchiveStream.Read(array, 0, array.Length);
				if (num3 != array.Length)
				{
					result = false;
				}
				else
				{
					num += num3;
					int num4 = 0;
					ze._VersionNeeded = (short)((int)array[num4++] + (int)array[num4++] * 256);
					ze._BitField = (short)((int)array[num4++] + (int)array[num4++] * 256);
					ze._CompressionMethod = (short)((int)array[num4++] + (int)array[num4++] * 256);
					ze._TimeBlob = (int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256;
					ze._LastModified = SharedUtilities.PackedToDateTime(ze._TimeBlob);
					ze._Crc32 = (int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256;
					ze._CompressedSize = (long)((ulong)((int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256));
					ze._UncompressedSize = (long)((ulong)((int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256));
					if ((uint)ze._CompressedSize == 4294967295U || (uint)ze._UncompressedSize == 4294967295U)
					{
						ze._InputUsesZip64 = true;
					}
					short num5 = (short)((int)array[num4++] + (int)array[num4++] * 256);
					short extraFieldLength = (short)((int)array[num4++] + (int)array[num4++] * 256);
					array = new byte[(int)num5];
					num3 = ze.ArchiveStream.Read(array, 0, array.Length);
					num += num3;
					ze._actualEncoding = (((ze._BitField & 2048) == 2048) ? Encoding.UTF8 : defaultEncoding);
					ze._FileNameInArchive = ze._actualEncoding.GetString(array, 0, array.Length);
					ze._LocalFileName = ze._FileNameInArchive;
					if (ze._LocalFileName.EndsWith("/"))
					{
						ze.MarkAsDirectory();
					}
					num += ze.ProcessExtraField(extraFieldLength);
					ze._LengthOfTrailer = 0;
					if (!ze._LocalFileName.EndsWith("/") && (ze._BitField & 8) == 8)
					{
						long position = ze.ArchiveStream.Position;
						bool flag = true;
						long num6 = 0L;
						int num7 = 0;
						while (flag)
						{
							num7++;
							ze._zipfile.OnReadBytes(ze);
							long num8 = SharedUtilities.FindSignature(ze.ArchiveStream, 134695760);
							if (num8 == -1L)
							{
								return false;
							}
							num6 += num8;
							if (ze._InputUsesZip64)
							{
								array = new byte[20];
								num3 = ze.ArchiveStream.Read(array, 0, array.Length);
								if (num3 != 20)
								{
									return false;
								}
								num4 = 0;
								ze._Crc32 = (int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256;
								ze._CompressedSize = BitConverter.ToInt64(array, num4);
								num4 += 8;
								ze._UncompressedSize = BitConverter.ToInt64(array, num4);
								num4 += 8;
								ze._LengthOfTrailer += 24;
							}
							else
							{
								array = new byte[12];
								num3 = ze.ArchiveStream.Read(array, 0, array.Length);
								if (num3 != 12)
								{
									return false;
								}
								num4 = 0;
								ze._Crc32 = (int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256;
								ze._CompressedSize = (long)((ulong)((int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256));
								ze._UncompressedSize = (long)((ulong)((int)array[num4++] + (int)array[num4++] * 256 + (int)array[num4++] * 256 * 256 + (int)array[num4++] * 256 * 256 * 256));
								ze._LengthOfTrailer += 16;
							}
							flag = (num6 != ze._CompressedSize);
							if (flag)
							{
								ze.ArchiveStream.Seek(-12L, SeekOrigin.Current);
								num6 += 4L;
							}
						}
						ze.ArchiveStream.Seek(position, SeekOrigin.Begin);
					}
					ze._CompressedFileDataSize = ze._CompressedSize;
					if ((ze._BitField & 1) == 1)
					{
						if (ze.Encryption == EncryptionAlgorithm.WinZipAes128 || ze.Encryption == EncryptionAlgorithm.WinZipAes256)
						{
							ze._aesCrypto = WinZipAesCrypto.ReadFromStream(null, (int)ze._KeyStrengthInBits, ze.ArchiveStream);
							num += ze._aesCrypto.SizeOfEncryptionMetadata - 10;
							ze._CompressedFileDataSize = ze.CompressedSize - (long)ze._aesCrypto.SizeOfEncryptionMetadata;
							ze._LengthOfTrailer += 10;
						}
						else
						{
							ze._WeakEncryptionHeader = new byte[12];
							num += ZipEntry.ReadWeakEncryptionHeader(ze._archiveStream, ze._WeakEncryptionHeader);
							ze._CompressedFileDataSize -= 12L;
						}
					}
					ze._LengthOfHeader = num;
					ze._TotalEntrySize = (long)ze._LengthOfHeader + ze._CompressedSize + (long)ze._LengthOfTrailer;
					result = true;
				}
			}
			return result;
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00005A98 File Offset: 0x00003C98
		internal static int ReadWeakEncryptionHeader(Stream s, byte[] buffer)
		{
			int num = s.Read(buffer, 0, 12);
			if (num != 12)
			{
				throw new ZipException(string.Format("Unexpected end of data at position 0x{0:X8}", s.Position));
			}
			return num;
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00005AE4 File Offset: 0x00003CE4
		private static bool IsNotValidSig(int signature)
		{
			return signature != 67324752;
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00005B08 File Offset: 0x00003D08
		internal static ZipEntry Read(ZipFile zf, bool first)
		{
			Stream readStream = zf.ReadStream;
			Encoding provisionalAlternateEncoding = zf.ProvisionalAlternateEncoding;
			ZipEntry zipEntry = new ZipEntry();
			zipEntry._Source = EntrySource.Zipfile;
			zipEntry._zipfile = zf;
			zipEntry._archiveStream = readStream;
			zf.OnReadEntry(true, null);
			if (first)
			{
				ZipEntry.HandlePK00Prefix(readStream);
			}
			ZipEntry result;
			if (!ZipEntry.ReadHeader(zipEntry, provisionalAlternateEncoding))
			{
				result = null;
			}
			else
			{
				zipEntry.__FileDataPosition = zipEntry.ArchiveStream.Position;
				readStream.Seek(zipEntry._CompressedFileDataSize, SeekOrigin.Current);
				if ((zipEntry._BitField & 8) == 8 && !zipEntry.FileName.EndsWith("/"))
				{
					int num = zipEntry._InputUsesZip64 ? 24 : 16;
					readStream.Seek((long)num, SeekOrigin.Current);
				}
				ZipEntry.HandleUnexpectedDataDescriptor(zipEntry);
				zf.OnReadBytes(zipEntry);
				zf.OnReadEntry(false, zipEntry);
				result = zipEntry;
			}
			return result;
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00005C04 File Offset: 0x00003E04
		internal static void HandlePK00Prefix(Stream s)
		{
			uint num = (uint)SharedUtilities.ReadInt(s);
			if (num != 808471376U)
			{
				s.Seek(-4L, SeekOrigin.Current);
			}
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00005C3C File Offset: 0x00003E3C
		private static void HandleUnexpectedDataDescriptor(ZipEntry entry)
		{
			Stream archiveStream = entry.ArchiveStream;
			uint num = (uint)SharedUtilities.ReadInt(archiveStream);
			if ((ulong)num == (ulong)((long)entry._Crc32))
			{
				int num2 = SharedUtilities.ReadInt(archiveStream);
				if ((long)num2 == entry._CompressedSize)
				{
					num2 = SharedUtilities.ReadInt(archiveStream);
					if ((long)num2 != entry._UncompressedSize)
					{
						archiveStream.Seek(-12L, SeekOrigin.Current);
					}
				}
				else
				{
					archiveStream.Seek(-8L, SeekOrigin.Current);
				}
			}
			else
			{
				archiveStream.Seek(-4L, SeekOrigin.Current);
			}
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00005CE0 File Offset: 0x00003EE0
		internal static string NameInArchive(string filename, string directoryPathInArchive)
		{
			string pathName;
			if (directoryPathInArchive == null)
			{
				pathName = filename;
			}
			else if (string.IsNullOrEmpty(directoryPathInArchive))
			{
				pathName = Path.GetFileName(filename);
			}
			else
			{
				pathName = Path.Combine(directoryPathInArchive, Path.GetFileName(filename));
			}
			return SharedUtilities.TrimVolumeAndSwapSlashes(pathName);
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00005D48 File Offset: 0x00003F48
		internal static ZipEntry Create(string filename, string nameInArchive)
		{
			return ZipEntry.Create(filename, nameInArchive, null);
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00005D6C File Offset: 0x00003F6C
		internal static ZipEntry Create(string filename, string nameInArchive, Stream stream)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ZipException("The entry name must be non-null and non-empty.");
			}
			ZipEntry zipEntry = new ZipEntry();
			if (stream != null)
			{
				zipEntry._sourceStream = stream;
				zipEntry._LastModified = DateTime.Now;
			}
			else
			{
				zipEntry._LastModified = ((File.Exists(filename) || Directory.Exists(filename)) ? SharedUtilities.RoundToEvenSecond(File.GetLastWriteTime(filename)) : DateTime.Now);
				if (!zipEntry._LastModified.IsDaylightSavingTime() && DateTime.Now.IsDaylightSavingTime())
				{
					zipEntry._LastModified += new TimeSpan(1, 0, 0);
				}
				if (zipEntry._LastModified.IsDaylightSavingTime() && !DateTime.Now.IsDaylightSavingTime())
				{
					zipEntry._LastModified -= new TimeSpan(1, 0, 0);
				}
			}
			zipEntry._LocalFileName = filename;
			zipEntry._FileNameInArchive = nameInArchive.Replace('\\', '/');
			return zipEntry;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00005EA4 File Offset: 0x000040A4
		public void Extract()
		{
			this.InternalExtract(".", null, null);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00005EB8 File Offset: 0x000040B8
		public void Extract(bool overwrite)
		{
			this.OverwriteOnExtract = overwrite;
			this.InternalExtract(".", null, null);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00005ED4 File Offset: 0x000040D4
		public void Extract(Stream stream)
		{
			this.InternalExtract(null, stream, null);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00005EE4 File Offset: 0x000040E4
		public void Extract(string baseDirectory)
		{
			this.InternalExtract(baseDirectory, null, null);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00005EF4 File Offset: 0x000040F4
		public void Extract(string baseDirectory, bool overwrite)
		{
			this.OverwriteOnExtract = overwrite;
			this.InternalExtract(baseDirectory, null, null);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00005F0C File Offset: 0x0000410C
		public void ExtractWithPassword(string password)
		{
			this.InternalExtract(".", null, password);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00005F20 File Offset: 0x00004120
		public void ExtractWithPassword(string baseDirectory, string password)
		{
			this.InternalExtract(baseDirectory, null, password);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00005F30 File Offset: 0x00004130
		public void ExtractWithPassword(bool overwrite, string password)
		{
			this.OverwriteOnExtract = overwrite;
			this.InternalExtract(".", null, password);
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00005F4C File Offset: 0x0000414C
		public void ExtractWithPassword(string baseDirectory, bool overwrite, string password)
		{
			this.OverwriteOnExtract = overwrite;
			this.InternalExtract(baseDirectory, null, password);
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00005F64 File Offset: 0x00004164
		public void ExtractWithPassword(Stream stream, string password)
		{
			this.InternalExtract(null, stream, password);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00005F74 File Offset: 0x00004174
		public CrcCalculatorStream OpenReader()
		{
			return this.InternalOpenReader(this._Password ?? this._zipfile._Password);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00005FAC File Offset: 0x000041AC
		public CrcCalculatorStream OpenReader(string password)
		{
			return this.InternalOpenReader(password);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00005FCC File Offset: 0x000041CC
		private CrcCalculatorStream InternalOpenReader(string password)
		{
			this.ValidateCompression();
			this.ValidateEncryption();
			this.SetupCrypto(password);
			Stream archiveStream = this.ArchiveStream;
			this.ArchiveStream.Seek(this.FileDataPosition, SeekOrigin.Begin);
			Stream stream = archiveStream;
			if (this.Encryption == EncryptionAlgorithm.PkzipWeak)
			{
				stream = new ZipCipherStream(archiveStream, this._zipCrypto, CryptoMode.Decrypt);
			}
			else if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				stream = new WinZipAesCipherStream(archiveStream, this._aesCrypto, this._CompressedFileDataSize, CryptoMode.Decrypt);
			}
			return new CrcCalculatorStream((this.CompressionMethod == 8) ? new DeflateStream(stream, CompressionMode.Decompress, true) : stream, this._UncompressedSize);
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000CA RID: 202 RVA: 0x000060A0 File Offset: 0x000042A0
		internal Stream ArchiveStream
		{
			get
			{
				if (this._archiveStream == null)
				{
					if (this._zipfile != null)
					{
						this._zipfile.Reset();
						this._archiveStream = this._zipfile.ReadStream;
					}
				}
				return this._archiveStream;
			}
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00006104 File Offset: 0x00004304
		private void OnExtractProgress(long bytesWritten, long totalBytesToWrite)
		{
			this._ioOperationCanceled = this._zipfile.OnExtractBlock(this, bytesWritten, totalBytesToWrite);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x0000611C File Offset: 0x0000431C
		private void OnBeforeExtract(string path)
		{
			if (!this._zipfile._inExtractAll)
			{
				this._ioOperationCanceled = this._zipfile.OnSingleEntryExtract(this, path, true, this.OverwriteOnExtract);
			}
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000615C File Offset: 0x0000435C
		private void OnAfterExtract(string path)
		{
			if (!this._zipfile._inExtractAll)
			{
				this._zipfile.OnSingleEntryExtract(this, path, false, this.OverwriteOnExtract);
			}
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00006198 File Offset: 0x00004398
		private void OnWriteBlock(long bytesXferred, long totalBytesToXfer)
		{
			this._ioOperationCanceled = this._zipfile.OnSaveBlock(this, bytesXferred, totalBytesToXfer);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x000061B0 File Offset: 0x000043B0
		private void InternalExtract(string baseDir, Stream outstream, string password)
		{
			this.OnBeforeExtract(baseDir);
			this._ioOperationCanceled = false;
			string text = null;
			Stream stream = null;
			bool flag = false;
			try
			{
				this.ValidateCompression();
				this.ValidateEncryption();
				if (this.ValidateOutput(baseDir, outstream, out text))
				{
					this.OnAfterExtract(baseDir);
				}
				else
				{
					this.SetupCrypto(password ?? (this._Password ?? this._zipfile._Password));
					if (text != null)
					{
						if (!Directory.Exists(Path.GetDirectoryName(text)))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(text));
						}
						if (File.Exists(text))
						{
							flag = true;
							if (!this.OverwriteOnExtract)
							{
								throw new ZipException("The file already exists.");
							}
							File.Delete(text);
						}
						stream = new FileStream(text, FileMode.CreateNew);
					}
					else
					{
						stream = outstream;
					}
					if (this._ioOperationCanceled)
					{
						try
						{
							if (text != null)
							{
								if (stream != null)
								{
									stream.Close();
								}
								if (File.Exists(text))
								{
									File.Delete(text);
								}
							}
						}
						finally
						{
						}
					}
					int num = this._ExtractOne(stream);
					if (this._ioOperationCanceled)
					{
						try
						{
							if (text != null)
							{
								if (stream != null)
								{
									stream.Close();
								}
								if (File.Exists(text))
								{
									File.Delete(text);
								}
							}
						}
						finally
						{
						}
					}
					if (num != this._Crc32)
					{
						if ((this.Encryption != EncryptionAlgorithm.WinZipAes128 && this.Encryption != EncryptionAlgorithm.WinZipAes256) || this._WinZipAesMethod != 2)
						{
							throw new BadCrcException("CRC error: the file being extracted appears to be corrupted. " + string.Format("Expected 0x{0:X8}, Actual 0x{1:X8}", this._Crc32, num));
						}
					}
					if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
					{
						this._aesCrypto.ReadAndVerifyMac(this.ArchiveStream);
					}
					if (text != null)
					{
						stream.Close();
						stream = null;
						DateTime lastWriteTime = this.LastModified;
						if (DateTime.Now.IsDaylightSavingTime() && !this.LastModified.IsDaylightSavingTime())
						{
							lastWriteTime = this.LastModified - new TimeSpan(1, 0, 0);
						}
						File.SetLastWriteTime(text, lastWriteTime);
					}
					this.OnAfterExtract(baseDir);
				}
			}
			catch
			{
				try
				{
					if (text != null)
					{
						if (stream != null)
						{
							stream.Close();
						}
						if (File.Exists(text))
						{
							if (!flag || this.OverwriteOnExtract)
							{
								File.Delete(text);
							}
						}
					}
				}
				finally
				{
				}
				throw;
			}
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00006538 File Offset: 0x00004738
		private void ValidateEncryption()
		{
			if (this.Encryption != EncryptionAlgorithm.PkzipWeak && this.Encryption != EncryptionAlgorithm.WinZipAes128 && this.Encryption != EncryptionAlgorithm.WinZipAes256 && this.Encryption != EncryptionAlgorithm.None)
			{
				throw new ArgumentException(string.Format("Unsupported Encryption algorithm ({0:X2})", this.Encryption));
			}
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x000065A0 File Offset: 0x000047A0
		private void ValidateCompression()
		{
			if (this.CompressionMethod != 0 && this.CompressionMethod != 8)
			{
				throw new ArgumentException(string.Format("Unsupported Compression method (0x{0:X2})", this.CompressionMethod));
			}
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x000065F0 File Offset: 0x000047F0
		private void SetupCrypto(string password)
		{
			if (password != null)
			{
				if (this.Encryption == EncryptionAlgorithm.PkzipWeak)
				{
					this.ArchiveStream.Seek(this.FileDataPosition - 12L, SeekOrigin.Begin);
					this._zipCrypto = ZipCrypto.ForRead(password, this);
				}
				else if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
				{
					if (this._aesCrypto != null)
					{
						this._aesCrypto.Password = password;
					}
					else
					{
						int num = (int)(this._KeyStrengthInBits / 8 / 2 + 2);
						this.ArchiveStream.Seek(this.FileDataPosition - (long)num, SeekOrigin.Begin);
						this._aesCrypto = WinZipAesCrypto.ReadFromStream(password, (int)this._KeyStrengthInBits, this.ArchiveStream);
					}
				}
			}
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x000066DC File Offset: 0x000048DC
		private bool ValidateOutput(string basedir, Stream outstream, out string OutputFile)
		{
			bool result;
			if (basedir != null)
			{
				OutputFile = (this.FileName.StartsWith("/") ? Path.Combine(basedir, this.FileName.Substring(1)) : Path.Combine(basedir, this.FileName));
				if (this.IsDirectory || this.FileName.EndsWith("/"))
				{
					if (!Directory.Exists(OutputFile))
					{
						Directory.CreateDirectory(OutputFile);
					}
					result = true;
				}
				else
				{
					result = false;
				}
			}
			else
			{
				if (outstream == null)
				{
					throw new ZipException("Cannot extract.", new ArgumentException("Invalid input.", "outstream | basedir"));
				}
				OutputFile = null;
				result = (this.IsDirectory || this.FileName.EndsWith("/"));
			}
			return result;
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x000067E8 File Offset: 0x000049E8
		private void _CheckRead(int nbytes)
		{
			if (nbytes == 0)
			{
				throw new BadReadException(string.Format("bad read of entry {0} from compressed archive.", this.FileName));
			}
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00006820 File Offset: 0x00004A20
		private int _ExtractOne(Stream output)
		{
			Stream archiveStream = this.ArchiveStream;
			archiveStream.Seek(this.FileDataPosition, SeekOrigin.Begin);
			int result = 0;
			byte[] array = new byte[17408];
			long num = (this.CompressionMethod == 8) ? this.UncompressedSize : this._CompressedFileDataSize;
			Stream stream;
			if (this.Encryption == EncryptionAlgorithm.PkzipWeak)
			{
				stream = new ZipCipherStream(archiveStream, this._zipCrypto, CryptoMode.Decrypt);
			}
			else if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				stream = new WinZipAesCipherStream(archiveStream, this._aesCrypto, this._CompressedFileDataSize, CryptoMode.Decrypt);
			}
			else
			{
				stream = new CrcCalculatorStream(archiveStream, this._CompressedFileDataSize);
			}
			Stream stream2 = (this.CompressionMethod == 8) ? new DeflateStream(stream, CompressionMode.Decompress, true) : stream;
			long num2 = 0L;
			using (CrcCalculatorStream crcCalculatorStream = new CrcCalculatorStream(stream2))
			{
				while (num > 0L)
				{
					int count = (num > (long)array.Length) ? array.Length : ((int)num);
					int num3 = crcCalculatorStream.Read(array, 0, count);
					this._CheckRead(num3);
					output.Write(array, 0, num3);
					num -= (long)num3;
					num2 += (long)num3;
					this.OnExtractProgress(num2, this.UncompressedSize);
					if (this._ioOperationCanceled)
					{
						break;
					}
				}
				result = crcCalculatorStream.Crc32;
				if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
				{
					WinZipAesCipherStream winZipAesCipherStream = stream as WinZipAesCipherStream;
					this._aesCrypto.CalculatedMac = winZipAesCipherStream.FinalAuthentication;
				}
			}
			return result;
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00006A18 File Offset: 0x00004C18
		internal void MarkAsDirectory()
		{
			this._IsDirectory = true;
			if (!this._FileNameInArchive.EndsWith("/"))
			{
				this._FileNameInArchive += "/";
			}
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00006A60 File Offset: 0x00004C60
		internal void WriteCentralDirectoryEntry(Stream s)
		{
			byte[] array = new byte[4096];
			int num = 0;
			array[num++] = 80;
			array[num++] = 75;
			array[num++] = 1;
			array[num++] = 2;
			array[num++] = (byte)(this._VersionMadeBy & 255);
			array[num++] = (byte)(((int)this._VersionMadeBy & 65280) >> 8);
			short num2 = this._OutputUsesZip64.Value ? (short)45 : (short)20;
			array[num++] = (byte)(num2 & 255);
			array[num++] = (byte)(((int)num2 & 65280) >> 8);
			array[num++] = (byte)(this._BitField & 255);
			array[num++] = (byte)(((int)this._BitField & 65280) >> 8);
			array[num++] = (byte)(this.CompressionMethod & 255);
			array[num++] = (byte)(((int)this.CompressionMethod & 65280) >> 8);
			if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				num -= 2;
				array[num++] = 99;
				array[num++] = 0;
			}
			array[num++] = (byte)(this._TimeBlob & 255);
			array[num++] = (byte)((this._TimeBlob & 65280) >> 8);
			array[num++] = (byte)((this._TimeBlob & 16711680) >> 16);
			array[num++] = (byte)(((long)this._TimeBlob & (long)(-16777216)) >> 24);
			array[num++] = (byte)(this._Crc32 & 255);
			array[num++] = (byte)((this._Crc32 & 65280) >> 8);
			array[num++] = (byte)((this._Crc32 & 16711680) >> 16);
			array[num++] = (byte)(((long)this._Crc32 & (long)(-16777216)) >> 24);
			int i;
			if (this._OutputUsesZip64.Value)
			{
				for (i = 0; i < 8; i++)
				{
					array[num++] = byte.MaxValue;
				}
			}
			else
			{
				array[num++] = (byte)(this._CompressedSize & 255L);
				array[num++] = (byte)((this._CompressedSize & 65280L) >> 8);
				array[num++] = (byte)((this._CompressedSize & 16711680L) >> 16);
				array[num++] = (byte)((this._CompressedSize & (long)(-16777216)) >> 24);
				array[num++] = (byte)(this._UncompressedSize & 255L);
				array[num++] = (byte)((this._UncompressedSize & 65280L) >> 8);
				array[num++] = (byte)((this._UncompressedSize & 16711680L) >> 16);
				array[num++] = (byte)((this._UncompressedSize & (long)(-16777216)) >> 24);
			}
			byte[] array2 = this._GetEncodedFileNameBytes();
			short num3 = (short)array2.Length;
			array[num++] = (byte)(num3 & 255);
			array[num++] = (byte)(((int)num3 & 65280) >> 8);
			this._presumeZip64 = this._OutputUsesZip64.Value;
			this._Extra = this.ConsExtraField();
			short num4 = (short)((this._Extra == null) ? 0 : this._Extra.Length);
			array[num++] = (byte)(num4 & 255);
			array[num++] = (byte)(((int)num4 & 65280) >> 8);
			int num5 = (this._CommentBytes == null) ? 0 : this._CommentBytes.Length;
			if (num5 + num > array.Length)
			{
				num5 = array.Length - num;
			}
			array[num++] = (byte)(num5 & 255);
			array[num++] = (byte)((num5 & 65280) >> 8);
			array[num++] = 0;
			array[num++] = 0;
			array[num++] = 0;
			array[num++] = 0;
			array[num++] = (this.IsDirectory ? (byte)16 : (byte)32);
			array[num++] = 0;
			array[num++] = 182;
			array[num++] = 129;
			if (this._OutputUsesZip64.Value)
			{
				for (i = 0; i < 4; i++)
				{
					array[num++] = byte.MaxValue;
				}
			}
			else
			{
				array[num++] = (byte)(this._RelativeOffsetOfLocalHeader & 255L);
				array[num++] = (byte)((this._RelativeOffsetOfLocalHeader & 65280L) >> 8);
				array[num++] = (byte)((this._RelativeOffsetOfLocalHeader & 16711680L) >> 16);
				array[num++] = (byte)((this._RelativeOffsetOfLocalHeader & (long)(-16777216)) >> 24);
			}
			for (i = 0; i < (int)num3; i++)
			{
				array[num + i] = array2[i];
			}
			num += i;
			if (this._Extra != null)
			{
				for (i = 0; i < (int)num4; i++)
				{
					array[num + i] = this._Extra[i];
				}
				num += i;
			}
			if (num5 != 0)
			{
				i = 0;
				while (i < num5 && num + i < array.Length)
				{
					array[num + i] = this._CommentBytes[i];
					i++;
				}
				num += i;
			}
			s.Write(array, 0, num);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00006FE0 File Offset: 0x000051E0
		private byte[] ConsExtraField()
		{
			byte[] array = null;
			byte[] array2 = null;
			if (this._zipfile._zip64 != Zip64Option.Default)
			{
				array = new byte[32];
				int num = 0;
				if (this._presumeZip64)
				{
					array[num++] = 1;
					array[num++] = 0;
				}
				else
				{
					array[num++] = 153;
					array[num++] = 153;
				}
				array[num++] = 28;
				array[num++] = 0;
				Array.Copy(BitConverter.GetBytes(this._UncompressedSize), 0, array, num, 8);
				num += 8;
				Array.Copy(BitConverter.GetBytes(this._CompressedSize), 0, array, num, 8);
				num += 8;
				Array.Copy(BitConverter.GetBytes(this._RelativeOffsetOfLocalHeader), 0, array, num, 8);
				num += 8;
				Array.Copy(BitConverter.GetBytes(0), 0, array, num, 4);
			}
			if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				array2 = new byte[11];
				int num = 0;
				array2[num++] = 1;
				array2[num++] = 153;
				array2[num++] = 7;
				array2[num++] = 0;
				array2[num++] = 1;
				array2[num++] = 0;
				array2[num++] = 65;
				array2[num++] = 69;
				array2[num] = byte.MaxValue;
				if (this._KeyStrengthInBits == 128)
				{
					array2[num] = 1;
				}
				if (this._KeyStrengthInBits == 256)
				{
					array2[num] = 3;
				}
				num++;
				array2[num++] = (byte)(this._CompressionMethod & 255);
				array2[num++] = (byte)((int)this._CompressionMethod & 65280);
			}
			byte[] array3 = null;
			int num2 = 0;
			if (array != null)
			{
				num2 += array.Length;
			}
			if (array2 != null)
			{
				num2 += array2.Length;
			}
			if (num2 > 0)
			{
				array3 = new byte[num2];
				int num3 = 0;
				if (array != null)
				{
					Array.Copy(array, 0, array3, num3, array.Length);
					num3 += array.Length;
				}
				if (array2 != null)
				{
					Array.Copy(array2, 0, array3, num3, array2.Length);
					num3 += array2.Length;
				}
			}
			return array3;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00007244 File Offset: 0x00005444
		private Encoding GenerateCommentBytes()
		{
			this._CommentBytes = ZipEntry.ibm437.GetBytes(this._Comment);
			string @string = ZipEntry.ibm437.GetString(this._CommentBytes, 0, this._CommentBytes.Length);
			Encoding provisionalAlternateEncoding;
			if (@string == this._Comment)
			{
				provisionalAlternateEncoding = ZipEntry.ibm437;
			}
			else
			{
				this._CommentBytes = this._provisionalAlternateEncoding.GetBytes(this._Comment);
				provisionalAlternateEncoding = this._provisionalAlternateEncoding;
			}
			return provisionalAlternateEncoding;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x000072CC File Offset: 0x000054CC
		private byte[] _GetEncodedFileNameBytes()
		{
			string text = this.FileName.Replace("\\", "/");
			string text2;
			if (this._TrimVolumeFromFullyQualifiedPaths && this.FileName.Length >= 3 && this.FileName[1] == ':' && text[2] == '/')
			{
				text2 = text.Substring(3);
			}
			else if (this.FileName.Length >= 4 && text[0] == '/' && text[1] == '/')
			{
				int num = text.IndexOf('/', 2);
				if (num == -1)
				{
					throw new ArgumentException("The path for that entry appears to be badly formatted");
				}
				text2 = text.Substring(num + 1);
			}
			else if (this.FileName.Length >= 3 && text[0] == '.' && text[1] == '/')
			{
				text2 = text.Substring(2);
			}
			else
			{
				text2 = text;
			}
			byte[] bytes = ZipEntry.ibm437.GetBytes(text2);
			string @string = ZipEntry.ibm437.GetString(bytes, 0, bytes.Length);
			this._CommentBytes = null;
			byte[] result;
			if (@string == text2)
			{
				if (this._Comment == null || this._Comment.Length == 0)
				{
					this._actualEncoding = ZipEntry.ibm437;
					result = bytes;
				}
				else
				{
					Encoding encoding = this.GenerateCommentBytes();
					if (encoding.CodePage == 437)
					{
						this._actualEncoding = ZipEntry.ibm437;
						result = bytes;
					}
					else
					{
						this._actualEncoding = encoding;
						bytes = encoding.GetBytes(text2);
						result = bytes;
					}
				}
			}
			else
			{
				bytes = this._provisionalAlternateEncoding.GetBytes(text2);
				if (this._Comment != null && this._Comment.Length != 0)
				{
					this._CommentBytes = this._provisionalAlternateEncoding.GetBytes(this._Comment);
				}
				this._actualEncoding = this._provisionalAlternateEncoding;
				result = bytes;
			}
			return result;
		}

		// Token: 0x060000DB RID: 219 RVA: 0x00007544 File Offset: 0x00005744
		private bool WantReadAgain()
		{
			return this._CompressionMethod != 0 && this._CompressedSize >= this._UncompressedSize && !this.ForceNoCompression && (this.WillReadTwiceOnInflation == null || this.WillReadTwiceOnInflation(this._UncompressedSize, this._CompressedSize, this.FileName));
		}

		// Token: 0x060000DC RID: 220 RVA: 0x000075E4 File Offset: 0x000057E4
		private bool SeemsCompressible(string filename)
		{
			string pattern = "(?i)^(.+)\\.(mp3|png|docx|xlsx|jpg|zip)$";
			return !Regex.IsMatch(filename, pattern);
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00007620 File Offset: 0x00005820
		private bool DefaultWantCompression()
		{
			bool result;
			if (this._LocalFileName != null)
			{
				result = this.SeemsCompressible(this._LocalFileName);
			}
			else
			{
				result = (this._FileNameInArchive == null || this.SeemsCompressible(this._FileNameInArchive));
			}
			return result;
		}

		// Token: 0x060000DE RID: 222 RVA: 0x00007680 File Offset: 0x00005880
		private void FigureCompressionMethodForWriting(int cycle)
		{
			if (cycle > 1)
			{
				this._CompressionMethod = 0;
			}
			else if (this.IsDirectory)
			{
				this._CompressionMethod = 0;
			}
			else if (this.__FileDataPosition == -1L)
			{
				if (this._Source == EntrySource.Stream)
				{
					if (this._sourceStream != null && this._sourceStream.CanSeek)
					{
						long length = this._sourceStream.Length;
						if (length == 0L)
						{
							this._CompressionMethod = 0;
						}
					}
				}
				else
				{
					FileInfo fileInfo = new FileInfo(this.LocalFileName);
					long length = fileInfo.Length;
					if (length == 0L)
					{
						this._CompressionMethod = 0;
					}
				}
				if (this._ForceNoCompression)
				{
					this._CompressionMethod = 0;
				}
				else if (this.WantCompression != null)
				{
					this._CompressionMethod = (this.WantCompression(this.LocalFileName, this._FileNameInArchive) ? (short)8 : (short)0);
				}
				else
				{
					this._CompressionMethod = (this.DefaultWantCompression() ? (short)8 : (short)0);
				}
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x000077F0 File Offset: 0x000059F0
		private void WriteHeader(Stream s, int cycle)
		{
			CountingStream countingStream = s as CountingStream;
			this._RelativeOffsetOfLocalHeader = ((countingStream != null) ? countingStream.BytesWritten : s.Position);
			byte[] array = new byte[512];
			int num = 0;
			array[num++] = 80;
			array[num++] = 75;
			array[num++] = 3;
			array[num++] = 4;
			if (this._zipfile._zip64 == Zip64Option.Default && (uint)this._RelativeOffsetOfLocalHeader >= 4294967295U)
			{
				throw new ZipException("Offset within the zip archive exceeds 0xFFFFFFFF. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");
			}
			this._presumeZip64 = (this._zipfile._zip64 == Zip64Option.Always || (this._zipfile._zip64 == Zip64Option.AsNecessary && !s.CanSeek));
			short num2 = this._presumeZip64 ? (short)45 : (short)20;
			array[num++] = (byte)(num2 & 255);
			array[num++] = (byte)(((int)num2 & 65280) >> 8);
			byte[] array2 = this._GetEncodedFileNameBytes();
			short num3 = (short)array2.Length;
			this._BitField = (this.UsesEncryption ? (short)1 : (short)0);
			if (this.UsesEncryption && ZipEntry.IsStrong(this.Encryption))
			{
				this._BitField |= 32;
			}
			if (this.ActualEncoding.CodePage == Encoding.UTF8.CodePage)
			{
				this._BitField |= 2048;
			}
			if (!s.CanSeek || this._presumeZip64)
			{
				this._BitField |= 8;
			}
			array[num++] = (byte)(this._BitField & 255);
			array[num++] = (byte)(((int)this._BitField & 65280) >> 8);
			if (this.__FileDataPosition == -1L)
			{
				this._UncompressedSize = 0L;
				this._CompressedSize = 0L;
				this._Crc32 = 0;
				this._crcCalculated = false;
			}
			this.FigureCompressionMethodForWriting(cycle);
			array[num++] = (byte)(this.CompressionMethod & 255);
			array[num++] = (byte)(((int)this.CompressionMethod & 65280) >> 8);
			if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				num -= 2;
				array[num++] = 99;
				array[num++] = 0;
			}
			this._TimeBlob = SharedUtilities.DateTimeToPacked(this.LastModified);
			array[num++] = (byte)(this._TimeBlob & 255);
			array[num++] = (byte)((this._TimeBlob & 65280) >> 8);
			array[num++] = (byte)((this._TimeBlob & 16711680) >> 16);
			array[num++] = (byte)(((long)this._TimeBlob & (long)(-16777216)) >> 24);
			array[num++] = (byte)(this._Crc32 & 255);
			array[num++] = (byte)((this._Crc32 & 65280) >> 8);
			array[num++] = (byte)((this._Crc32 & 16711680) >> 16);
			array[num++] = (byte)(((long)this._Crc32 & (long)(-16777216)) >> 24);
			int i;
			if (this._presumeZip64)
			{
				for (i = 0; i < 8; i++)
				{
					array[num++] = byte.MaxValue;
				}
			}
			else
			{
				array[num++] = (byte)(this._CompressedSize & 255L);
				array[num++] = (byte)((this._CompressedSize & 65280L) >> 8);
				array[num++] = (byte)((this._CompressedSize & 16711680L) >> 16);
				array[num++] = (byte)((this._CompressedSize & (long)(-16777216)) >> 24);
				array[num++] = (byte)(this._UncompressedSize & 255L);
				array[num++] = (byte)((this._UncompressedSize & 65280L) >> 8);
				array[num++] = (byte)((this._UncompressedSize & 16711680L) >> 16);
				array[num++] = (byte)((this._UncompressedSize & (long)(-16777216)) >> 24);
			}
			array[num++] = (byte)(num3 & 255);
			array[num++] = (byte)(((int)num3 & 65280) >> 8);
			this._Extra = this.ConsExtraField();
			short num4 = (short)((this._Extra == null) ? 0 : this._Extra.Length);
			array[num++] = (byte)(num4 & 255);
			array[num++] = (byte)(((int)num4 & 65280) >> 8);
			i = 0;
			while (i < array2.Length && num + i < array.Length)
			{
				array[num + i] = array2[i];
				i++;
			}
			num += i;
			if (this._Extra != null)
			{
				for (i = 0; i < this._Extra.Length; i++)
				{
					array[num + i] = this._Extra[i];
				}
				num += i;
			}
			this._LengthOfHeader = num;
			s.Write(array, 0, num);
			this._EntryHeader = new byte[num];
			for (i = 0; i < num; i++)
			{
				this._EntryHeader[i] = array[i];
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x00007D74 File Offset: 0x00005F74
		private int FigureCrc32()
		{
			if (!this._crcCalculated)
			{
				Stream stream;
				if (this._sourceStream != null)
				{
					this._sourceStream.Position = 0L;
					stream = this._sourceStream;
				}
				else
				{
					stream = File.Open(this.LocalFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				}
				CRC32 crc = new CRC32();
				this._Crc32 = crc.GetCrc32(stream);
				if (this._sourceStream == null)
				{
					stream.Close();
					stream.Dispose();
				}
				this._crcCalculated = true;
			}
			return this._Crc32;
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00007E20 File Offset: 0x00006020
		internal void CopyMetaData(ZipEntry source)
		{
			this.__FileDataPosition = source.__FileDataPosition;
			this.CompressionMethod = source.CompressionMethod;
			this._CompressedFileDataSize = source._CompressedFileDataSize;
			this._UncompressedSize = source._UncompressedSize;
			this._BitField = source._BitField;
			this._LastModified = source._LastModified;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00007E7C File Offset: 0x0000607C
		private void _WriteFileData(Stream s)
		{
			Stream stream = null;
			CrcCalculatorStream crcCalculatorStream = null;
			CountingStream countingStream = null;
			try
			{
				this.__FileDataPosition = s.Position;
			}
			catch
			{
			}
			try
			{
				if (this._sourceStream != null)
				{
					this._sourceStream.Position = 0L;
					stream = this._sourceStream;
				}
				else
				{
					FileShare fileShare = FileShare.ReadWrite;
					fileShare |= FileShare.Delete;
					stream = File.Open(this.LocalFileName, FileMode.Open, FileAccess.Read, fileShare);
				}
				long totalBytesToXfer = 0L;
				if (this._sourceStream == null)
				{
					FileInfo fileInfo = new FileInfo(this.LocalFileName);
					totalBytesToXfer = fileInfo.Length;
				}
				crcCalculatorStream = new CrcCalculatorStream(stream);
				countingStream = new CountingStream(s);
				Stream stream2 = countingStream;
				if (this.Encryption == EncryptionAlgorithm.PkzipWeak)
				{
					stream2 = new ZipCipherStream(countingStream, this._zipCrypto, CryptoMode.Encrypt);
				}
				else if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
				{
					stream2 = new WinZipAesCipherStream(countingStream, this._aesCrypto, CryptoMode.Encrypt);
				}
				bool flag = false;
				Stream stream3;
				if (this.CompressionMethod == 8)
				{
					stream3 = new DeflateStream(stream2, CompressionMode.Compress, this._zipfile.CompressionLevel, true);
					flag = true;
				}
				else
				{
					stream3 = stream2;
				}
				byte[] buffer = new byte[17408];
				for (int i = crcCalculatorStream.Read(buffer, 0, 17408); i > 0; i = crcCalculatorStream.Read(buffer, 0, 17408))
				{
					stream3.Write(buffer, 0, i);
					this.OnWriteBlock(crcCalculatorStream.TotalBytesSlurped, totalBytesToXfer);
					if (this._ioOperationCanceled)
					{
						break;
					}
				}
				if (flag)
				{
					stream3.Close();
				}
				stream2.Flush();
				stream2.Close();
				this._LengthOfTrailer = 0;
				WinZipAesCipherStream winZipAesCipherStream = stream2 as WinZipAesCipherStream;
				if (winZipAesCipherStream != null)
				{
					s.Write(winZipAesCipherStream.FinalAuthentication, 0, 10);
					this._LengthOfTrailer += 10;
				}
			}
			finally
			{
				if (this._sourceStream == null && stream != null)
				{
					stream.Close();
					stream.Dispose();
				}
			}
			if (!this._ioOperationCanceled)
			{
				this._UncompressedSize = crcCalculatorStream.TotalBytesSlurped;
				this._CompressedSize = countingStream.BytesWritten;
				this._Crc32 = crcCalculatorStream.Crc32;
				if (this._Password != null)
				{
					if (this.Encryption == EncryptionAlgorithm.PkzipWeak)
					{
						this._CompressedSize += 12L;
					}
					else if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
					{
						this._CompressedSize += (long)this._aesCrypto.SizeOfEncryptionMetadata;
					}
				}
				int num = 8;
				this._EntryHeader[num++] = (byte)(this.CompressionMethod & 255);
				this._EntryHeader[num++] = (byte)(((int)this.CompressionMethod & 65280) >> 8);
				num = 14;
				this._EntryHeader[num++] = (byte)(this._Crc32 & 255);
				this._EntryHeader[num++] = (byte)((this._Crc32 & 65280) >> 8);
				this._EntryHeader[num++] = (byte)((this._Crc32 & 16711680) >> 16);
				this._EntryHeader[num++] = (byte)(((long)this._Crc32 & (long)(-16777216)) >> 24);
				this._entryRequiresZip64 = new bool?(this._CompressedSize >= (long)(-1) || this._UncompressedSize >= (long)(-1) || this._RelativeOffsetOfLocalHeader >= (long)(-1));
				if (this._zipfile._zip64 == Zip64Option.Default && this._entryRequiresZip64.Value)
				{
					throw new ZipException("Compressed or Uncompressed size, or offset exceeds the maximum value. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");
				}
				this._OutputUsesZip64 = new bool?(this._zipfile._zip64 == Zip64Option.Always || this._entryRequiresZip64.Value);
				short num2 = (short)((int)this._EntryHeader[26] + (int)this._EntryHeader[27] * 256);
				short num3 = (short)((int)this._EntryHeader[28] + (int)this._EntryHeader[29] * 256);
				if (this._OutputUsesZip64.Value)
				{
					this._EntryHeader[4] = 45;
					this._EntryHeader[5] = 0;
					for (int j = 0; j < 8; j++)
					{
						this._EntryHeader[num++] = byte.MaxValue;
					}
					num = (int)(30 + num2);
					this._EntryHeader[num++] = 1;
					this._EntryHeader[num++] = 0;
					num += 2;
					Array.Copy(BitConverter.GetBytes(this._UncompressedSize), 0, this._EntryHeader, num, 8);
					num += 8;
					Array.Copy(BitConverter.GetBytes(this._CompressedSize), 0, this._EntryHeader, num, 8);
					num += 8;
					Array.Copy(BitConverter.GetBytes(this._RelativeOffsetOfLocalHeader), 0, this._EntryHeader, num, 8);
				}
				else
				{
					this._EntryHeader[4] = 20;
					this._EntryHeader[5] = 0;
					num = 18;
					this._EntryHeader[num++] = (byte)(this._CompressedSize & 255L);
					this._EntryHeader[num++] = (byte)((this._CompressedSize & 65280L) >> 8);
					this._EntryHeader[num++] = (byte)((this._CompressedSize & 16711680L) >> 16);
					this._EntryHeader[num++] = (byte)((this._CompressedSize & (long)(-16777216)) >> 24);
					this._EntryHeader[num++] = (byte)(this._UncompressedSize & 255L);
					this._EntryHeader[num++] = (byte)((this._UncompressedSize & 65280L) >> 8);
					this._EntryHeader[num++] = (byte)((this._UncompressedSize & 16711680L) >> 16);
					this._EntryHeader[num++] = (byte)((this._UncompressedSize & (long)(-16777216)) >> 24);
					if (num3 != 0)
					{
						num = (int)(30 + num2);
						short num4 = (short)((int)this._EntryHeader[num + 2] + (int)this._EntryHeader[num + 3] * 256);
						if (num4 == 28)
						{
							this._EntryHeader[num++] = 153;
							this._EntryHeader[num++] = 153;
						}
					}
				}
				if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
				{
					num = 8;
					this._EntryHeader[num++] = 99;
					this._EntryHeader[num++] = 0;
					num = (int)(30 + num2);
					do
					{
						ushort num5 = (ushort)((int)this._EntryHeader[num] + (int)this._EntryHeader[num + 1] * 256);
						short num4 = (short)((int)this._EntryHeader[num + 2] + (int)this._EntryHeader[num + 3] * 256);
						if (num5 != 39169)
						{
							num += (int)(num4 + 4);
						}
						else
						{
							num += 9;
							this._EntryHeader[num++] = (byte)(this._CompressionMethod & 255);
							this._EntryHeader[num++] = (byte)((int)this._CompressionMethod & 65280);
						}
					}
					while (num < (int)(num3 - 30 - num2));
				}
				if ((this._BitField & 8) != 8)
				{
					s.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
					s.Write(this._EntryHeader, 0, this._EntryHeader.Length);
					CountingStream countingStream2 = s as CountingStream;
					if (countingStream2 != null)
					{
						countingStream2.Adjust((long)this._EntryHeader.Length);
					}
					s.Seek(this._CompressedSize, SeekOrigin.Current);
				}
				else
				{
					byte[] array;
					if (this._zipfile._zip64 == Zip64Option.Always || this._zipfile._zip64 == Zip64Option.AsNecessary)
					{
						array = new byte[24];
						num = 0;
						Array.Copy(BitConverter.GetBytes(134695760), 0, array, num, 4);
						num += 4;
						Array.Copy(BitConverter.GetBytes(this._Crc32), 0, array, num, 4);
						num += 4;
						Array.Copy(BitConverter.GetBytes(this._CompressedSize), 0, array, num, 8);
						num += 8;
						Array.Copy(BitConverter.GetBytes(this._UncompressedSize), 0, array, num, 8);
						num += 8;
					}
					else
					{
						array = new byte[16];
						num = 0;
						int num6 = 134695760;
						array[num++] = (byte)(num6 & 255);
						array[num++] = (byte)((num6 & 65280) >> 8);
						array[num++] = (byte)((num6 & 16711680) >> 16);
						array[num++] = (byte)(((long)num6 & (long)(-16777216)) >> 24);
						array[num++] = (byte)(this._Crc32 & 255);
						array[num++] = (byte)((this._Crc32 & 65280) >> 8);
						array[num++] = (byte)((this._Crc32 & 16711680) >> 16);
						array[num++] = (byte)(((long)this._Crc32 & (long)(-16777216)) >> 24);
						array[num++] = (byte)(this._CompressedSize & 255L);
						array[num++] = (byte)((this._CompressedSize & 65280L) >> 8);
						array[num++] = (byte)((this._CompressedSize & 16711680L) >> 16);
						array[num++] = (byte)((this._CompressedSize & (long)(-16777216)) >> 24);
						array[num++] = (byte)(this._UncompressedSize & 255L);
						array[num++] = (byte)((this._UncompressedSize & 65280L) >> 8);
						array[num++] = (byte)((this._UncompressedSize & 16711680L) >> 16);
						array[num++] = (byte)((this._UncompressedSize & (long)(-16777216)) >> 24);
					}
					s.Write(array, 0, array.Length);
					this._LengthOfTrailer += array.Length;
				}
			}
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x0000898C File Offset: 0x00006B8C
		internal void Write(Stream outstream)
		{
			if (this._Source == EntrySource.Zipfile && !this._restreamRequiredOnSave)
			{
				this.CopyThroughOneEntry(outstream);
			}
			else
			{
				bool flag = true;
				int num = 0;
				for (;;)
				{
					num++;
					this.WriteHeader(outstream, num);
					if (this.IsDirectory)
					{
						break;
					}
					this._EmitOne(outstream);
					if (flag)
					{
						flag = (num <= 1 && outstream.CanSeek && (this._aesCrypto == null || this.CompressedSize - (long)this._aesCrypto.SizeOfEncryptionMetadata > this.UncompressedSize) && (this._zipCrypto == null || this.CompressedSize - 12L > this.UncompressedSize) && this.WantReadAgain());
					}
					if (flag)
					{
						outstream.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
						outstream.SetLength(outstream.Position);
						CountingStream countingStream = outstream as CountingStream;
						if (countingStream != null)
						{
							countingStream.Adjust(this._TotalEntrySize);
						}
					}
					if (!flag)
					{
						return;
					}
				}
				this._entryRequiresZip64 = new bool?(this._RelativeOffsetOfLocalHeader >= (long)(-1));
				this._OutputUsesZip64 = new bool?(this._zipfile._zip64 == Zip64Option.Always || this._entryRequiresZip64.Value);
			}
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00008B38 File Offset: 0x00006D38
		private void _EmitOne(Stream outstream)
		{
			this._WriteSecurityMetadata(outstream);
			this._WriteFileData(outstream);
			this._TotalEntrySize = (long)this._LengthOfHeader + this._CompressedSize + (long)this._LengthOfTrailer;
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00008B68 File Offset: 0x00006D68
		private void _WriteSecurityMetadata(Stream outstream)
		{
			if (this._Password != null)
			{
				if (this.Encryption == EncryptionAlgorithm.PkzipWeak)
				{
					this._zipCrypto = ZipCrypto.ForWrite(this._Password);
					Random random = new Random();
					byte[] array = new byte[12];
					random.NextBytes(array);
					this.FigureCrc32();
					array[11] = (byte)(this._Crc32 >> 24 & 255);
					byte[] array2 = this._zipCrypto.EncryptMessage(array, array.Length);
					outstream.Write(array2, 0, array2.Length);
				}
				else if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
				{
					this._aesCrypto = WinZipAesCrypto.Generate(this._Password, (int)this._KeyStrengthInBits);
					outstream.Write(this._aesCrypto.Salt, 0, this._aesCrypto._Salt.Length);
					outstream.Write(this._aesCrypto.GeneratedPV, 0, this._aesCrypto.GeneratedPV.Length);
				}
			}
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00008C88 File Offset: 0x00006E88
		private void CopyThroughOneEntry(Stream outstream)
		{
			byte[] array = new byte[17408];
			Stream archiveStream = this.ArchiveStream;
			if (this._metadataChanged || (this._InputUsesZip64 && this._zipfile.UseZip64WhenSaving == Zip64Option.Default) || (!this._InputUsesZip64 && this._zipfile.UseZip64WhenSaving == Zip64Option.Always))
			{
				long relativeOffsetOfLocalHeader = this._RelativeOffsetOfLocalHeader;
				if (this.LengthOfHeader == 0)
				{
					throw new ZipException("Bad header length.");
				}
				int lengthOfHeader = this.LengthOfHeader;
				this.WriteHeader(outstream, 0);
				if (!this.FileName.EndsWith("/"))
				{
					archiveStream.Seek(relativeOffsetOfLocalHeader + (long)lengthOfHeader, SeekOrigin.Begin);
					int num2;
					for (long num = this._CompressedSize; num > 0L; num -= (long)num2)
					{
						int count = (num > (long)array.Length) ? array.Length : ((int)num);
						num2 = archiveStream.Read(array, 0, count);
						this._CheckRead(num2);
						outstream.Write(array, 0, num2);
					}
					this._LengthOfTrailer = 0;
					if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
					{
						byte[] buffer = new byte[10];
						archiveStream.Read(buffer, 0, 10);
						outstream.Write(buffer, 0, 10);
						this._LengthOfTrailer += 10;
					}
					if ((this._BitField & 8) == 8)
					{
						int num3 = 16;
						if (this._InputUsesZip64)
						{
							num3 += 8;
						}
						byte[] buffer2 = new byte[num3];
						archiveStream.Read(buffer2, 0, num3);
						if (this._InputUsesZip64 && this._zipfile.UseZip64WhenSaving == Zip64Option.Default)
						{
							outstream.Write(buffer2, 0, 8);
							outstream.Write(buffer2, 8, 4);
							outstream.Write(buffer2, 16, 4);
							this._LengthOfTrailer += 16;
						}
						else if (!this._InputUsesZip64 && this._zipfile.UseZip64WhenSaving == Zip64Option.Always)
						{
							byte[] buffer3 = new byte[4];
							outstream.Write(buffer2, 0, 8);
							outstream.Write(buffer2, 8, 4);
							outstream.Write(buffer3, 0, 4);
							outstream.Write(buffer2, 12, 4);
							outstream.Write(buffer3, 0, 4);
							this._LengthOfTrailer += 24;
						}
						else
						{
							outstream.Write(buffer2, 0, num3);
							this._LengthOfTrailer += num3;
						}
					}
				}
				this._TotalEntrySize = (long)this._LengthOfHeader + this._CompressedSize + (long)this._LengthOfTrailer;
			}
			else
			{
				if (this.LengthOfHeader == 0)
				{
					throw new ZipException("Bad header length.");
				}
				long relativeOffsetOfLocalHeader = this._RelativeOffsetOfLocalHeader;
				archiveStream.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
				if (this._TotalEntrySize == 0L)
				{
					this._TotalEntrySize = (long)this._LengthOfHeader + this._CompressedSize + (long)this._LengthOfTrailer;
				}
				CountingStream countingStream = outstream as CountingStream;
				this._RelativeOffsetOfLocalHeader = ((countingStream != null) ? countingStream.BytesWritten : outstream.Position);
				int num2;
				for (long num = this._TotalEntrySize; num > 0L; num -= (long)num2)
				{
					int count = (num > (long)array.Length) ? array.Length : ((int)num);
					num2 = archiveStream.Read(array, 0, count);
					this._CheckRead(num2);
					outstream.Write(array, 0, num2);
				}
			}
			this._entryRequiresZip64 = new bool?(this._CompressedSize >= (long)(-1) || this._UncompressedSize >= (long)(-1) || this._RelativeOffsetOfLocalHeader >= (long)(-1));
			this._OutputUsesZip64 = new bool?(this._zipfile._zip64 == Zip64Option.Always || this._entryRequiresZip64.Value);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x000090C8 File Offset: 0x000072C8
		internal static bool IsStrong(EncryptionAlgorithm e)
		{
			return e != EncryptionAlgorithm.None && e != EncryptionAlgorithm.PkzipWeak;
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x000090F4 File Offset: 0x000072F4
		internal int ProcessExtraField(short extraFieldLength)
		{
			int num = 0;
			Stream archiveStream = this.ArchiveStream;
			if (extraFieldLength > 0)
			{
				byte[] array = this._Extra = new byte[(int)extraFieldLength];
				num = archiveStream.Read(array, 0, array.Length);
				int num2;
				short num4;
				for (int i = 0; i < array.Length; i = num2 + (int)num4 + 4)
				{
					num2 = i;
					ushort num3 = (ushort)((int)array[i] + (int)array[i + 1] * 256);
					num4 = (short)((int)array[i + 2] + (int)array[i + 3] * 256);
					i += 4;
					ushort num5 = num3;
					if (num5 != 1)
					{
						if (num5 == 39169)
						{
							if (this._CompressionMethod == 99)
							{
								if ((this._BitField & 1) != 1)
								{
									throw new BadReadException(string.Format("  Inconsistent metadata at position 0x{0:X16}", archiveStream.Position - (long)num));
								}
								this._sourceIsEncrypted = true;
								if (num4 != 7)
								{
									throw new BadReadException(string.Format("  Inconsistent WinZip AES datasize (0x{0:X4}) at position 0x{1:X16}", num4, archiveStream.Position - (long)num));
								}
								this._WinZipAesMethod = BitConverter.ToInt16(array, i);
								i += 2;
								if (this._WinZipAesMethod != 1 && this._WinZipAesMethod != 2)
								{
									throw new BadReadException(string.Format("  Unexpected vendor version number (0x{0:X4}) for WinZip AES metadata at position 0x{1:X16}", this._WinZipAesMethod, archiveStream.Position - (long)num));
								}
								short num6 = BitConverter.ToInt16(array, i);
								i += 2;
								if (num6 != 17729)
								{
									throw new BadReadException(string.Format("  Unexpected vendor ID (0x{0:X4}) for WinZip AES metadata at position 0x{1:X16}", num6, archiveStream.Position - (long)num));
								}
								this._KeyStrengthInBits = -1;
								if (array[i] == 1)
								{
									this._KeyStrengthInBits = 128;
								}
								if (array[i] == 3)
								{
									this._KeyStrengthInBits = 256;
								}
								if (this._KeyStrengthInBits < 0)
								{
									throw new Exception(string.Format("Invalid key strength ({0})", this._KeyStrengthInBits));
								}
								this._Encryption = ((this._KeyStrengthInBits == 128) ? EncryptionAlgorithm.WinZipAes128 : EncryptionAlgorithm.WinZipAes256);
								i++;
								this._CompressionMethod = BitConverter.ToInt16(array, i);
								i += 2;
							}
						}
					}
					else
					{
						this._InputUsesZip64 = true;
						if (num4 > 28)
						{
							throw new BadReadException(string.Format("  Inconsistent datasize (0x{0:X4}) for ZIP64 extra field at position 0x{1:X16}", num4, archiveStream.Position - (long)num));
						}
						if (this._UncompressedSize == (long)(-1))
						{
							this._UncompressedSize = BitConverter.ToInt64(array, i);
							i += 8;
						}
						if (this._CompressedSize == (long)(-1))
						{
							this._CompressedSize = BitConverter.ToInt64(array, i);
							i += 8;
						}
						if (this._RelativeOffsetOfLocalHeader == (long)(-1))
						{
							this._RelativeOffsetOfLocalHeader = BitConverter.ToInt64(array, i);
							i += 8;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00009454 File Offset: 0x00007654
		private void SetFdpLoh()
		{
			long position = this.ArchiveStream.Position;
			this.ArchiveStream.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
			byte[] array = new byte[30];
			this.ArchiveStream.Read(array, 0, array.Length);
			short num = (short)((int)array[26] + (int)array[27] * 256);
			short num2 = (short)((int)array[28] + (int)array[29] * 256);
			this.ArchiveStream.Seek((long)(num + num2), SeekOrigin.Current);
			this._LengthOfHeader = (int)(30 + num2 + num);
			this.__FileDataPosition = this._RelativeOffsetOfLocalHeader + 30L + (long)num + (long)num2;
			if (this._Encryption == EncryptionAlgorithm.PkzipWeak)
			{
				this.__FileDataPosition += 12L;
			}
			else if (this.Encryption == EncryptionAlgorithm.WinZipAes128 || this.Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				this.__FileDataPosition += (long)(this._KeyStrengthInBits / 8 / 2 + 2);
			}
			this.ArchiveStream.Seek(position, SeekOrigin.Begin);
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000EA RID: 234 RVA: 0x00009570 File Offset: 0x00007770
		internal long FileDataPosition
		{
			get
			{
				if (this.__FileDataPosition == -1L)
				{
					this.SetFdpLoh();
				}
				return this.__FileDataPosition;
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000EB RID: 235 RVA: 0x000095AC File Offset: 0x000077AC
		private int LengthOfHeader
		{
			get
			{
				if (this._LengthOfHeader == 0)
				{
					this.SetFdpLoh();
				}
				return this._LengthOfHeader;
			}
		}

		// Token: 0x0400003F RID: 63
		private const int WORKING_BUFFER_SIZE = 17408;

		// Token: 0x04000040 RID: 64
		private const int Rfc2898KeygenIterations = 1000;

		// Token: 0x04000041 RID: 65
		private short _VersionMadeBy;

		// Token: 0x04000042 RID: 66
		private short _InternalFileAttrs;

		// Token: 0x04000043 RID: 67
		private int _ExternalFileAttrs;

		// Token: 0x04000044 RID: 68
		private int _LengthOfDirEntry;

		// Token: 0x04000045 RID: 69
		private short _filenameLength;

		// Token: 0x04000046 RID: 70
		private short _extraFieldLength;

		// Token: 0x04000047 RID: 71
		private short _commentLength;

		// Token: 0x04000048 RID: 72
		internal ZipCrypto _zipCrypto;

		// Token: 0x04000049 RID: 73
		internal WinZipAesCrypto _aesCrypto;

		// Token: 0x0400004A RID: 74
		internal short _KeyStrengthInBits;

		// Token: 0x0400004B RID: 75
		private short _WinZipAesMethod;

		// Token: 0x0400004C RID: 76
		internal DateTime _LastModified;

		// Token: 0x0400004D RID: 77
		private bool _TrimVolumeFromFullyQualifiedPaths = true;

		// Token: 0x0400004E RID: 78
		private bool _ForceNoCompression;

		// Token: 0x0400004F RID: 79
		internal string _LocalFileName;

		// Token: 0x04000050 RID: 80
		private string _FileNameInArchive;

		// Token: 0x04000051 RID: 81
		internal short _VersionNeeded;

		// Token: 0x04000052 RID: 82
		internal short _BitField;

		// Token: 0x04000053 RID: 83
		internal short _CompressionMethod;

		// Token: 0x04000054 RID: 84
		internal string _Comment;

		// Token: 0x04000055 RID: 85
		private bool _IsDirectory;

		// Token: 0x04000056 RID: 86
		private byte[] _CommentBytes;

		// Token: 0x04000057 RID: 87
		internal long _CompressedSize;

		// Token: 0x04000058 RID: 88
		internal long _CompressedFileDataSize;

		// Token: 0x04000059 RID: 89
		internal long _UncompressedSize;

		// Token: 0x0400005A RID: 90
		internal int _TimeBlob;

		// Token: 0x0400005B RID: 91
		private bool _crcCalculated = false;

		// Token: 0x0400005C RID: 92
		internal int _Crc32;

		// Token: 0x0400005D RID: 93
		internal byte[] _Extra;

		// Token: 0x0400005E RID: 94
		private bool _OverwriteOnExtract;

		// Token: 0x0400005F RID: 95
		private bool _metadataChanged;

		// Token: 0x04000060 RID: 96
		private bool _restreamRequiredOnSave;

		// Token: 0x04000061 RID: 97
		private bool _sourceIsEncrypted;

		// Token: 0x04000062 RID: 98
		private long _cdrPosition;

		// Token: 0x04000063 RID: 99
		private static Encoding ibm437 = Encoding.GetEncoding("IBM437");

		// Token: 0x04000064 RID: 100
		private Encoding _provisionalAlternateEncoding = Encoding.GetEncoding("IBM437");

		// Token: 0x04000065 RID: 101
		private Encoding _actualEncoding = null;

		// Token: 0x04000066 RID: 102
		internal ZipFile _zipfile;

		// Token: 0x04000067 RID: 103
		internal long __FileDataPosition = -1L;

		// Token: 0x04000068 RID: 104
		private byte[] _EntryHeader;

		// Token: 0x04000069 RID: 105
		internal long _RelativeOffsetOfLocalHeader;

		// Token: 0x0400006A RID: 106
		private long _TotalEntrySize;

		// Token: 0x0400006B RID: 107
		internal int _LengthOfHeader;

		// Token: 0x0400006C RID: 108
		internal int _LengthOfTrailer;

		// Token: 0x0400006D RID: 109
		private bool _InputUsesZip64;

		// Token: 0x0400006E RID: 110
		internal string _Password;

		// Token: 0x0400006F RID: 111
		internal EntrySource _Source = EntrySource.None;

		// Token: 0x04000070 RID: 112
		internal EncryptionAlgorithm _Encryption = EncryptionAlgorithm.None;

		// Token: 0x04000071 RID: 113
		internal byte[] _WeakEncryptionHeader;

		// Token: 0x04000072 RID: 114
		internal Stream _archiveStream;

		// Token: 0x04000073 RID: 115
		private Stream _sourceStream;

		// Token: 0x04000074 RID: 116
		private object LOCK = new object();

		// Token: 0x04000075 RID: 117
		private bool _ioOperationCanceled;

		// Token: 0x04000076 RID: 118
		private bool _presumeZip64;

		// Token: 0x04000077 RID: 119
		private bool? _entryRequiresZip64;

		// Token: 0x04000078 RID: 120
		private bool? _OutputUsesZip64;
	}
}
