using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AfterImage : MonoBehaviour
{
    [HideInInspector] public bool showAfterImage;
    [SerializeField] private Transform container;
    [SerializeField] private float delay;
    [SerializeField] private string fadeProperty = "_Fade";
    [SerializeField] private GameObject presetObj;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private int fadeMaterialIndex;
    
    private Renderer[] renderers;
    private float fadeSpeed = 3f;
    private float[] fadeTimers;
    private float timer;
    private SkinnedMeshRenderer[] skinRenderers;
    private MeshRenderer[] meshRenderers;
    private MaterialPropertyBlock props;
    private Matrix4x4 matrix;
    private CombineInstance[] combine;
    private List<GameObject> objectPool;
    private MeshFilter[] poolMeshFilters;
    private MeshFilter[] meshFilters;


    private void Awake()
    {
        SetUpRenderers();

        // create a pool for all the objects for the trail
        objectPool = new List<GameObject>();
        poolMeshFilters = new MeshFilter[poolSize];
        renderers = new Renderer[poolSize];
        props = new MaterialPropertyBlock();
        fadeTimers = new float[poolSize];

        // instantiate the pool of objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(presetObj, container);
            poolMeshFilters[i] = obj.GetComponent<MeshFilter>();
            renderers[i] = obj.GetComponent<Renderer>();
            // renderers[i].material = dodgeMat;
            obj.SetActive(false);
            objectPool.Add(obj);
        }
    }

    private void SetUpRenderers()
    {
        // get the skinned mesh renderers
        skinRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        // get normal mesh renderers and their filters
        meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
        meshFilters = new MeshFilter[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshFilters[i] = meshRenderers[i].GetComponent<MeshFilter>();
        }
        // create combineinstances for every mesh we need to grab
        combine = new CombineInstance[skinRenderers.Length + meshRenderers.Length];
    }



    private void Update()
    {
        if (Time.timeScale == 0f) return;
        timer -= Time.deltaTime;

        // only create image if moving and timer is down
        if (timer < 0 && rb.velocity.magnitude > 0.1f && showAfterImage)
        {
            timer = delay;
            CreateAfterImage();
        }

        // fade the property block
        for (int i = 0; i < poolSize; i++)
        {
            fadeTimers[i] -= Time.deltaTime * fadeSpeed;
            renderers[i].GetPropertyBlock(props, fadeMaterialIndex);
            props.SetFloat(fadeProperty, fadeTimers[i]);
            renderers[i].SetPropertyBlock(props, fadeMaterialIndex);

        }

    }



    // get a gameobject from the pool, and its index
    public (GameObject, int) GetPooledObject()
    {
        for (int i = 0; i < objectPool.Count; i++)
        {
            if (!objectPool[i].activeInHierarchy)
            {
                return (objectPool[i], i);
            }
        }
        return (null, -1);
    }

    private void CreateAfterImage()
    {
        // grab a pooled object
        (GameObject, int) obj = GetPooledObject();
        // if no object to assign, return
        if (obj.Item1 == null)
        {
            return;
        }
        // current transform matrix
        matrix = transform.worldToLocalMatrix;
        //  create mesh snapshot for all skinned meshes       
        for (int i = 0; i < skinRenderers.Length; i++)
        {
            Mesh mesh = new();
            skinRenderers[i].BakeMesh(mesh);
            combine[i].mesh = mesh;
            combine[i].transform = matrix * skinRenderers[i].localToWorldMatrix;
        }
        // also add normal meshes
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            combine[skinRenderers.Length + i].mesh = meshFilters[i].sharedMesh;
            combine[skinRenderers.Length + i].transform = matrix * meshRenderers[i].transform.localToWorldMatrix;
        }

        // set property block
        fadeTimers[obj.Item2] = 1f;
        renderers[obj.Item2].GetPropertyBlock(props, fadeMaterialIndex);
        props.SetFloat(fadeProperty, fadeTimers[obj.Item2]);
        renderers[obj.Item2].SetPropertyBlock(props, fadeMaterialIndex);

        // combine meshes into the right instance
        poolMeshFilters[obj.Item2].mesh.CombineMeshes(combine);

        // set object to transform and active
        obj.Item1.transform.position = transform.position;
        obj.Item1.transform.rotation = transform.rotation;
        obj.Item1.SetActive(true);

        StartCoroutine(DisableClone(obj.Item1));

    }

    public void ClearClones()
    {
        StopAllCoroutines();
        foreach(Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator DisableClone(GameObject afterImage)
    {
        yield return new WaitForSeconds(2f);
        afterImage.SetActive(false);
    }

}