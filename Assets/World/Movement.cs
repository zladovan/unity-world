using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Transform mover;
    public float speed = 30f;
    public Joystick Joystick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var input = new Vector2(
            Input.GetAxis("Horizontal") + Joystick.Horizontal, 
            Input.GetAxis("Vertical") + Joystick.Vertical
        );
        mover.Rotate(Vector3.up * input.x * speed * Time.deltaTime);
        mover.Translate(Vector3.forward * input.y * speed * Time.deltaTime);
    }

}    