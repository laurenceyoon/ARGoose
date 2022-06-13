using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goose : MonoBehaviour
{
    public FMOD.Studio.EventInstance instance;
    public float sizeScale;
    public float pitchScale;  // 0 ~ 1 사이 
    public Vector3 originalSize;
    public string choir;

    public void Init(FMOD.Studio.EventInstance fmodInstance, string gooseName)
    {
        instance = fmodInstance;
        instance.start();
        instance.setPaused(true);
        sizeScale = 1;
        originalSize = transform.localScale;
        choir = gooseName;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(this.gameObject));
        instance.setParameterByName("Pitch", pitchScale);
    }

    public void play()
    {
        instance.setPaused(false);
    }
}
