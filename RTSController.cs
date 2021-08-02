using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
public class RTSController : MonoBehaviour 
{
    //speed the camera moves
    float moveSpeed = 10;
    //speed camera moves when shift is NOT being pressed
    float baseSpeed = 10;
    //speed that gets added when shift is being pressed
    public float bonusSpeed = 30;

    Transform t;
    public SecurityGuard[] SelectableUnits;
    public GameObject fpsPlayer;
    public float minCameraHeight = 6;
    public float maxCameraHeight = 40;

	// Use this for initialization
	void Start ()
    {
        t = transform;

        //If this script is NOT Network.Instantiated by you (server/client side) it will disable this script and the camera object (preventing overlapping camera views) 
        this.enabled = networkView.isMine;
        camera.enabled = networkView.isMine;
        //put the guards in the list
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Guard");
        SelectableUnits = new SecurityGuard[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            SelectableUnits[i] = temp[i].GetComponent<SecurityGuard>();
        }
        //offline play
        if (Network.isServer || !Network.isClient && !Network.isServer)
        {
            fpsPlayer = GameObject.FindGameObjectWithTag("FPSPlayer");
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        //see if the FPS player is being seen
        bool canSeeFPS = false;
        foreach (SecurityGuard g in SelectableUnits)
        {
            if (fpsPlayer != null)
            {
                if (g.SeeingPlayer = g.CanSeePlayer(fpsPlayer))
                {
                    canSeeFPS = true;
                }
            }
        }
        if (fpsPlayer != null)
        {
            fpsPlayer.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = canSeeFPS;
        }
        //zooming out
        if(Input.GetAxis("Mouse ScrollWheel") < 0 && t.position.y < maxCameraHeight)
        {
            t.position += new Vector3(0, 1, 0);
        }
        //zooming in
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && t.position.y > minCameraHeight)
        {
            t.position -= new Vector3(0, 1, 0);
        }
        if (Input.GetMouseButtonDown(0))
        {
            //deselect everything when the button is pressed
            foreach (SecurityGuard unit in SelectableUnits)
            {
                unit.SendMessage("UnSelected", SendMessageOptions.DontRequireReceiver);
            }

            //if a guard gets clicked select that one
            Ray ray = this.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                hit.transform.gameObject.SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
            }
        }
        //make the selected guard move towards the point
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 clickPos = new Vector3(0, 0, 0);
            Ray ray = this.camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                clickPos = hit.point;
            }
            foreach (SecurityGuard unit in SelectableUnits)
            {
                SecurityGuard guard = unit.GetComponent<SecurityGuard>();
                if (guard.isSelected)
                {
                    NavMeshAgent navAgent = unit.GetComponent<NavMeshAgent>();
                    navAgent.SetDestination(clickPos);
                }
            }
        }
        //TODO: display the numbers above guards
        for (int i = 0; i < SelectableUnits.Length; i++)
        {
            int tempInt = i + 1;
            if (Input.GetKey(tempInt.ToString()))
            {
                DeselectGuards();
                SelectableUnits[i].SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
            }
        }
        //move the camera
        if (Input.GetKey(KeyCode.W))
        {
            t.position += Vector3.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            t.position -= Vector3.forward * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            t.position -= Vector3.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            t.position += Vector3.right * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveSpeed = baseSpeed + bonusSpeed;
        }
        else
        {
            moveSpeed = baseSpeed;
        }
	}

    void DeselectGuards()
    {
        foreach (SecurityGuard unit in SelectableUnits)
        {
            unit.SendMessage("UnSelected", SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnGUI()
    {
        foreach (SecurityGuard unit in SelectableUnits)
        {
            if(unit.SeeingPlayer)
            {
                if (camera)
                { 
                    Vector3 screenPos = camera.WorldToScreenPoint(unit.transform.position);
                    Vector2 normalized  = new Vector2(screenPos.x - Screen.width / 2, Screen.height / 2 - screenPos.y);
                    float distance = 256;
                    if (Vector3.Distance(screenPos, new Vector3(Screen.width / 2, Screen.height / 2, 0)) > 256)
                    {
                        normalized.Normalize();
                        float angle = (Mathf.Rad2Deg * Mathf.Atan2(normalized.y, normalized.x) + 720 +90)%360;
                        Rect screenRect = new Rect();
                        screenRect.x = Screen.width  / 2 + (normalized.x * distance);
                        screenRect.y = Screen.height / 2 + (normalized.y * distance);
                            
                        screenRect.width = 64;
                        screenRect.height = 64;

                        Matrix4x4 matrixBackup = GUI.matrix;
                        GUIUtility.RotateAroundPivot(angle, new Vector2(screenRect.x,screenRect.y));

                        screenRect.x -= 32;
                        screenRect.y -= 32;

                        GUI.DrawTexture(screenRect, AssetPool.testTexture);
                        GUI.matrix = matrixBackup;
                    }
                }
            }
        }
    }
}
