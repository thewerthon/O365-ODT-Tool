using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace O365ODTTool.Core.Setup;

/// <summary>
/// Verifies embedded Authenticode signatures of executable files.
/// </summary>
public sealed class AuthenticodeSignatureVerifier : ISignatureVerifier {

	private const int trustSuccess = 0;
	private const uint uiChoiceNone = 2;
	private const uint revokeNone = 0;
	private const uint unionChoiceFile = 1;
	private const uint stateActionVerify = 1;
	private const uint stateActionClose = 2;

	private static readonly Guid genericVerifyAction = new("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

	/// <inheritdoc />
	public bool IsSignedBy(string path, string publisher) => File.Exists(path) && IsTrusted(path) && HasPublisher(path, publisher);

	private static bool HasPublisher(string path, string publisher) {

		try {

			using var certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(path));

			return string.Equals(certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false), publisher, StringComparison.Ordinal);

		} catch (CryptographicException) {

			return false;

		}

	}

	private static bool IsTrusted(string path) {

		var fileInfo = new NativeMethods.WinTrustFileInfo {
			StructSize = (uint)Marshal.SizeOf<NativeMethods.WinTrustFileInfo>(),
			FilePath = path,
		};
		var fileInfoPointer = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.WinTrustFileInfo>());

		Marshal.StructureToPtr(fileInfo, fileInfoPointer, fDeleteOld: false);

		var dataPointer = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.WinTrustData>());

		try {

			var data = new NativeMethods.WinTrustData {
				StructSize = (uint)Marshal.SizeOf<NativeMethods.WinTrustData>(),
				UiChoice = uiChoiceNone,
				RevocationChecks = revokeNone,
				UnionChoice = unionChoiceFile,
				FileInfo = fileInfoPointer,
				StateAction = stateActionVerify,
			};

			Marshal.StructureToPtr(data, dataPointer, fDeleteOld: false);

			var result = NativeMethods.WinVerifyTrust(IntPtr.Zero, genericVerifyAction, dataPointer);

			data = Marshal.PtrToStructure<NativeMethods.WinTrustData>(dataPointer);
			data.StateAction = stateActionClose;
			Marshal.StructureToPtr(data, dataPointer, fDeleteOld: false);
			NativeMethods.WinVerifyTrust(IntPtr.Zero, genericVerifyAction, dataPointer);

			return result == trustSuccess;

		} finally {

			Marshal.DestroyStructure<NativeMethods.WinTrustFileInfo>(fileInfoPointer);
			Marshal.FreeHGlobal(dataPointer);
			Marshal.FreeHGlobal(fileInfoPointer);

		}

	}

	private static class NativeMethods {

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct WinTrustFileInfo {

			public uint StructSize;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string FilePath;

			public IntPtr FileHandle;

			public IntPtr KnownSubject;

		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct WinTrustData {

			public uint StructSize;

			public IntPtr PolicyCallbackData;

			public IntPtr SipClientData;

			public uint UiChoice;

			public uint RevocationChecks;

			public uint UnionChoice;

			public IntPtr FileInfo;

			public uint StateAction;

			public IntPtr StateData;

			public IntPtr UrlReference;

			public uint ProviderFlags;

			public uint UiContext;

			public IntPtr SignatureSettings;

		}

		[DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
		internal static extern int WinVerifyTrust(IntPtr windowHandle, [MarshalAs(UnmanagedType.LPStruct)] Guid actionId, IntPtr data);

	}

}
