using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SceneLoadSwap : MonoBehaviour
{
    [Header("# Declared in Inspector #")]
    public string OriginalSceneToBeReplaced = "";
    public string NewSceneName = "";

    public string SceneToBeDisabled = "";

    [Header("# Declared by Script #")]
    private List<LoadSceneAsyncOnEnter> listOfSceneLoaders = new List<LoadSceneAsyncOnEnter>();
    private LoadSceneAsyncOnEnter Bank;
    private LoadSceneAsyncOnEnter BankToDisable;
    private Toggle toggle;

    public UnityEvent WhenInsideScene;
    public UnityEvent WhenOutsideScene;

    private bool ListenersEnabled = false;

    void Awake() {
        listOfSceneLoaders.AddRange(GameObject.FindObjectsByType<LoadSceneAsyncOnEnter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        Bank = listOfSceneLoaders.Find(listOfSceneLoaders => listOfSceneLoaders.sceneToLoad == OriginalSceneToBeReplaced);
        BankToDisable = listOfSceneLoaders.Find(listOfSceneLoaders => listOfSceneLoaders.sceneToLoad == SceneToBeDisabled);

        if (Bank != null)
        {
            Bank.OnSceneLoaded.AddListener(OnSceneLoaded);
            Bank.OnSceneUnloaded.AddListener(OnSceneUnloaded);
            ListenersEnabled = true;
        }

        if (toggle == null) {
            toggle = GetComponent<Toggle>();
        }
    }

    private void OnDestroy()
    {
        if (!ListenersEnabled) {
            Bank.OnSceneLoaded.RemoveListener(OnSceneLoaded);
            Bank.OnSceneUnloaded.RemoveListener(OnSceneUnloaded);
        }
    }

    void OnSceneLoaded() 
    {
        WhenInsideScene.Invoke();
    }

    void OnSceneUnloaded()
    {
        WhenOutsideScene.Invoke();
    }

    public void SwapLoadScene() {
        if (Bank != null) {

            if (toggle.isOn)
            {
                Bank.SetSceneToLoad(NewSceneName);
                BankToDisable.gameObject.SetActive(false);
            }
            else
            {
                Bank.SetSceneToLoad(OriginalSceneToBeReplaced);
                BankToDisable.gameObject.SetActive(true);
            }
        }
    }
}
