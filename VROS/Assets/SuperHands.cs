﻿using UnityEngine;
using System.Collections;
using Leap;

public class SuperHands : MonoBehaviour
{
    public Hand hand;
    // Use this for initialization
    void Start()
    {


    }

    Schmitt grab = new Schmitt();

    public float grabTresh = 0.95f;
    public float letgoTresh = 0.05f;
    public SuperGrab holding;
    public GameObject lightSaber;
    // Update is called once per frame
    void Update()
    {
        grab.treshH = grabTresh;
        grab.treshL = letgoTresh;

        var handModel = GetComponent<HandModel>();
        if (handModel == null)
            return;

        hand = GetComponent<HandModel>().GetLeapHand();

        if (hand == null)
            return;

        var palmPos = hand.PalmPosition.ToVector3();
        var thumb = hand.Fingers[0];
        if(thumb != null) palmPos = thumb.TipPosition.ToVector3();

        var direction = (palmPos - Camera.main.transform.position).normalized;

       grab.N(hand.PinchStrength);

        if (holding == null)
        {
            RaycastHit hover;
            if (!Physics.SphereCast(palmPos, 0.1f, direction, out hover, 10f))
                return;
            var superGrab = hover.transform.GetComponent<SuperGrab>();
            if (superGrab == null)
                return;
            

            if (grab.raisingEdge)
            {
                holding = superGrab;
                superGrab.Grab();
                //print("Grab" + holding.name);
            }

            //if(hand.GrabStrength> grabTresh)
            //{
            //    lightSaber.SetActive(true);
            //}
            //else
            //    lightSaber.SetActive(false);
        }
        else
        {
            if (grab.fallingEdge)
            {
                holding.Drop();
                //print("Letgo" + holding.name);
                holding = null;
            }
        }



        if (holding != null)
        {
            var handDist = Vector3.Distance(palmPos, Camera.main.transform.position);
            //print(handDist);
            var normalizedHandDist = Mathf.InverseLerp(.2f, .5f, handDist);

            holding.transform.position = palmPos + direction * Mathf.Lerp(minHoldDist,maxHoldDist, normalizedHandDist);
            holding.transform.forward = direction;
        }
    }
    public float minHoldDist = 1;
    public float maxHoldDist = 3;
}

class Schmitt
{
    public float treshH;
    public float treshL;
    public bool status;
    public bool raisingEdge;
    public bool fallingEdge;

    public bool N(float value)
    {
        raisingEdge = !status && value > treshH;
        fallingEdge = status && value < treshL;

        status ^= raisingEdge | fallingEdge;

        return status;
    }
}