using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using System.Diagnostics;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaceHologram : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefabToPlace;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private static readonly List<ARRaycastHit> Hits = new();
    public UnityEngine.UI.Text Log;

    protected void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    protected void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    protected void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();
    }

    protected void Update()
    {
        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count < 1 || activeTouches[0].phase != TouchPhase.Began)
        {
            return;
        }

        const TrackableType trackableTypes =
            TrackableType.FeaturePoint |
            TrackableType.PlaneWithinPolygon;

        if (_raycastManager.Raycast(activeTouches[0].screenPosition, Hits, trackableTypes))
            CreateAnchor(Hits[0]);
    }


    private ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        ARAnchor anchor;

        if (hit.trackable is ARPlane hitPlane)
        {
            var oldPrefab = _anchorManager.anchorPrefab;
            _anchorManager.anchorPrefab = _prefabToPlace;
            anchor = _anchorManager.AttachAnchor(hitPlane, hit.pose);
            _anchorManager.anchorPrefab = oldPrefab;

        }
        else
        {
            var instantiatedObject = Instantiate(_prefabToPlace, hit.pose.position, hit.pose.rotation);

            if (!instantiatedObject.TryGetComponent<ARAnchor>(out anchor))
            {
                anchor = instantiatedObject.AddComponent<ARAnchor>();
            }
        }

        return anchor;
    }
}