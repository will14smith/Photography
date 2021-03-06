﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Toxon.Photography.CloudWatchEvents
{
    /// <summary>
    /// An object representing a container instance or task attachment.
    /// https://docs.aws.amazon.com/AmazonECS/latest/APIReference/API_Attachment.html
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Details of the attachment. For elastic network interfaces, this includes the
        /// network interface ID, the MAC address, the subnet ID, and the private IPv4 address.
        /// </summary>
        public List<KeyValuePair<string, string>> Details { get; set; }

        /// <summary>
        /// The unique identifier for the attachment.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The status of the attachment. Valid values are PRECREATED, CREATED, ATTACHING,
        /// ATTACHED, DETACHING, DETACHED, and DELETED.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The type of the attachment, such as ElasticNetworkInterface.
        /// </summary>
        public string Type { get; set; }
    }
}
