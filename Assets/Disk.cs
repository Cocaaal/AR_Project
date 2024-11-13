using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Disk : MonoBehaviour
{

    private Outline outline;
    private TipTool tipTool;
    private MeshRenderer meshRenderer;

    private Color hoverColor = Color.red;
    private Color originColor;

    // Start is called before the first frame update
    void Start()
    {
        outline = GetComponent<Outline>();
        tipTool = GameObject.Find("ToolTarget").GetComponent<TipTool>();
        meshRenderer = GetComponent<MeshRenderer>();
        originColor = meshRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tip")
        {
            Debug.Log("ENTER "+ this.name + " AND " + other.name);
            hover(true);
            tipTool.setHoveredDisk(this.gameObject);
        }
        else if (other.gameObject.tag == "Pillar" && tipTool.diskSelected)
        {
            Debug.Log("ENTER " + this.name + " AND " + other.name);
            other.GetComponent<Pillar>().hover(true);
            tipTool.setHoveredPillar(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Tip" && (tipTool.hoveredDisk!=null && tipTool.hoveredDisk.name==this.name))
        {
            Debug.Log("EXIT " + this.name + " AND " + other.name);
            hover(false);
            tipTool.setHoveredDisk(null);
        }
        else if (other.gameObject.tag == "Pillar" && (tipTool.hoveredPillar!=null && tipTool.hoveredPillar.name == other.name))
        {
            Debug.Log("EXIT " + this.name + " AND " + other.name);
            other.GetComponent<Pillar>().hover(false);
            tipTool.setHoveredPillar(null);
        }
    }

    public void hover(bool hover)
    {
        if (hover)
        {
            meshRenderer.material.color = hoverColor;
        }else
        {
            meshRenderer.material.color = originColor;
        }
    }
}
