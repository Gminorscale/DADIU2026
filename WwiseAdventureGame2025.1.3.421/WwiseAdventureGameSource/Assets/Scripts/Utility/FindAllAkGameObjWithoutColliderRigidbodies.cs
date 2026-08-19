////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindAllAkGameObjWithoutColliderRigidbodies : MonoBehaviour {

	void Start ()
    {
        AkGameObj[] Missings = FindObjectsByType<AkGameObj>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < Missings.Length; i++)
        {
            if (Missings[i].gameObject.GetComponent<Collider>() == null)
            {
                print(Missings[i].name + " missing collider.");
            }
            if (Missings[i].gameObject.GetComponent<Rigidbody>() == null)
            {
                print(Missings[i].name + " missing rigidbody.");
            }
        }
    }

}
