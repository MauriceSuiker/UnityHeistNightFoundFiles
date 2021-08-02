using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(NetworkView))]
[RequireComponent(typeof(LineRenderer))]
public class SecurityGuard : MonoBehaviour
{
    public bool isSelected;
    public bool SeeingPlayer = false;
    LineRenderer viewCone;

    void Start()
    {
        viewCone = this.gameObject.GetComponent<LineRenderer>();
        if (Network.isServer || !Network.isClient && !Network.isServer)
        {
            gameObject.AddComponent<NavMeshAgent>();
            gameObject.GetComponent<NavMeshAgent>().angularSpeed = 300;
        }
        else if(Network.isClient)
        {
            viewCone.enabled = false;
        }
        SecurityGuard.SetAiDebugColor();
    }

    private void OnSelected()
    {
        renderer.material.color = Color.red;
        isSelected = true;
    }
    private void UnSelected()
    {
        renderer.material.color = Color.white;
        isSelected = false;
    }

    /////////////////////////////////// Vision cone code
    public int viewDistance = 10; //viewdistance for vision cone
    public float fieldOfViewRange = 68; // in degrees ( 68 gives him 68 degrees of sight)
    public float minPlayerDetectDistance = 4; // distance before it sees the player, even behind him ("hears him coming")

    public bool CanSeePlayer(GameObject player)
    {
        RaycastHit hit;
        Vector3 rayDirection = Vector3.zero;
        rayDirection = player.transform.position - transform.position; //direction to player (player position - enemy position = direction to player)
        rayDirection.Normalize();
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position); //calculate distance to be used for min and max distance
        if (distanceToPlayer < minPlayerDetectDistance)
        {
            return true;
        }
        else if (distanceToPlayer < viewDistance) //if it's further away then return false (optimization)
        {
            if ((Vector3.Angle(rayDirection, transform.forward)) < fieldOfViewRange) //check in the player is in view regarding degrees
            {
                if (Physics.Raycast(transform.position, rayDirection, out hit, viewDistance)) // check if the player really is in vision (no objects in the way like walls)
                {
                    if (hit.transform.gameObject.tag == "FPSPlayer") //when the player is detected (tag)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }
    public Color colorToUse = new Color(0, 0, 0, 255); //color of vision cone
    public bool ShowDebugLines = true; //show vision cone
    //public float lineWidth;
    void Update()
    {
        if(Application.isEditor)
        {
            DrawVisionCone();
        }
        viewCone.SetWidth(0, fieldOfViewRange);
        viewCone.SetPosition(0, transform.position);
        viewCone.SetPosition(1, transform.position + transform.forward * viewDistance + Vector3.up * transform.position.y);
    }
    void DrawVisionCone()
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward * minPlayerDetectDistance, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.forward * -minPlayerDetectDistance, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.right * minPlayerDetectDistance, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.right * -minPlayerDetectDistance, Color.red);

        // The actual vision cone visualization 
        for (int i = 0; i < fieldOfViewRange / 3; i++)
        {
            Debug.DrawLine(transform.position, transform.position + transform.forward * viewDistance - transform.right * (i * 3 - fieldOfViewRange / 2), colorToUse);
        }
    }
    static public void SetAiDebugColor() // set color for each guard  //for debug colors, to make it easier to see each seperate guard
    {
        GameObject[] ais = GameObject.FindGameObjectsWithTag("Guard");
        for (int i = 0; i < ais.Length; i++)
        {
            Color coll = new Color(0, 0, 0, 255);
            float colorr = 2295 / ais.Length * i;
            if (colorr < 256)
            {
                coll.r = colorr / 255;
            }
            else if (colorr >= 256 && colorr < 511)
            {
                coll.g = (colorr - 255) / 255;
            }
            else if (colorr >= 511 && colorr < 756)
            {
                coll.b = (colorr - 510) / 255;
            }
            else if (colorr >= 756 && colorr < 1011)
            {
                coll.b = (colorr - 755) / 255;
                coll.g = (colorr - 755) / 255;
            }
            else if (colorr >= 1011 && colorr < 1266)
            {
                coll.b = (colorr - 1010) / 255;
                coll.r = (colorr - 1010) / 255;
            }
            else if (colorr >= 1266 && colorr < 1521)
            {
                coll.r = (colorr - 1265) / 255;
                coll.g = (colorr - 1265) / 255;
            }
            else if (colorr >= 1521 && colorr < 1776)
            {
                coll.r = (colorr - 1520) / 255;
                coll.b = (colorr - 1520) / 255;
            }
            else if (colorr >= 1776 && colorr < 2031)
            {
                coll.g = (colorr - 1775) / 255;
                coll.r = (colorr - 1775) / 255;
            }
            else if (colorr >= 2031)
            {
                coll.g = (colorr - 2030) / 255;
                coll.b = (colorr - 2030) / 255;
            }
            coll.a = 1;
            ais[i].transform.GetComponent<SecurityGuard>().colorToUse = coll;
        }
    }
}