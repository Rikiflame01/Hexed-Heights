using System;
using System.Collections;
using UnityEngine;

public class HexSpells : MonoBehaviour
{
    public Material frozenMaterial;
    public Material normalMaterial;

    public static HexSpells Instance { get; private set; }

    private Coroutine sneezeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void FreezeBlock()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Untouched");
        if (blocks.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, blocks.Length);
            GameObject block = blocks[index];

            block.tag = "Frozen";
            Rigidbody rb = block.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            Renderer rend = block.GetComponentInChildren<Renderer>();
            if (rend != null && frozenMaterial != null)
            {
                normalMaterial = rend.material;
                rend.material = frozenMaterial;
            }

            Debug.Log($"Freeze Block hex applied to {block.name}");
        }
        else
        {
            Debug.Log("No blocks available to freeze.");
        }
    }

    public void UnfreezeBlock()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Frozen");
        if (blocks.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, blocks.Length);
            GameObject block = blocks[index];

            block.tag = "Untouched";
            Rigidbody rb = block.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.None;
            }

            Renderer rend = block.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                rend.material = normalMaterial;
            }

            Debug.Log($"Unfreeze Block hex applied to {block.name}");
        }
        else
        {
            Debug.Log("No blocks available to unfreeze.");
        }
    }

    public void Sneeze()
    {
        if (sneezeCoroutine == null)
        {
            sneezeCoroutine = StartCoroutine(SneezeCoroutine());
        }
        if (TurnManager.Instance.sneezeHexIsActive){
            StartCoroutine(SneezeCoroutine());
        }
    }

    private IEnumerator SneezeCoroutine()
    {
        while (TurnManager.Instance.sneezeHexIsActive)
        {
            int randomSeconds = UnityEngine.Random.Range(3, 7);
            yield return new WaitForSeconds(randomSeconds);
            ActionManager.InvokeSneeze();
        }
        StopCoroutine(sneezeCoroutine);
    }
}