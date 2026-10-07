namespace O365ODTTool.Core.Setup;

/// <summary>
/// Verifies digital signatures of executable files.
/// </summary>
public interface ISignatureVerifier {

	/// <summary>
	/// Determines whether a file carries a valid signature issued to the expected publisher.
	/// </summary>
	bool IsSignedBy(string path, string publisher);

}
