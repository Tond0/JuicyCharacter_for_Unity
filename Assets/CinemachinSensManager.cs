using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

//Let's change sens to the pov!
public class CinemachinSensManager : CinemachineExtension
{
    //The camera
    [SerializeField] private CinemachineVirtualCamera _cam;
    //The cinemachine pov component
    private CinemachinePOV pov;
    private void Start()
    {
        //we get the pov component
        pov = _cam.GetCinemachineComponent<CinemachinePOV>();
    }

    /// <summary>
    /// Change sens!
    /// </summary>
    /// <param name="stringedSens"></param>
    public void ChangeSens(string stringedSens)
    {        
        //We parse the text into a float (can sometime return an error, nothing gamebreaking, it's just for let testing be more accessible)
        float newSens = float.Parse(stringedSens, CultureInfo.InvariantCulture.NumberFormat);

        //Apply new sens
        pov.m_VerticalAxis.m_MaxSpeed = newSens;
        pov.m_HorizontalAxis.m_MaxSpeed = newSens;
    }

    //We don't need any effect, we just want it to be an extension
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) { }
}
