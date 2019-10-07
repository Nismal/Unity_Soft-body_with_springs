using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class softBodyGenerator : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool gravity = true;


    [SerializeField] private int numOfBorderPoints = 3;

    [SerializeField] private float radius;

    [SerializeField] private float centerMass;
    [SerializeField] private float borderMass;

    [SerializeField] private float borderSpringsForce;
    [SerializeField] private float borderSpringsDamp;

    [SerializeField] private float radialSpringsFoce;
    [SerializeField] private float radialSpringsDamp;

    [SerializeField] private Mesh pointMesh;

    [SerializeField] private bool doubleSkin = true;

    [SerializeField] private float radius2;
    [SerializeField] private float border2Mass;

    [SerializeField] private float border2SpringsForce;
    [SerializeField] private float border2SpringsDamp;

    [SerializeField] private float radial2SpringsForce;
    [SerializeField] private float radial2SpringsDamp;


    [SerializeField] private float border2ToBorder1SpringForce;
    [SerializeField] private float border2ToBorder1SpringDamp;


    private GameObject pointPrototipe;

    private GameObject centerPoint;

    private GameObject[] radialpoints;

    private GameObject[] radial2points;

    MeshFilter MF;

    // Start is called before the first frame update
    void Start()
    {
        //Create prototype

        pointPrototipe = new GameObject();

        Rigidbody pointP_RB = (Rigidbody)pointPrototipe.AddComponent(typeof(Rigidbody));
        pointP_RB.useGravity = gravity;
        pointP_RB.constraints = RigidbodyConstraints.FreezePositionZ;
        
        SphereCollider pointP_SC = (SphereCollider)pointPrototipe.AddComponent(typeof(SphereCollider));
        MeshRenderer pointP_MR = (MeshRenderer)pointPrototipe.AddComponent(typeof(MeshRenderer));
        
        if (debugMode)
        {
            MeshFilter pointP_MF = (MeshFilter)pointPrototipe.AddComponent(typeof(MeshFilter));
            pointP_MF.mesh = pointMesh;
        }


        //pointPrototipe.hideFlags = HideFlags.HideInHierarchy;
        pointPrototipe.SetActive(false);


        //Generate center
        centerPoint = Instantiate(pointPrototipe, gameObject.transform);
        centerPoint.SetActive(true);
        centerPoint.name = "center point";
        ((Rigidbody)centerPoint.GetComponent(typeof(Rigidbody))).mass = centerMass;
        //Generate sides

        generateRadial1Point();
        jointSpringsRadial1();

        generateRadia2Point();
        jointSpringsRadial2();

        addTexture();

    }

    // Update is called once per frame
    void Update()
    {
        refreshMesh();
    }
    //src https://stackoverflow.com/questions/25929820/how-to-calculate-position-on-a-circle-with-a-certain-angle
    private Vector3 getPosition(Vector3 center, float radius, float angle)
    {

        Vector3 p = new Vector3((float)(center.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad)),
        (float)(center.y + radius * Mathf.Sin(angle * Mathf.Deg2Rad)));

        return p;
    }


    private void generateRadial1Point()
    {

        radialpoints = new GameObject[numOfBorderPoints];
        float angle = 360 / radialpoints.Length;

        for (int i = 0; i < radialpoints.Length; i++)
        {
            GameObject currentpoint = Instantiate(pointPrototipe, getPosition(gameObject.transform.position, radius, i * angle), new Quaternion(0, 0, 0, 0), gameObject.transform);
            currentpoint.SetActive(true);
            currentpoint.name = "radial_" + i;
            ((Rigidbody)currentpoint.GetComponent(typeof(Rigidbody))).mass = borderMass;

            radialpoints[i] = currentpoint;

        }
    }
    private void jointSpringsRadial1()
    {

        for (int i = 0; i < radialpoints.Length - 1; i++)
        {
            unionSpringPoints(centerPoint, radialpoints[i], radialSpringsFoce);
            unionSpringPoints(radialpoints[i], radialpoints[i + 1], radialSpringsFoce);

        }

        unionSpringPoints(centerPoint, radialpoints[radialpoints.Length - 1], radialSpringsFoce);
        unionSpringPoints(radialpoints[radialpoints.Length - 1], radialpoints[0], borderSpringsForce);


    }
    private void generateRadia2Point()
    {
        radial2points = new GameObject[numOfBorderPoints];
        float angle = 360 / radialpoints.Length;

        for (int i = 0; i < radialpoints.Length; i++)
        {
            GameObject currentpoint = Instantiate(pointPrototipe, getPosition(gameObject.transform.position, radius + radius2, i * angle), new Quaternion(0, 0, 0, 0), gameObject.transform);
            currentpoint.SetActive(true);
            currentpoint.name = "radial_2_" + i;
            ((Rigidbody)currentpoint.GetComponent(typeof(Rigidbody))).mass = border2Mass;

            radial2points[i] = currentpoint;

        }
    }
    private void jointSpringsRadial2()
    {
        for (int i = 0; i < radial2points.Length - 1; i++)
        {
            unionSpringPoints(radial2points[i], radialpoints[i], radial2SpringsForce);
            unionSpringPoints(radial2points[i], radialpoints[i + 1], border2ToBorder1SpringForce);
            unionSpringPoints(radial2points[i], radial2points[i + 1], border2SpringsForce);



        }

        unionSpringPoints(radial2points[radial2points.Length - 1], radialpoints[radial2points.Length - 1], radial2SpringsForce);
        unionSpringPoints(radial2points[radial2points.Length - 1], radialpoints[0], border2ToBorder1SpringForce);
        unionSpringPoints(radial2points[radial2points.Length - 1], radial2points[0], border2SpringsForce);

    }

    private void unionSpringPoints(GameObject ob1, GameObject obj2, float springForce)
    {

        SpringJoint sj = ob1.AddComponent<SpringJoint>();
        sj.connectedBody = (Rigidbody)obj2.GetComponent(typeof(Rigidbody));
        sj.spring = springForce;
        if (debugMode)
        {
            Debug.DrawLine(ob1.transform.position, obj2.transform.position, Color.white, float.MaxValue);
        }

    }

    private Mesh generateMesh()
    {

        Mesh mesh = new Mesh();

        //num of point + the center point
        Vector3[] vertices = generateVertices();
        int[] tri = generateTriangles(vertices);

        mesh.vertices = vertices;
        mesh.triangles = tri;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void refreshMesh()
    {
        MF.mesh.vertices = generateVertices();
    }

    private Vector3[] generateVertices()
    {
        Vector3[] vertices = new Vector3[numOfBorderPoints + 1];

        vertices[0] = centerPoint.transform.localPosition;

        for (int i = 0; i < radial2points.Length; i++)
        {
            vertices[i + 1] = radial2points[i].transform.localPosition;
            if (debugMode)
            {
                Debug.Log(radial2points[i].name);
            }

        }
        return vertices;
    }
    private int[] generateTriangles(Vector3[] vertices)
    {
        int[] tri = new int[(radial2points.Length) * 3];
        int triIndex = 0;
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            tri[triIndex] = 0;
            tri[triIndex + 1] = i;
            tri[triIndex + 2] = i + 1;
            triIndex += 3;
        }
        //Add he last triangle
        tri[triIndex] = 0;
        tri[triIndex + 1] = vertices.Length - 1;
        tri[triIndex + 2] = 1;

        return tri;
    }

    private void addTexture()
    {
        MF = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        MF.mesh = generateMesh();

        if (debugMode)
        {
            foreach (Vector3 v3 in MF.mesh.vertices)
            {
                Debug.Log(v3);
            }
            foreach (int intengerMy in MF.mesh.triangles)
            {
                Debug.Log(intengerMy);
            }
        }

    }

}
