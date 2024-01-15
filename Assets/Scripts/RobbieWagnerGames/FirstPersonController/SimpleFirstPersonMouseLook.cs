using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames.FirstPerson
{
    public class SimpleFirstPersonMouseLook : MonoBehaviour
    {

        [SerializeField] private float mouseSensitivity = 500f;
        [SerializeField] private Transform playerBody;

        private float xRotation = 0f;

        // Start is called before the first frame update
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}