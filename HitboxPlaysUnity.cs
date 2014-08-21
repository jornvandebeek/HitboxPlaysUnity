using UnityEngine;
using System.Collections;

// This class is for configuring your hitbox username and password from within the unity editor for testing or demo purposes
// Built game executables would need another solution like reading them from a file or getting them through the UI,
// unless you plan to only distribute your game and project files to people you trust with your hitbox credentials.
public class HitboxPlaysUnity : MonoBehaviour{
    public static HitboxIrcState instance;

    // Unity will save the values of these strings in some places once you configure them in the editor, 
    // so be sure to take them out before building your game for public release
    // and be careful when distributing your unity project files.
    // Also, the unity editor will show these fields in plain text on screen,
    // so be careful when streaming the editor directly.
    public string hitboxUsername;
    public string hitboxPassword;

    void Awake(){
        if (instance == null){
            instance = new HitboxIrcState(hitboxUsername, hitboxPassword);
        } else {
            Debug.Log("Already constructed global HitboxIrcState");
        }
    }

    void Start(){
        instance.ConnectToHitbox();
    }

    void OnDestroy() {
        instance.QuitHitbox();
        Debug.Log("Quitting HitboxIrcState");
    }

}
