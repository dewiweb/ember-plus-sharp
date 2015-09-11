﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using Lawo.IO;

    /// <summary>Transparently decodes a single S101 message.</summary>
    /// <remarks>
    /// <para>At construction, a <see cref="Message"/> object is first decoded from the <see cref="ReadBuffer"/> object
    /// passed to <see cref="MessageDecodingStream.MessageDecodingStream"/> and made available through the
    /// <see cref="MessageDecodingStream.Message"/> property. Afterwards, a call to any of the Read methods of this
    /// stream removes data from <see cref="ReadBuffer"/> object passed to the constructor. The data is then decoded and
    /// the decoded form is then returned.</para>
    /// <para>If a message contains multiple packets, their payload is automatically joined such that it can be read
    /// through this stream as if the message consisted of only one packet.</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    internal sealed class MessageDecodingStream : NonSeekableStream
    {
        private readonly ReadBuffer rawBuffer;
        private DeframingStream deframingStream;
        private readonly ReadBuffer deframedBuffer;
        private readonly byte[] discardBuffer;
        private S101Message message;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public sealed override async Task DisposeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (this.deframingStream != null)
                {
                    while (await this.ReadAsync(
                        this.discardBuffer, 0, this.discardBuffer.Length, cancellationToken) > 0)
                    {
                    }

                    await this.deframingStream.DisposeAsync(cancellationToken);
                    await base.DisposeAsync(cancellationToken);
                }
            }
            finally
            {
                this.deframingStream = null;
            }
        }

        public sealed override bool CanRead
        {
            get { return this.deframingStream != null; }
        }

        public sealed override Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.AssertNotDisposed();
            BufferHelper.AssertValidRange(buffer, "buffer", offset, "offset", count, "count");
            return StreamHelper.TryFillAsync(this.ReadFromCurrentPacketAsync, buffer, offset, count, cancellationToken);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static async Task<MessageDecodingStream> CreateAsync(
            ReadBuffer rawBuffer, byte[] discardBuffer, CancellationToken cancellationToken)
        {
            var result = new MessageDecodingStream(rawBuffer, discardBuffer);
            var newMessage = await S101Message.ReadFromAsync(result.deframedBuffer, cancellationToken);

            if ((newMessage != null) && newMessage.CanHaveMultiplePackets &&
                ((newMessage.PacketFlags & PacketFlags.FirstPacket) == 0))
            {
                throw new S101Exception(string.Format(
                    CultureInfo.InvariantCulture, "Missing {0} flag in first packet.", PacketFlags.FirstPacket));
            }

            result.message = newMessage;
            return result;
        }

        internal S101Message Message
        {
            get { return this.message; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private MessageDecodingStream(ReadBuffer rawBuffer, byte[] discardBuffer)
        {
            this.rawBuffer = rawBuffer;
            this.deframingStream = new DeframingStream(this.rawBuffer);

            // This buffer is kept small in size, because a new one is allocated for each message.
            // This has the effect that only the bytes of reads <= MessageHeaderMaxLength bytes are actually copied into
            // this buffer. Larger reads are automatically done by calling this.ReadDeframed (without copying the bytes
            // into the MessageHeaderMaxLength byte buffer first). The former happens when packet headers are read
            // (multiple small-sized reads), the latter happens when the payload is read (typically done with a buffer
            // >= 1024 bytes).
            // This approach minimizes the allocations per message, while guaranteeing the best possible performance for
            // header *and* payload reading.
            this.deframedBuffer =
                new ReadBuffer((ReadAsyncCallback)this.ReadDeframedAsync, Constants.MessageHeaderMaxLength);
            this.discardBuffer = discardBuffer;
        }

        private async Task<int> ReadFromCurrentPacketAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read;

            while (((read = await this.deframedBuffer.ReadAsync(buffer, offset, count, cancellationToken)) == 0) &&
                (count > 0) && (this.message != null) && this.message.CanHavePayload &&
                this.message.CanHaveMultiplePackets && ((this.message.PacketFlags & PacketFlags.LastPacket) == 0))
            {
                this.deframingStream.Dispose();
                this.deframingStream = new DeframingStream(this.rawBuffer);
                this.ValidateMessage(await S101Message.ReadFromAsync(this.deframedBuffer, cancellationToken));
            }

            return read;
        }

        private void ValidateMessage(S101Message newMessage)
        {
            if (newMessage == null)
            {
                throw new S101Exception("Unexpected end of stream.");
            }

            if (this.message.Slot != newMessage.Slot)
            {
                throw new S101Exception("Inconsistent Slot in multi-packet message.");
            }

            if (!this.message.Command.Equals(newMessage.Command))
            {
                throw new S101Exception("Inconsistent Command in multi-packet message.");
            }

            if ((newMessage.PacketFlags & PacketFlags.FirstPacket) > 0)
            {
                throw new S101Exception(string.Format(
                    CultureInfo.InvariantCulture, "{0} flag in subsequent packet.", PacketFlags.FirstPacket));
            }

            this.message = newMessage;
        }

        private Task<int> ReadDeframedAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            return this.deframingStream.ReadAsync(buffer, index, count, cancellationToken);
        }
    }
}