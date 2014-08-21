using UnityEngine;
using System.Collections;
using ChatSharp;

// For this class to work you need to have the glados moderation bot present to your hitbox chat channel
// Follow the instructions on http://www.hitbox.tv/GLaDOS to add glados to your channel

// You want only 1 of these objects running at one time for each hitbox user's channel that you want to connect to.
// See HitboxPlaysUnity.cs for an example how to make a global object out of it.

public class HitboxIrcState{

    private string hitboxUsername;
    private string hitboxPassword;
    private IrcClient ircClient;

    public HitboxIrcState(string username, string password){
        hitboxUsername = username;
        hitboxPassword = password;
    }

    // This method will get called each time someone posts a message in chat
    private void HandleMessage(PrivateMessage message){
        // Source is the hitbox username of the person who sent the message
        // Message is the full text string that was sent by that person
        Debug.Log(message.Source + ":" + message.Message);
    }

    public void ConnectToHitbox(){
        if(ircClient != null) {
            Debug.Log("Already connected to hitbox");
            return;
        }
        
        if(hitboxUsername == null || hitboxPassword == null) {
            Debug.Log("Empty username or password");
            return;
        }

        // irc.glados.tv is an irc server that hosts irc channels that mirror every hitbox chat channel glados is part of.
        // In order for glados to retrieve chat messages from hitbox on your behalf and pose as you in the hitbox chat
        // it needs your hitbox username and password.
        // This means glados (a third party from hitbox) receives your username and password,
        // so do not use this software if you are not okay with that.
        ircClient = new IrcClient("irc.glados.tv", new IrcUser(hitboxUsername, hitboxUsername, hitboxPassword));

        // The channel name of a hitbox chat corresponds to the hitbox username, so the channel of user fred would be #fred
        ircClient.ConnectionComplete += (s, e) => ircClient.Channels.Join("#" + hitboxUsername);
        ircClient.ChannelMessageRecieved += (s, e) =>
        {
            HandleMessage(e.PrivateMessage);
        };
        
        ircClient.ConnectAsync();
    }

    public void QuitHitbox(){
        ircClient.Quit();
    }
}
