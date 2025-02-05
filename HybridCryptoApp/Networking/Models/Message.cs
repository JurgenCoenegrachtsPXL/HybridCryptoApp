﻿using HybridCryptoApp.Crypto;
using System;

namespace HybridCryptoApp.Networking.Models
{
    public class Message : IComparable<Message>
    {
        /// <summary>
        /// Datatype in this message
        /// </summary>
        public DataType DataType { get; set; }

        public ContactPerson Sender { get; set; }

        /// <summary>
        /// Name of sender
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Time at which message was sent
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// Actual message content
        /// </summary>
        public string MessageFromSender { get; set; }

        /// <summary>
        /// Original packet
        /// </summary>
        public StrippedDownEncryptedPacket Packet { get; set; }

        /// <inheritdoc />
        public int CompareTo(Message other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return SendTime.CompareTo(other.SendTime);
        }
    }
}