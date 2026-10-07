using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace O365ODTTool.Services;

/// <summary>
/// Shows the modern system folder picker, which the framework does not expose on this runtime.
/// </summary>
internal static class FolderPicker {

	private const uint pickFoldersOption = 0x20;
	private const uint forceFileSystemOption = 0x40;
	private const uint fileSystemPathName = 0x80058000;
	private const int cancelledResult = unchecked((int)0x800704C7);

	private static readonly Guid shellItemId = new("43826D1E-E718-42EE-BC55-A1E261C37BFE");

	/// <summary>
	/// Shows the picker and returns the selected folder, or <see langword="null"/> when cancelled.
	/// </summary>
	public static string? Pick(string initialFolder) {

		var dialog = (IFileOpenDialog)new FileOpenDialogCoClass();

		try {

			dialog.GetOptions(out var options);
			dialog.SetOptions(options | pickFoldersOption | forceFileSystemOption);
			SetInitialFolder(dialog, initialFolder);

			var result = dialog.Show(GetOwnerHandle());

			if (result == cancelledResult) return null;

			Marshal.ThrowExceptionForHR(result);
			dialog.GetResult(out var item);
			item.GetDisplayName(fileSystemPathName, out var pathPointer);

			try {

				return Marshal.PtrToStringUni(pathPointer);

			} finally {

				Marshal.FreeCoTaskMem(pathPointer);

			}

		} finally {

			Marshal.ReleaseComObject(dialog);

		}

	}

	private static IntPtr GetOwnerHandle() {

		var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(static window => window.IsActive) ?? Application.Current?.MainWindow;

		return owner is null ? IntPtr.Zero : new WindowInteropHelper(owner).Handle;

	}

	private static void SetInitialFolder(IFileOpenDialog dialog, string initialFolder) {

		if (!Directory.Exists(initialFolder)) return;

		var itemId = shellItemId;

		if (SHCreateItemFromParsingName(initialFolder, IntPtr.Zero, ref itemId, out var item) != 0) return;

		dialog.SetFolder(item);

	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
	private static extern int SHCreateItemFromParsingName(string path, IntPtr bindContext, ref Guid interfaceId, out IShellItem item);

	[ComImport]
	[Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
	private class FileOpenDialogCoClass { }

	[ComImport]
	[Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IFileOpenDialog {

		[PreserveSig]
		int Show(IntPtr parent);

		void SetFileTypes(uint count, IntPtr filterSpecs);

		void SetFileTypeIndex(uint index);

		void GetFileTypeIndex(out uint index);

		void Advise(IntPtr events, out uint cookie);

		void Unadvise(uint cookie);

		void SetOptions(uint options);

		void GetOptions(out uint options);

		void SetDefaultFolder(IShellItem item);

		void SetFolder(IShellItem item);

		void GetFolder(out IShellItem item);

		void GetCurrentSelection(out IShellItem item);

		void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);

		void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);

		void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);

		void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);

		void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);

		void GetResult(out IShellItem item);

	}

	[ComImport]
	[Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IShellItem {

		void BindToHandler(IntPtr bindContext, ref Guid handlerId, ref Guid interfaceId, out IntPtr result);

		void GetParent(out IShellItem parent);

		void GetDisplayName(uint nameType, out IntPtr name);

		void GetAttributes(uint mask, out uint attributes);

		void Compare(IShellItem other, uint hint, out int order);

	}

}
