using ChatSharp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatSharp.Handlers
{
    internal static class ChannelHandlers
    {
        public static void HandleJoin(IrcClient client, IrcMessage message)
        {
            if (client.User.Nick == new IrcUser(message.Prefix).Nick)
            {
                // We've joined this channel
                var channel = new IrcChannel(client, message.Parameters[0]);
                client.Channels.Add(channel);
            }
            else
            {
                // Someone has joined a channel we're already in
                client.Channels[message.Parameters[0]].Users.Add(new IrcUser(message.Prefix));
            }
            client.OnUserJoinedChannel(new ChannelUserEventArgs(client.Channels[message.Parameters[0]], new IrcUser(message.Prefix)));
        }

        public static void HandlePart(IrcClient client, IrcMessage message)
        {
            if (client.User.Match(message.Prefix)) // We've parted this channel
                client.Channels.Remove(client.Channels[message.Parameters[0]]);
            else // Someone has parted a channel we're already in
            {
                var user = new IrcUser(message.Prefix).Nick;
                var channel = client.Channels[message.Parameters[0]];
                if (channel.Users.Contains(user))
                    channel.Users.Remove(user);
                foreach (var mode in channel.UsersByMode)
                {
                    if (mode.Value.Contains(user))
                        mode.Value.Remove(user);
                }
            }
            client.OnUserPartedChannel(new ChannelUserEventArgs(client.Channels[message.Parameters[0]], new IrcUser(message.Prefix)));
        }

        public static void HandleUserListPart(IrcClient client, IrcMessage message)
        {
            var channel = client.Channels[message.Parameters[2]];
            var users = message.Parameters[3].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < users.Length; i++)
            {
                var user = users[i];
                var mode = client.ServerInfo.GetModeForPrefix(user[0]);
                if (mode == null)
                    channel.Users.Add(new IrcUser(user));
                else
                {
                    channel.Users.Add(new IrcUser(user.Substring(1)));
                    if (!channel.UsersByMode.ContainsKey(mode.Value))
                        channel.UsersByMode.Add(mode.Value, new UserCollection());
                    channel.UsersByMode[mode.Value].Add(new IrcUser(user.Substring(1)));
                }
            }
        }

        public static void HandleUserListEnd(IrcClient client, IrcMessage message)
        {
            var channel = client.Channels[message.Parameters[1]];
            client.OnChannelListRecieved(new ChannelEventArgs(channel));
            if (client.Settings.ModeOnJoin)
            {
                try
                {
                    client.GetMode(channel.Name, c => Console.WriteLine(c.Mode));
                }
                catch { }
            }
        }

        public static void HandleKick(IrcClient client, IrcMessage message)
        {
            var channel = client.Channels[message.Parameters[0]];
            var kicked = channel.Users[message.Parameters[1]];
            if (message.Parameters[1] == client.User.Nick) // We've been kicked
                client.Channels.Remove(client.Channels[message.Parameters[0]]);
            else
            {
                if (channel.Users.Contains(message.Parameters[1]))
                    channel.Users.Remove(message.Parameters[1]);
                foreach (var mode in channel.UsersByMode)
                {
                    if (mode.Value.Contains(message.Parameters[1]))
                        mode.Value.Remove(message.Parameters[1]);
                }
            }
            client.OnUserKicked(new KickEventArgs(channel, new IrcUser(message.Prefix),
                kicked, message.Parameters[2]));
        }
    }
}
