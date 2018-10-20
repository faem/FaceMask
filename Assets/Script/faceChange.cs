using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class faceChange : MonoBehaviour {
    public GameObject m1;
    public GameObject m2;
    public GameObject m3;
    public GameObject m4;
    public GameObject m5;
    int i = 0;
    // Use this for initialization
    void Start () {
        m1.SetActive(true);
        m2.SetActive(false);
        m3.SetActive(false);
        m4.SetActive(false);
        m5.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        switch (i)
        {
            case 0:
                m1.SetActive(true);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(false);
                m5.SetActive(false);
                break;
            case 1:
                m1.SetActive(false);
                m2.SetActive(true);
                m3.SetActive(false);
                m4.SetActive(false);
                m5.SetActive(false);
                break;
            case 2:
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(true);
                m4.SetActive(false);
                m5.SetActive(false);
                break;
            case 3:
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(true);
                m5.SetActive(false);
                break;
            case 4:
                m1.SetActive(false);
                m2.SetActive(false);
                m3.SetActive(false);
                m4.SetActive(false);
                m5.SetActive(true);
                break;
            case 5:
                i = 0; 
                break;
        }
    }

    public void Change()
    {
        Debug.Log("i=" + i);
        
        i++;
    }
}
