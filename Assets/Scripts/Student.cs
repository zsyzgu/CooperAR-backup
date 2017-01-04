using UnityEngine;
using UnityEngine.UI;

public class Student : MonoBehaviour {
    public GameObject rawImage;

    private int id;

	void Start () {

    }
	
	void Update () {
        dealTransform();
        dealVideo();
    }

    public void setID(int id) {
        this.id = id;
    }

    private void dealTransform() {
        Vector3 pos;
        Vector3 rot;
        if (Tracking.getTransform(id, out pos, out rot)) {
            transform.position = pos;
            transform.eulerAngles = rot;
        }
    }

    private void dealVideo() {
        Texture2D texture;
        if (VideoManager.getFrame(id, out texture)) {
            Destroy(rawImage.GetComponent<RawImage>().texture);
            rawImage.GetComponent<RawImage>().texture = texture;
        }
    }
}
