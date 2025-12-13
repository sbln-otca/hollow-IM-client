using Hollow_IM_Client.Classes.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Hollow_IM_Client.Classes
{
    internal class HollowClient
    {
        

        private TcpClient? client;

        private NetworkStream? serverStream;

        private Chat? chat;
        public HollowClient()
        {
            client = null;
            serverStream = null;
            chat = null;
        }

        private async Task<Response?> readResponseAsync(byte[] prefixBuffer)
        {
            using var cts1 = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                await serverStream!.ReadExactlyAsync(prefixBuffer, 0, 4, cts1.Token);

                Int32 length = BitConverter.ToInt32(prefixBuffer, 0);
                byte[] responseBuffer = new byte[length];

                using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await serverStream.ReadExactlyAsync(responseBuffer, 0, length, cts2.Token);

                string responseJson = Encoding.UTF8.GetString(responseBuffer);

                return JsonSerializer.Deserialize<Response>(responseJson)!;

            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Read timed out after 5 seconds.");
                return null;
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Stream ended before enough bytes were read.");
                return null;
            }

        }

        public async Task ReadLoop()
        {
            byte[] prefixBuffer = new byte[4];

            while (serverStream != null)
            {
                Response? response = await readResponseAsync(prefixBuffer);
                if (response == null)
                    continue;

                switch (response.Action)
                {
                    case "GET_CHAT_INFO":
                        if (response.Status)
                        {
                            var data = response.Payload.Deserialize<ChatModel>()!;

                            chat = new Chat(data);
                        }
                        break;
                    case "SEND_MESSAGE":
                        if (response.Status && chat != null)
                        {
                            var message = response.Payload.Deserialize<MessageModel>()!;

                            chat.AddMessage(message);
                        }
                        break;
                    case "SYNC_CHAT":
                        if (response.Status && chat != null)
                        {
                            var data = response.Payload.Deserialize<SyncChatModel>()!;
                            chat.SyncChat(data);
                        }
                        break;
                }
            }
        }

        public async Task Connect(string address, Int32 port, string username)
        {
            try
            {
                client = new TcpClient(address, port);

                serverStream = client.GetStream();

                UserModel user = new UserModel { username = username };

                HollowProtocol.JoinChat(serverStream, user);

                await ReadLoop();
            }
            catch (SocketException ex) {
                    Console.WriteLine($"Socket error: {ex.SocketErrorCode}");
            }
        }

        public void Disconnect() 
        {
            if (serverStream != null)
            {
                serverStream.Dispose();
                serverStream = null;
            }

            if (client != null)
            {
                client.Dispose();
                client = null;
            }

            chat = null;
        }

        public void SendMessage(string message) 
        {
            if (chat == null) 
                return;

            chat.SendMessage(serverStream!, message);
        }

    }
}
