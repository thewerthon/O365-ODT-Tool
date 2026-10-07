using System.Net;
using System.Net.Http;
using O365ODTTool.Core.Setup;

namespace O365ODTTool.Core.Tests.Setup;

internal sealed class FakeSignatureVerifier(bool trusted) : ISignatureVerifier {

	public List<string> VerifiedPaths { get; } = [];

	public bool IsSignedBy(string path, string publisher) {

		VerifiedPaths.Add(path);

		return trusted;

	}

}

internal sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, byte[] content) : HttpMessageHandler {

	public int RequestCount { get; private set; }

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {

		RequestCount++;

		return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new ByteArrayContent(content) });

	}

}

internal sealed class SynchronousProgress : IProgress<string> {

	public List<string> Messages { get; } = [];

	public void Report(string value) => Messages.Add(value);

}
