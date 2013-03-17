﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using MARC.HI.EHRS.CR.Core.ComponentModel;

namespace MARC.HI.EHRS.CR.Messaging.Admin
{
    [XmlRoot("registrationEventCollection")]
    [XmlType("RegistrationEventCollection")]
    public class RegistrationEventCollection 
    {

        /// <summary>
        /// Creates a new instance of the registration event collection
        /// </summary>
        public RegistrationEventCollection()
        {
            this.Event = new List<RegistrationEvent>();
        }

        /// <summary>
        /// Registration event collection
        /// </summary>
        [XmlElement("registration")]
        public List<RegistrationEvent> Event { get; set; }

    }
}
