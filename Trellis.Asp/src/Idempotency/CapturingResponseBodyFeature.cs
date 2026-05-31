namespace Trellis.Asp.Idempotency;

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

/// <summary>
/// Decorator over an <see cref="IHttpResponseBodyFeature"/> that tees writes into a
/// bounded in-memory capture buffer while still forwarding every byte to the original
/// response stream. The capture is used by the idempotency middleware to record a replayable
/// response snapshot.
/// </summary>
/// <remarks>
/// <para>
/// Writes go to <c>both</c> the original feature stream and a <see cref="MemoryStream"/>
/// capture. If the capture exceeds <c>maxBytes</c>, capture is aborted and only the original
/// stream continues to receive bytes — the client is never affected by capture failure.
/// </para>
/// <para>
/// <c>SendFileAsync</c> calls abort capture (the file content does not flow through the wrapped
/// stream). When capture is aborted <see cref="GetCapturedBytes"/> returns <see langword="null"/>
/// and the middleware skips recording a snapshot for this request.
/// </para>
/// </remarks>
public sealed class CapturingResponseBodyFeature : IHttpResponseBodyFeature, IDisposable
{
    private readonly IHttpResponseBodyFeature inner;
    private readonly MemoryStream capture;
    private readonly TeeStream teeStream;
    private PipeWriter? cachedWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapturingResponseBodyFeature"/> class.
    /// </summary>
    /// <param name="inner">The wrapped response body feature.</param>
    /// <param name="maxBytes">The maximum number of bytes to retain in the capture buffer.</param>
    public CapturingResponseBodyFeature(IHttpResponseBodyFeature inner, long maxBytes)
    {
        ArgumentNullException.ThrowIfNull(inner);
        if (maxBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBytes), "maxBytes must be positive.");
        }

        this.inner = inner;
        this.capture = new MemoryStream();
        this.teeStream = new TeeStream(inner.Stream, this.capture, maxBytes, this);
    }

    /// <inheritdoc/>
    public Stream Stream => this.teeStream;

    /// <inheritdoc/>
    public PipeWriter Writer => this.cachedWriter ??= PipeWriter.Create(this.teeStream, new StreamPipeWriterOptions(leaveOpen: true));

    /// <summary>
    /// Gets a value indicating whether the capture has been aborted (overflow, SendFile, or trailers).
    /// </summary>
    public bool CaptureAborted { get; private set; }

    /// <inheritdoc/>
    public Task CompleteAsync() => this.inner.CompleteAsync();

    /// <inheritdoc/>
    public void DisableBuffering() => this.inner.DisableBuffering();

    /// <inheritdoc/>
    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken)
    {
        this.CaptureAborted = true;
        return this.inner.SendFileAsync(path, offset, count, cancellationToken);
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken = default) => this.inner.StartAsync(cancellationToken);

    /// <summary>
    /// Returns the captured bytes if capture is intact, otherwise <see langword="null"/>.
    /// </summary>
    public byte[]? GetCapturedBytes() => this.CaptureAborted ? null : this.capture.ToArray();

    /// <summary>
    /// Marks capture as aborted from an external observer (for example, when the middleware
    /// detects that response trailers were written or that <c>HasStarted</c> was set before
    /// the wrapper was installed).
    /// </summary>
    public void AbortCapture() => this.CaptureAborted = true;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.cachedWriter?.Complete();
        this.capture.Dispose();
    }

    private sealed class TeeStream : Stream
    {
        private readonly Stream original;
        private readonly MemoryStream capture;
        private readonly long maxBytes;
        private readonly CapturingResponseBodyFeature owner;

        public TeeStream(Stream original, MemoryStream capture, long maxBytes, CapturingResponseBodyFeature owner)
        {
            this.original = original;
            this.capture = capture;
            this.maxBytes = maxBytes;
            this.owner = owner;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => this.original.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) => this.original.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.original.Write(buffer, offset, count);
            this.TryCapture(buffer.AsSpan(offset, count));
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await this.original.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
            this.TryCapture(buffer.AsSpan(offset, count));
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await this.original.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            this.TryCapture(buffer.Span);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            this.original.Write(buffer);
            this.TryCapture(buffer);
        }

        private void TryCapture(ReadOnlySpan<byte> buffer)
        {
            if (this.owner.CaptureAborted)
            {
                return;
            }

            if (this.capture.Length + buffer.Length > this.maxBytes)
            {
                this.owner.AbortCapture();
                return;
            }

            this.capture.Write(buffer);
        }
    }
}
