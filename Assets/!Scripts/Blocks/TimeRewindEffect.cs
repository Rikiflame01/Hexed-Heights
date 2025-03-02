using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindManager : MonoBehaviour
{
    public static TimeRewindManager Instance { get; private set; }

    [Header("Recording Settings")]

    public float recordDuration = 80f;

    public float recordInterval = 0.1f;

    public float rewindSpeed = 2f;

    public List<GameObject> objectsToRewind = new List<GameObject>();
    public struct Snapshot
    {
        public Vector3 position;
        public Quaternion rotation;

        public Snapshot(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }

    private Dictionary<GameObject, List<Snapshot>> snapshots = new Dictionary<GameObject, List<Snapshot>>();
    private int maxSnapshotsCount;
    private float recordTimer = 0f;
    private bool isRewinding = false;
    public bool IsRewinding 
    { 
        get { return isRewinding; } 
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        maxSnapshotsCount = Mathf.CeilToInt(recordDuration / recordInterval);

        foreach (GameObject obj in objectsToRewind)
        {
            if (obj != null)
                snapshots[obj] = new List<Snapshot>();
        }
    }

    private void OnEnable()
    {
        ActionManager.OnRewind += HandleRewindEvent;
    }

    private void OnDisable()
    {
        ActionManager.OnRewind -= HandleRewindEvent;
    }

    private void Update()
    {
        if (isRewinding)
            return;

        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            RecordSnapshots();
            recordTimer = 0f;
        }
    }
    private void RecordSnapshots()
    {
        foreach (GameObject obj in objectsToRewind)
        {
            if (obj == null)
                continue;


            if (!snapshots.ContainsKey(obj))
                snapshots[obj] = new List<Snapshot>();

            snapshots[obj].Add(new Snapshot(obj.transform.position, obj.transform.rotation));

            if (snapshots[obj].Count > maxSnapshotsCount)
            {
                snapshots[obj].RemoveAt(0);
            }
        }
    }
    private void HandleRewindEvent()
    {
        if (!isRewinding)
            StartCoroutine(RewindCoroutine());
    }
    private IEnumerator RewindCoroutine()
    {
        isRewinding = true;

        foreach (GameObject obj in objectsToRewind)
        {
            if (obj == null)
                continue;
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }

        bool anySnapshotsLeft = true;
        while (anySnapshotsLeft)
        {
            anySnapshotsLeft = false;

            foreach (GameObject obj in objectsToRewind)
            {
                if (obj == null || !snapshots.ContainsKey(obj))
                    continue;

                List<Snapshot> snapshotList = snapshots[obj];
                if (snapshotList.Count > 0)
                {
                    anySnapshotsLeft = true;
                    Snapshot targetSnapshot = snapshotList[snapshotList.Count - 1];

                    obj.transform.position = Vector3.Lerp(obj.transform.position, targetSnapshot.position, rewindSpeed * Time.deltaTime);
                    obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetSnapshot.rotation, rewindSpeed * Time.deltaTime);

                    if (Vector3.Distance(obj.transform.position, targetSnapshot.position) < 0.01f)
                    {
                        obj.transform.position = targetSnapshot.position;
                        obj.transform.rotation = targetSnapshot.rotation;
                        snapshotList.RemoveAt(snapshotList.Count - 1);
                    }
                }
            }
            yield return null;
        }

        foreach (GameObject obj in objectsToRewind)
        {
            if (obj == null)
                continue;
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }

        isRewinding = false;
        Debug.Log("Rewind end event invoked.");
    }

}
