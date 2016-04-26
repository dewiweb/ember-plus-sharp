﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Represents a command including command-specific data but excluding payload. This is the base of all
    /// commands.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class S101Command : IEquatable<S101Command>
    {
        private const byte DefaultVersion = 0x01;
        private static readonly Task Completed = Task.FromResult(false);

        private readonly CommandType commandType;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public bool Equals(S101Command other)
        {
            // We're enforcing the glow dtd version for EmberData instances, so it is sufficient to compare the
            // command types.
            return (other != null) && (other.commandType == this.commandType);
        }

        /// <inheritdoc/>
        public sealed override bool Equals(object obj)
        {
            return this.Equals(obj as S101Command);
        }

        /// <inheritdoc/>
        public sealed override int GetHashCode()
        {
            return (int)this.commandType;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.commandType.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal S101Command(CommandType commandType)
        {
            this.commandType = commandType;
        }

        internal virtual bool CanHavePayload
        {
            get { return false; }
        }

        internal virtual bool CanHaveMultiplePackets
        {
            get { return false; }
        }

        internal PacketFlags PacketFlags { get; set; }

        internal async Task WriteToAsync(WriteBuffer writeBuffer, CancellationToken cancellationToken)
        {
            await writeBuffer.ReserveAsync(2, cancellationToken);
            writeBuffer[writeBuffer.Count++] = (byte)this.commandType;
            writeBuffer[writeBuffer.Count++] = DefaultVersion;
            await this.WriteToCoreAsync(writeBuffer, cancellationToken);
        }

        internal virtual Task ReadFromCoreAsync(ReadBuffer readBuffer, CancellationToken cancellationToken)
        {
            return Completed;
        }

        internal virtual Task WriteToCoreAsync(WriteBuffer writeBuffer, CancellationToken cancellationToken)
        {
            return Completed;
        }

        internal virtual void ParseCore(string[] components)
        {
        }

        internal static async Task<S101Command> ReadFromAsync(ReadBuffer readBuffer, CancellationToken cancellationToken)
        {
            await readBuffer.FillAsync(2, cancellationToken);
            var result = GetCommandAndVersion(readBuffer);
            await result.ReadFromCoreAsync(readBuffer, cancellationToken);
            return result;
        }

        internal static S101Command Parse(string str)
        {
            var components = str.Split();
            var commandType = (CommandType)Enum.Parse(typeof(CommandType), components[0]);
            var result = CreateCommand(commandType);
            result.ParseCore(components);
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static S101Command GetCommandAndVersion(ReadBuffer readBuffer)
        {
            var commandType = (CommandType)readBuffer[readBuffer.Index++];

            if (readBuffer[readBuffer.Index++] != DefaultVersion)
            {
                throw new S101Exception("Unexpected Version.");
            }

            return CreateCommand(commandType);
        }

        private static S101Command CreateCommand(CommandType commandType)
        {
            switch (commandType)
            {
                case CommandType.EmberData:
                    return new EmberData();
                case CommandType.KeepAliveRequest:
                    return new KeepAliveRequest();
                case CommandType.KeepAliveResponse:
                    return new KeepAliveResponse();
                default:
                    throw new S101Exception("Unexpected Command.");
            }
        }
    }
}