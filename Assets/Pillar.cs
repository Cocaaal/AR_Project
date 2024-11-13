using MathNet.Numerics.Distributions;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Pillar : MonoBehaviour
{

    private Outline outline;
    private int nbDisks = 0;
    private float diskHeight = 0.15f;
    private Vector3 bottomLocalPosition = new Vector3(0, -0.92f, 0);
    public bool intialPillar = false;

    // Start is called before the first frame update
    void Start()
    {
        outline = GetComponent<Outline>();
        if (intialPillar)
        {
            //nbDisks = 5;
            place(GameObject.Find("5"));
            place(GameObject.Find("4"));
            place(GameObject.Find("3"));
            place(GameObject.Find("2"));
            place(GameObject.Find("1"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void hover(bool hover) 
    {
        outline.enabled = hover;
    }

    public int smallestDisk()
    {
        int smallest = 100;
        foreach (Transform disk in transform)
        {
            int number = int.Parse(disk.name);
            if (number < smallest)
            {
                smallest = number;
            }
        }
        return smallest;
    }

    public void place(GameObject newDisk)
    {
        newDisk.transform.SetParent(this.transform);
        newDisk.transform.localPosition = bottomLocalPosition + new Vector3(0, nbDisks*diskHeight, 0);
        newDisk.transform.localRotation = Quaternion.identity;
        nbDisks++;
    }

    public void remove()
    {
        nbDisks--;
    }

}
