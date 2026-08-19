////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using System.Collections;
using UnityEngine;

public class MoveParticlesAndPause : MonoBehaviour
{
    public GameObject targetPos;

    #region private variables
    private Vector3 startPos;
    private Rigidbody rb;
    #endregion

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
        StartCoroutine(MoveAndPause());
    }

    IEnumerator MoveAndPause()
    {
        rb.linearVelocity = (targetPos.transform.position - startPos);

        yield return new WaitUntil(() => ((transform.position - startPos).magnitude >= (targetPos.transform.position - startPos).magnitude));

        rb.linearVelocity = Vector3.zero;

        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in particleSystems)
        {
            p.Pause();
        }
    }
}
