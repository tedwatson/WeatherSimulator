using UnityEngine;
using System.Collections;

public class DragObject : MonoBehaviour
{
    public TimeController timeController;

    private float distance;
    private Vector3 startPosition;
    private Transform target = null;
    private Color originalColor = Color.clear;
    public Color selectedColor;
    public float vel;

    public float getDistance()
    {
        return distance;
    }

    private void Start()
    {
        startPosition = new Vector3(0, 0, 0);
        distance = 0;
    }

    public void Update()
    {
        // Test if Left button was pressed and there was
        // no object selected
        if (Input.GetMouseButton(0) && target == null)
        {
            // Cast a Ray from Camera Position
            Vector3 v = new Vector3(Input.mousePosition.x,
            Input.mousePosition.y, 0);
            Ray ray = Camera.main.ScreenPointToRay(v);
            // This object will store the closest object
            // in front of the camera
            RaycastHit hit;

            // Test if there’s something in front of the camera
            if (Physics.Raycast(ray, out hit))
            {
                // Hide the Cursor
                Cursor.visible = false;
                target = hit.transform;

                timeController.setIsHoldingCube(true);

                if (target.GetComponent<Renderer>().material != null)
                {
                    if (originalColor == Color.clear)
                        originalColor = target.GetComponent<Renderer>().material.color;
                    target.GetComponent<Renderer>().material.color = selectedColor;
                }
            }
        }
        // Reset the target if Left button was released
        else if (Input.GetMouseButtonUp(0) && target != null)
        {
            // Show Cursor again
            Cursor.visible = true;
            if (target.GetComponent<Renderer>().material != null)
            {
                target.GetComponent<Renderer>().material.color = originalColor;
                originalColor = Color.clear;
                target.position = startPosition;
                distance = 0;
                timeController.setIsHoldingCube(false);
                timeController.droppedCube();
            }
            target = null;
        }
        // Move the target
        if (target != null)
        {
            Vector3 moved = new Vector3(Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"), 0);
            target.position += moved;
            distance = Vector3.Distance(target.position, startPosition);
        }
    }
}