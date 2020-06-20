using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class ARLessons : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text StateText;

    [SerializeField] TMPro.TMP_Text PlaneText;

    [SerializeField] ARPlaneManager mArPlaneManager;

    [SerializeField] ARPointCloudManager mArPointCloudManager;

    ARRaycastManager aRRaycastManager;

    [SerializeField] GameObject robot;

    private GameObject go = null;

    [SerializeField] ARCameraManager aRCameraManager;

    [SerializeField] Light mLight;

    [SerializeField] Image image;

    [SerializeField] GameObject ballPrefab;

    int numPlanesAdded = 0, numPlanesUpdated = 0, numPlanesRemoved = 0, numPointsUpdated = 0;

    void Start()
    {
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();

        mArPlaneManager.planesChanged += onPlanesChanged;

        mArPointCloudManager.pointCloudsChanged += onPointCloudsChanged;

        ARSession.stateChanged += Change;

        aRCameraManager.frameReceived += onCameraFrameChanged;

    }

    void onCameraFrameChanged(ARCameraFrameEventArgs args)
    {
        if(args.lightEstimation.averageBrightness.HasValue)
        {
            mLight.intensity = args.lightEstimation.averageBrightness.Value;
        }

        if(args.lightEstimation.averageColorTemperature.HasValue)
        {
            mLight.colorTemperature = args.lightEstimation.averageColorTemperature.Value;
        }

        if(args.lightEstimation.colorCorrection.HasValue)
        {
            image.color = args.lightEstimation.colorCorrection.Value;
            mLight.color = args.lightEstimation.colorCorrection.Value;
        }
    }

    void Change(ARSessionStateChangedEventArgs e)
    {
        print("state changed to : " + e.state);
        StateText.SetText(e.state.ToString());
    }

    void onPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach(var planesAdded in args.added)
        {
            numPlanesAdded += 1;
        }

        foreach(var planesUpdated in args.updated)
        {
            numPlanesUpdated += 1;
        }

        foreach(var planesRemoved in args.removed)
        {
            numPlanesRemoved += 1;
        }
    }

    void onPointCloudsChanged(ARPointCloudChangedEventArgs args)
    {
        foreach(var pointCloudUpdated in args.updated)
        {
            numPointsUpdated += 1;
        }
    }

    private void Update()
    {
        PlaneText.SetText("Planes Added = " + numPlanesAdded + " Planes Updated = " + numPlanesUpdated + " Planes Removed = " + numPlanesRemoved + " Point Clouds Updated = " + numPointsUpdated);

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            var hits = new List<ARRaycastHit>();

            aRRaycastManager.Raycast(ray, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

            if(hits.Count > 0)
            {
                var pos = hits[0].pose.position;

                if(go != null)
                {
                    go.gameObject.transform.position = pos;
                }
                else
                {
                     go = Instantiate(robot, pos, Quaternion.identity) as GameObject;

                    Rigidbody rigidbody = go.GetComponent<Rigidbody>();

                    rigidbody.isKinematic = true;
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }
            
        }
    }

    public void ShootBall()
    {
        GameObject newBall = Instantiate<GameObject>(ballPrefab);
        newBall.transform.position = Camera.main.transform.position;
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        rb.AddForce(5000 * Camera.main.transform.forward);
    }
}
