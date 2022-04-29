using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

public class SmoothMovementController : MonoBehaviour
{
    public float speed = 1;
    public XRNode inputSource;
    public float gravity = -9.81f;
    public LayerMask groundLayer;
    public float additionalHeight = 0.2f;
    public Collider tableCollider;

    private XROrigin origin;
    private Vector2 inputAxis;
    private CharacterController character;
    private float fallingSpeed = 0f;
    private float colliderThreshold = 100.0f;

    void Start()
    {
        character = GetComponent<CharacterController>();
        origin = GetComponent<XROrigin>();
    }

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);
    }

    void FixedUpdate()
    {
        CapsuleFollowHeadset();

        // Move in direction we are facing
        Quaternion headYaw = Quaternion.Euler(0, origin.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, 0, inputAxis.y);
        CollisionFlags flags = character.Move(direction * Time.fixedDeltaTime * speed);

        // Add effect of gravity
        if (IsGrounded())
            fallingSpeed = 0f;
        else
            fallingSpeed += gravity * Time.fixedDeltaTime;
        character.Move(Vector3.up * fallingSpeed * Time.fixedDeltaTime);
    }

    void CapsuleFollowHeadset()
    {
        // Make the capsule collider follow our headset during movement so collisions work properly
        character.height = origin.CameraInOriginSpaceHeight + additionalHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(origin.Camera.gameObject.transform.position);
        Vector3 newCenter = new Vector3(capsuleCenter.x, character.height / 2 + character.skinWidth, capsuleCenter.z);

        // If our headset is moved into the circuit table collider, don't move the character controller. 
        // This avoids the player suddenly standing on the table when leaning in for a closer look.
        Vector3 newWorldCenter = transform.TransformPoint(newCenter);
        Vector3 closest = tableCollider.ClosestPoint(newWorldCenter);
        float diff = Mathf.Abs(closest.sqrMagnitude - newWorldCenter.sqrMagnitude);
        if (diff > colliderThreshold)
        {
            character.center = newCenter;
        }
    }

    bool IsGrounded()
    {
        // Find out if we are standing on solid ground
        Vector3 rayStart = transform.TransformPoint(character.center);
        float rayLength = character.center.y + 0.01f;
        RaycastHit hitInfo;
        bool hasHit = Physics.SphereCast(rayStart, character.radius, Vector3.down, out hitInfo, rayLength, groundLayer);
        return hasHit;
    }
}
