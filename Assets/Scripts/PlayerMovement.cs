﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Car;

namespace Com.Geo.Respect
{
    public class PlayerMovement : MonoBehaviour
    {
        //public Interactable focus; 

        public float speed = 5f, rotationSpeed = 450f, jumpForce = 2.0f;

        private Vector3 moveVelocity, moveInput, jump;
        private Quaternion targetRotation;
        private Rigidbody rigidBody;
        public bool canMove = true, isInCar = false, isGrounded = true;
        public GameObject playerGraphics, interactingObject;
        private Camera gameCam;


        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            gameCam = Camera.main;
            jump = new Vector3(0.0f, 2.0f, 0.0f);
        }

        void Update()
        {

            moveInput = new Vector3(CrossPlatformInputManager.GetAxisRaw("Horizontal"), 0, CrossPlatformInputManager.GetAxisRaw("Vertical"));
            moveVelocity = moveInput.normalized * speed;

            if (CrossPlatformInputManager.GetButtonDown("Jump") && isGrounded)
            {
                rigidBody.AddForce(jump * jumpForce, ForceMode.Impulse);
            }

            /*//If we press middle mouse button
            if (Input.GetMouseButtonDown(2))
            {
                RaycastHit hit;
                Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);

                if(Physics.Raycast(ray, out hit))
                {
                    //Stop focusing any objects
                    RemoveFocus();
                   
                }
            }

            //If we press right mouse button
            if (Input.GetMouseButtonDown(1))
            {
                //Then we create a ray
                Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, 100))
                {
                    //Check if we hit an interactable
                    Interactable interactable = hit.collider.GetComponent<Interactable>();
                    
                    //If we did, set it as our focus
                    if(interactable != null)
                    {
                        SetFocus(interactable);
                    }
                }
            }*/
        }

        /*void SetFocus(Interactable newFocus)
        {
            focus = newFocus;
        }

        void RemoveFocus()
        {
            focus = null;
        }*/


        private void FixedUpdate()
        {
            //Update movement
            if (canMove)
            {
                rigidBody.MovePosition(rigidBody.position + moveVelocity * Time.fixedDeltaTime);

                if (moveInput != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(moveInput);
                    transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.fixedDeltaTime);
                }
            }
            else if (!canMove && isInCar)
            {
                this.transform.position = interactingObject.transform.position;

                if (CrossPlatformInputManager.GetButtonDown("Use1"))
                {
                    StartCoroutine(ExitCar(interactingObject));
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {

            //ENTER/LEAVE CAR
            if (other.tag == "Car")
            {
                if (!isInCar)
                {
                    interactingObject = other.gameObject;
                }

                //IF WE WANT TO ENTER CAR
                if (canMove && !isInCar)
                {
                    if (CrossPlatformInputManager.GetButtonDown("Use1"))
                    {
                        //Activate Car controls
                        //                  other.GetComponent<CarController>().isControlledByPlayer = true;
                        other.GetComponent<CarMovement>().isControlledByPlayer = true;
                        //                  other.tag = "Player";

                        //Deactivate Playermovement and set car as parent
                        canMove = false;
                        isInCar = true;
                        rigidBody.isKinematic = true;
                        //this.GetComponent<PlayerMovement>().enabled = false;
                        this.transform.SetParent(other.transform);

                        //Deactivate Playervisuals (and shooting)
                        playerGraphics.SetActive(false);

                        //Update camera target
                        gameCam.GetComponent<TransformFollower>().target = other.transform;

                        if (other.GetComponent<EnemyPatrol>() != null)
                        {
                            other.GetComponent<EnemyPatrol>().enabled = false;
                        }

                        Debug.Log("Just ENTERED the car!");
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //ENTER/LEAVE CAR
            if (other.tag == "Car" && !isInCar)
            {
                interactingObject = null;
            }
        }


        IEnumerator ExitCar(GameObject objectToExit)
        {
            //Turn off Car controls
            //      objectToExit.GetComponent<CarController>().isControlledByPlayer = false;
            objectToExit.GetComponent<CarMovement>().isControlledByPlayer = false;
            //      objectToExit.GetComponent<CarUserControl>().enabled = false;
            objectToExit.tag = "Car";

            //Change Camera back to normal
            Camera.main.fieldOfView = 60;

            //this.GetComponent<PlayerMovement>().enabled = false;
            this.transform.SetParent(null);
            this.transform.position = objectToExit.GetComponent<CarMovement>().spawnPosition.transform.position;

            //Deactivate Playervisuals (and shooting)
            playerGraphics.SetActive(true);

            canMove = true;

            //Update camera target
            gameCam.GetComponent<TransformFollower>().target = this.transform;

            yield return new WaitForSeconds(0.2f);

            //Deactivate Playermovement and set car as parent
            isInCar = false;
            rigidBody.isKinematic = false;



            Debug.Log("Just EXITED the car!");

        }
    }
}