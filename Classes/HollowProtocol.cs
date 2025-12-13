using Hollow_IM_Client.Classes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Hollow_IM_Client.Classes
{
    internal class HollowProtocol
    {
        private static Byte[] BuildRequestPacket(Request request)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            string serialized = JsonSerializer.Serialize(request);
            Byte[] bytes = Encoding.UTF8.GetBytes(serialized);

            // Write Length Prefix (int32)
            writer.Write(bytes.Length);

            // Write Payload
            writer.Write(bytes);

            byte[] packet = ms.ToArray();
            return packet;
        }
        public static void JoinChat(NetworkStream stream, UserModel user)
        {
            string userStr = JsonSerializer.Serialize<UserModel>(user);
            var userJson = JsonDocument.Parse(userStr);

            var request = new Models.Request { Action = "JOIN_CHAT", Payload = userJson.RootElement.Clone() };

            var packet = BuildRequestPacket(request);
            stream.Write(packet, 0, packet.Length);

            return;
        }
        public static void SendMessage(NetworkStream stream, MessageModel message)
        {
            string messageStr = JsonSerializer.Serialize<MessageModel>(message);
            var messageJson = JsonDocument.Parse(messageStr);

            var request = new Models.Request { Action = "SEND_MESSAGE", Payload = messageJson.RootElement.Clone() };

            var packet = BuildRequestPacket(request);
            stream.Write(packet, 0, packet.Length);

            return;
        }
        public static void SyncChat(NetworkStream stream, int messageState, int userState)
        {
            string stateStr = JsonSerializer.Serialize<int>(messageState);
            var stateJson = JsonDocument.Parse(stateStr);

            var request = new Models.Request { Action = "SYNC_CHAT", Payload = stateJson.RootElement.Clone() };

            var packet = BuildRequestPacket(request);
            stream.Write(packet, 0, packet.Length);

            return;
        }
    }
}
