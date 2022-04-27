using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationController : MonoBehaviour
{
    public XRController rightTeleportRay;
    public InputHelpers.Button teleportActivationButton;
    public GameObject reticle;

    void Update()
    {
        if (rightTeleportRay)
        {
            bool isActive = CheckIfActivated(rightTeleportRay);
            rightTeleportRay.gameObject.SetActive(isActive);
            reticle.SetActive(isActive);
        }
    }

    public bool CheckIfActivated(XRController controller)
    {
        bool isActivated = false;
        InputHelpers.IsPressed(controller.inputDevice, teleportActivationButton, out isActivated);
        return isActivated;
    }
}
