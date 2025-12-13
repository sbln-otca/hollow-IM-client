using Hollow_IM_Client.Classes.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Hollow_IM_Client.Classes
{
    internal class Chat
    {
        private UserModel me;

        private int messagesState;
        private List<MessageModel> messages;

        private int usersState;
        private List<UserModel> users;

        public Chat(ChatModel chat)
        {
            this.me = chat.Me;

            messagesState = chat.MessagesState;
            messages = chat.Messages;

            usersState = chat.UsersState;
            users = chat.Users;
        }

        public void SendMessage(NetworkStream stream, string content)
        {
            MessageModel message = new MessageModel
            {
                User = me,
                SentAt = DateTime.Now,
                Content = content
            };

            HollowProtocol.SendMessage(stream, message);
        }
        public void AddMessage(MessageModel message)
        {
            messages.Add(message);
        }

        public void SyncChat(SyncChatModel state)
        {

            //if (latestState == null) 
            //    return;

            if (state.MessagesDelta != null)
            {
                messages.AddRange(state.MessagesDelta);
                messagesState = state.LastMessagesState;
            }
                
            if (state.UsersDelta != null)
            {
                foreach (UserDelta delta in state.UsersDelta)
                {
                    if (delta.UsersToRemove != null)
                    {
                        foreach(UserModel user in delta.UsersToRemove)
                        {
                            users.Remove(user);
                        }
                    }
                    if (delta.UsersToAdd != null)
                    {
                        foreach (UserModel user in delta.UsersToAdd)
                        {
                            users.Add(user);
                        }
                    }
                }
                usersState = state.LastUsersState;
            }
        }
    }
}
