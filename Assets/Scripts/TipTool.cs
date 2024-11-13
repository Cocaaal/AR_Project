using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

public class Sphere
{
    public Vector3 center;
    public float radius;

    public Sphere(Vector3 tipPosition, float radius)
    {
        this.center = tipPosition;
        this.radius = radius;
    }
}

public class TipTool : MonoBehaviour
{

    private bool found = false;
    private bool calibrated = false;

    public int totalNumberOfPoses;
    public float timeBetweenTwoPoses;

    public GameObject axisPrefab;

    public GameObject tip;

    private int currentNumberOfPoses = 0;

    private float nextPoseTime;

    private List<Vector3> positions = new List<Vector3>();
    private List<GameObject> axisObjects = new List<GameObject>();

    public GameObject hoveredDisk = null;
    public GameObject hoveredPillar = null;

    private LineRenderer lineRenderer;

    private bool selectionModeRaycast = true;

    public bool diskSelected = false;

    private Pillar originPillar;

    // Start is called before the first frame update
    void Start()
    {
        nextPoseTime = Time.time;
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!found) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !diskSelected)
        {
            selectionModeRaycast = !selectionModeRaycast;
            Debug.Log("Selection mode changed : " + (selectionModeRaycast ? "RAYCAST" : "TIP"));
            tip.SetActive(!selectionModeRaycast);
            lineRenderer.enabled = selectionModeRaycast;
        }

        if (calibrated)
        {

            if (selectionModeRaycast && !diskSelected)
            {
                RaycastHit hit = new RaycastHit();
                bool isHit = Physics.Raycast(new Ray(transform.position, tip.transform.position - transform.position), out hit);
                Vector3 lineRendererEndPoint = transform.position + (tip.transform.position - transform.position) * 10;
                if (isHit)
                {
                    lineRendererEndPoint = hit.point;
                }
                lineRenderer.SetPositions(new Vector3[] { transform.position, lineRendererEndPoint});
                //Debug.DrawRay(transform.position, (tip.transform.position - transform.position) * 10, Color.green, 0.1f, false);
                if (isHit && hit.collider.tag == "Disk")
                {
                    GameObject currentHoveredDisk = hit.collider.gameObject;
                    if (currentHoveredDisk != hoveredDisk)
                    {
                        currentHoveredDisk.GetComponent<Disk>().hover(true);
                        if (hoveredDisk != null)
                        {
                            hoveredDisk.GetComponent<Disk>().hover(false);
                        }
                        hoveredDisk = currentHoveredDisk;
                    }
                }
                else
                {
                    if (hoveredDisk != null)
                    {
                        hoveredDisk.GetComponent<Disk>().hover(false);
                        hoveredDisk = null;
                    }
                }
            }

        }else
        {
            if (currentNumberOfPoses < totalNumberOfPoses)
            {
                if (Time.time > nextPoseTime)
                {
                    GetPose();
                }
            }
            else
            {
                List<Vector3> inliers = Ransac();
                Sphere bestSphere = SphereFitting(inliers);

                tip.transform.position = bestSphere.center;

                Debug.Log("RADIUS : " + bestSphere.radius);

                foreach (GameObject go in axisObjects)
                {
                    Destroy(go);
                }
                calibrated = true;
                lineRenderer.enabled = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (diskSelected)
            {
                if (hoveredPillar != null)
                {
                    diskSelected = false;
                    Pillar hoveredPillarComponent = hoveredPillar.GetComponent<Pillar>();
                    if (int.Parse(hoveredDisk.name) < hoveredPillarComponent.smallestDisk())
                    {
                        hoveredPillarComponent.place(hoveredDisk);
                    }
                    else
                    {
                        originPillar.place(hoveredDisk);
                        originPillar.hover(false);
                    }
                    hoveredPillarComponent.hover(false);
                    hoveredPillar = null;
                    originPillar = null;
                }

            }else
            {
                if (hoveredDisk != null)
                {
                    originPillar = hoveredDisk.transform.parent.GetComponent<Pillar>();
                    if (originPillar.smallestDisk() == int.Parse(hoveredDisk.name))
                    {
                        tip.SetActive(false);
                        diskSelected = true;
                        originPillar.remove();
                        hoveredDisk.transform.SetParent(this.transform);
                    }else
                    {
                        originPillar = null;
                    }
                }
            }
        }
    }

    public void GetPose()
    {
        nextPoseTime = Time.time + timeBetweenTwoPoses;
        positions.Add(transform.position);
        axisObjects.Add(Instantiate(axisPrefab, transform.position, transform.rotation));
        currentNumberOfPoses++;
    }

    public void OnTargetFound()
    {
        found = true;
    }
    public void OnTargetLost()
    {
        found = false;
    }

    public void setHoveredDisk(GameObject hoveredDisk)
    {
        if (this.hoveredDisk != null)
        {
            this.hoveredDisk.GetComponent<Disk>().hover(false);
        }
        this.hoveredDisk = hoveredDisk;
        if (hoveredDisk != null)
        {
            hoveredDisk.GetComponent<Disk>().hover(true);
        }
    }

    public void setHoveredPillar(GameObject hoveredPillar)
    {
        if (this.hoveredPillar != null)
        {
            this.hoveredPillar.GetComponent<Pillar>().hover(false);
        }
        this.hoveredPillar = hoveredPillar;
        if (hoveredPillar != null)
        {
            hoveredPillar.GetComponent<Pillar>().hover(true);
        }
    }

    public Sphere SphereFitting(List<Vector3> liste)
    {
        int nbPoints = liste.Count;

        double[,] a = new double[nbPoints, 4];
        double[] f = new double[nbPoints];

        for (var i = 0; i < nbPoints; i++)
        {
            Vector3 pose = liste[i];

            a[i, 0] = pose.x * 2;
            a[i, 1] = pose.y * 2;
            a[i, 2] = pose.z * 2;
            a[i, 3] = 1.0f;
            f[i] = (pose.x * pose.x) + (pose.y * pose.y) + (pose.z * pose.z);
        }

        Matrix<double> aMatrix = Matrix<double>.Build.DenseOfArray(a);
        Vector<double> fVector = Vector<double>.Build.DenseOfArray(f);

        Vector<double> cVector = MultipleRegression.NormalEquations(aMatrix, fVector);
        
        // Convert from MathNet.LinearAlgebra.Vector to UnityEngine.Vector3
        Vector3 tipPosition = new Vector3((float)cVector[0], (float)cVector[1], (float)cVector[2]);

        double t = (cVector[0] * cVector[0]) + (cVector[1] * cVector[1]) + (cVector[2] * cVector[2]) + cVector[3];
        float radius = (float)System.Math.Sqrt(t);

        Sphere sphere = new Sphere(tipPosition, radius);
        return sphere;
    }

    public List<Vector3> Ransac()
    {
        float threshold = 0.01f;
        float bestScore = Mathf.Infinity;
        List<Vector3> bestInliers = new List<Vector3>();

        int nbIteration = 30;
        for (int i = 0; i < nbIteration; i++)
        {

            // Shuffle the list and take the 4 first points to get 4 random points
            List<Vector3> sample = new List<Vector3>(positions);
            for (int j=0; j<totalNumberOfPoses; j++)
            {
                int newIndex = Random.Range(0, totalNumberOfPoses);
                var tmp = sample[j];
                sample[j] = sample[newIndex];
                sample[newIndex] = tmp;
            }
            sample = sample.GetRange(0, 4);

            float score = 0;
            List<Vector3> inliers = new List<Vector3>();

            // fit sphere to those 4 points and get center and radius
            Sphere sphere = SphereFitting(sample);

            for (int n = 0; n < totalNumberOfPoses; n++)
            {
                var p = positions[n];
                // distance to sphere
                var currentError = System.Math.Abs(Vector3.Distance(sphere.center, p) - sphere.radius);

                if (currentError < threshold)
                {
                    score += currentError;
                    inliers.Add(p);
                }
                else
                {
                    score += threshold;
                }
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestInliers = inliers;
            }
        }

        return bestInliers;
    }
}
