using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoManager : MonoBehaviour {
    static VideoManager videoManager = null;

	void Start () {
		if (videoManager == null) {
            videoManager = this;
        }
	}

    static public bool getFrame(int id, out Texture2D texture) {
        texture = null;
        return false;
    }
}
