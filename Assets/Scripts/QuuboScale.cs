using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuuboScale : MonoBehaviour
{


    [Header("Scale Size")]
    [SerializeField] private float scale = 2.0f;




    // Start is called before the first frame update
    void Start()
    {
        transform.localScale *= scale;
    }
}
