////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public string GameScene;

    public void LoadGame()
    {
        StartCoroutine(LoadScene(GameScene));
        AkUnitySoundEngine.StopAll();
    }

    private IEnumerator LoadScene(string name)
    {
        yield return null;
        SceneManager.LoadScene(GameScene);
    }

}
