using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Hollow_IM_Client.Classes.Models
{
    internal class ClientChatState
    {
        [JsonPropertyName("messages_state")]
        public required int MessagesState { get; set; }
    }
}
