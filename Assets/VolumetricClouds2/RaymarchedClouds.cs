using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RaymarchedClouds : MonoBehaviour
{
    public bool attachToEditorCam;
    public bool hidePlane = true;
    public Material materialUsed;

    RaymarchedClouds editorCamScript;
    GameObject plane;
    MeshRenderer mRenderer;
    Camera thisCam;

    void Start()
    {
        if (materialUsed != null)
            Loop();
        else if(Application.isPlaying)
            Debug.LogError("Object " + gameObject.name + " has a RaymarchedClouds attached to it but it's specified material is null");
    }

    void OnEnable()
    {
        if(plane)
            plane.SetActive(true);
    }

    void OnDisable()
    {
        if(plane)
            plane.SetActive(false);
    }

    void OnDestroy()
    {
        DestroySafe(plane);
        DestroySafe(editorCamScript);
    }

    void OnPreCull()
    {
        if (materialUsed == null)
            return;
        Loop();
        mRenderer.enabled = true;
        mRenderer.sharedMaterial.SetMatrix("_ToWorldMatrix", thisCam.cameraToWorldMatrix);
    }

    void OnPostRender()
    {
        if(mRenderer)
            mRenderer.enabled = false;
    }

    void OnDrawGizmos()
    {
        if(materialUsed == null)
            return;
        VerifEditorCam();
        Loop();
    }

    void Loop()
    {
        if (materialUsed == null)
            return;
        VerifCam();
        VerifPlane();
        FitPlane();
    }

    void VerifCam()
    {
        if (thisCam == null)
            thisCam = GetComponent<Camera>();
    }

    void DestroySafe(Object o)
    {
        if (o == null)
            return;
        if (Application.isPlaying)
            Destroy(o);
        else
            DestroyImmediate(o);
    }

    // Uses a quad instead of Graphics.Blit to conform to render queue
    void VerifPlane()
    {
        if (plane == null)
        {
            foreach (Transform child in thisCam.transform.GetComponentsInChildren<Transform>().Where(child => child.name == "VolumetricCloudRenderer"))
                plane = child.gameObject;

            if (plane == null)
                CreatePlane();
        }
        if (mRenderer == null)
            mRenderer = plane.GetComponent<MeshRenderer>();
        if (mRenderer.sharedMaterial != materialUsed)
            mRenderer.sharedMaterial = materialUsed;

        plane.hideFlags = hidePlane ? HideFlags.HideAndDontSave : HideFlags.None;

        mRenderer.enabled = false;
    }

    public bool IsEditorCamera()
    {
        return gameObject.name == "SceneCamera";
    }

    void CreatePlane()
    {
        plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "VolumetricCloudRenderer";
        MeshCollider collider = plane.GetComponent<MeshCollider>();
        DestroySafe(collider);
    }

    void FitPlane()
    {
        float epsilon = 0.01f;//Mathf.Epsilon;
        float zOffset = thisCam.nearClipPlane+epsilon;
        float frustumHeight = 2.0f * zOffset * Mathf.Tan(thisCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * thisCam.aspect;
        plane.transform.parent = thisCam.transform;
        plane.transform.localPosition = new Vector3(0.0f, 0.0f, zOffset);
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = new Vector3(frustumWidth + epsilon, frustumHeight + epsilon, 1.0f);
    }



    void VerifEditorCam()
    {
        if (editorCamScript == null && attachToEditorCam)
        {
            if (Camera.current.name == "SceneCamera")
            {
                editorCamScript = Camera.current.gameObject.GetComponent<RaymarchedClouds>();
                if (editorCamScript == null)
                    editorCamScript = Camera.current.gameObject.AddComponent<RaymarchedClouds>();
                editorCamScript.materialUsed = materialUsed;
            }
        }
        else if (editorCamScript != null && !attachToEditorCam)
        {
            DestroySafe(editorCamScript);
        }
    }
}
