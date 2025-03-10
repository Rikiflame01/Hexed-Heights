using UnityEngine;

public class BlockusDeletus : MonoBehaviour
{
    public Material highlightMaterial; 
    public LayerMask blockLayer;      

    private GameObject highlightedBlock = null;
    private Material originalMaterial = null;

    void Update()
    {
        if (TurnManager.Instance.blockusDeletusIsActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, blockLayer))
            {
                GameObject hitBlock = hit.collider.gameObject;

                if (hitBlock != highlightedBlock)
                {
                    UnhighlightCurrentBlock();
                    HighlightBlock(hitBlock);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Destroy(hitBlock);
                    TurnManager.Instance.blockusDeletusIsActive = false;
                    highlightedBlock = null;
                    TurnManager.Instance.MarkTurnSuccessful();
                    TurnManager.Instance.ResetTurn();
                    TurnManager.Instance.HandleHex();
                }
            }
            else
            {
                UnhighlightCurrentBlock();
            }
        }
    }
    private void HighlightBlock(GameObject block)
    {
        highlightedBlock = block;
        Renderer renderer = block.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            renderer.material = highlightMaterial;
        }
        else
        {
            Debug.LogWarning("Block has no Renderer: " + block.name);
        }
    }
    private void UnhighlightCurrentBlock()
    {
        if (highlightedBlock != null)
        {
            Renderer renderer = highlightedBlock.GetComponentInChildren<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
            highlightedBlock = null;
            originalMaterial = null;
        }
    }
}