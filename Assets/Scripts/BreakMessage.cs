using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakMessage : MonoBehaviour

{
    public GameObject thisObject;


    // Start is called before the first frame update
    void Start()
    {
        thisObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowMessage()
    {
        thisObject.SetActive(true);
    }

    public void ConfirmPress()
    {
        thisObject.SetActive(false);
    }





}
