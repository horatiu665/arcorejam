// this file was made to help move the code from the ToothpickPlacerTest into new clean classes that use the Har system
// all was moved except rotation. to completely obsolete the toothpick placer test,
// implement rotation and phys movement in a new clean script that uses the new toothpick placer built with Har.

//using System;
//using System.Collections.Generic;
//using GoogleARCore;
//using UnityEngine;

//#if UNITY_EDITOR
//// Set up touch input propagation while using Instant Preview in the editor.
//using Input = GoogleARCore.InstantPreviewInput;
//#endif

///// <summary>
///// Controls the HelloAR example.
///// </summary>
//public class ToothpickPlacerTest2 : MonoBehaviour
//{
//    [Header("Rotation settings")]
//    public Vector2 rotationSpeed = new Vector2(1, 1);
//    public float maxDeltaTouchForRotation = 100f;
//    private bool scaleMode = false;
//    public AnimationCurve rotationMapping = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) } };
//    public float rotationForceMulti = 10f;

//    [SerializeField]
//    private GameObject _rotationDummy;
//    public GameObject rotationDummy
//    {
//        get
//        {
//            if (_rotationDummy == null)
//            {
//                _rotationDummy = new GameObject("[RotationDummy]");
//            }
//            return _rotationDummy;
//        }
//    }

//    [SerializeField]
//    private GameObject _rotationDummyChild;
//    public GameObject rotationDummyChild
//    {
//        get
//        {
//            if (_rotationDummyChild == null)
//            {
//                _rotationDummyChild = new GameObject("[RotationDummyChild]");
//            }
//            return _rotationDummyChild;
//        }
//    }

//    // trigger rotation when touching and selected...
//    private void FixedUpdate()
//    {
//        if (curTouching)
//        {
//            if (selection.Count > 0)
//                Rotate1();
//        }
//    }

//    // the rotation shit, that depends on the prev. touch, and swipy input xy
//    private void Rotate1()
//    {
//        // do rotation shit...
//        var delta = curTouchPos - prevTouchPos;
//        var maxDelta = maxDeltaTouchForRotation * 1080 * Mathf.Min(Screen.width, Screen.height);
//        var rotationInput = new Vector2(
//            rotationMapping.Evaluate(Mathf.Abs(delta.x) / maxDeltaTouchForRotation) * Mathf.Sign(delta.x) * rotationSpeed.x,
//            rotationMapping.Evaluate(Mathf.Abs(delta.y) / maxDeltaTouchForRotation) * Mathf.Sign(delta.y) * rotationSpeed.y);

//        foreach (var s in selection)
//        {
//            var dummy = rotationDummy.transform;
//            var dummyChild = rotationDummyChild.transform;
//            dummy.position = s.transform.position;
//            dummy.rotation = mainCamera.transform.rotation;
//            dummyChild.position = s.transform.position;
//            dummyChild.rotation = s.transform.rotation;
//            dummyChild.SetParent(dummy);
//            var initRot = dummy.rotation;
//            dummy.Rotate(new Vector3(rotationInput.y, 0, 0), Space.Self);
//            //dummy.Rotate(mainCamera.transform.right, rotationInput.y);
//            dummy.Rotate(Vector3.up, rotationInput.x);

//            dummyChild.SetParent(null);

//            RotateRigidbody(s.GetComponent<Rigidbody>(), dummyChild.rotation, s.transform.rotation);

//        }
//    }

//    // Sets the angular velocity of the rigidbody.
//    private void RotateRigidbody(Rigidbody r, Quaternion finalRot, Quaternion curRot)
//    {
//        float angle;
//        Vector3 axis;
//        // Get the delta between the control transform rotation and the rigidbody.
//        Quaternion rigidbodyRotationDelta = finalRot *
//                                            Quaternion.Inverse(curRot);
//        // Convert this rotation delta to values that can be assigned to rigidbody
//        // angular velocity.
//        rigidbodyRotationDelta.ToAngleAxis(out angle, out axis);
//        // Set the angular velocity of the rigidbody so it rotates towards the
//        // control transform.
//        float timeStep = Mathf.Clamp01(Time.fixedDeltaTime * 8f * rotationForceMulti);
//        r.angularVelocity = timeStep * angle * axis;
//    }

//    private Vector2 debug_lastTouchPos;
//    private bool curTouching;
//    //private void OnGUI()
//    //{
//    //    //GUI.Label(new Rect(10, 10, 300, 100), debug_lastTouchPos.ToString());
//    //}
//}
