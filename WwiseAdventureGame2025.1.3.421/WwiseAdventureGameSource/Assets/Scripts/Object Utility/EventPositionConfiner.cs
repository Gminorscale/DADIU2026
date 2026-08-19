////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EventPositionConfiner : MonoBehaviour
{
    [Header("Event to clamp to AkAudioListener")]
    public AK.Wwise.Event Event = new AK.Wwise.Event();

    [Header("Settings")]
    public float UpdateInterval = 0.05f;
    public bool isAkRoomAware = false;

    #region private variables
    private IEnumerator positionClamperRoutine;

    private Collider trigger;
    public Transform targetTransform;

    private GameObject eventEmitter;
    #endregion

    private void Awake()
    {
        trigger = GetComponent<Collider>();
        trigger.isTrigger = true;

        eventEmitter = new GameObject("Clamped Emitter");
        eventEmitter.transform.parent = transform;
        Rigidbody RB = eventEmitter.AddComponent<Rigidbody>();
        RB.isKinematic = true;
        SphereCollider SPC = eventEmitter.AddComponent<SphereCollider>();
        SPC.isTrigger = true;
        eventEmitter.AddComponent<AkGameObj>();
        if (isAkRoomAware) {
            eventEmitter.AddComponent<AkRoomAwareObject>();
        }

        //InputManager.OnControlChange += OnControlChange;
        AkListenerDistanceProbeChanger.OnListenerProbeChange += ChangeListener;
    }
    private void ChangeListener(AkGameObj AkGo) {
        targetTransform = AkGo.transform;
    }

    private void OnEnable()
    {
        
        var listenerGameObject = PlayerManager.Instance.cameraScript.GetComponent<AkListenerDistanceProbe>().distanceProbe;

        if (listenerGameObject != null)
        {
            targetTransform = listenerGameObject.transform;
        }
        else
        {
            Debug.LogError(this + ": No listener probe found on "+this.name+"! Aborting.");
            enabled = false;
        }

        Event.Post(eventEmitter);

        positionClamperRoutine = ClampEmitterPosition();
        StartCoroutine(positionClamperRoutine);
    }

    private void OnDisable()
    {
        Event.Stop(eventEmitter);

        if(positionClamperRoutine != null)
        {
            StopCoroutine(positionClamperRoutine);
        }
    }

    IEnumerator ClampEmitterPosition()
    {
        while (true)
        {
            Vector3 closestPoint = trigger.ClosestPoint(targetTransform.position);
            eventEmitter.transform.position = closestPoint;

            yield return new WaitForSecondsRealtime(UpdateInterval);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (eventEmitter != null)
        {
            Gizmos.DrawSphere(eventEmitter.transform.position, 0.2f);
        }
    }

}
