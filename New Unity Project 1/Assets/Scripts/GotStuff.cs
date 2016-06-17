using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class GotStuff : MonoBehaviour
{

    public Text stuffText = null;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void setStuff()
    {
        stuffText.text = "You got stuff";
    }
}
