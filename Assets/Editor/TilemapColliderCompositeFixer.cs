using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapColliderCompositeFixer
{
    [MenuItem("Tools/Physics2D/Fix Ground Tilemap Colliders")]
    private static void FixGroundTilemapColliders()
    {
        int fixedCount = 0;
        TilemapCollider2D[] colliders = Object.FindObjectsOfType<TilemapCollider2D>(true);

        foreach (TilemapCollider2D tilemapCollider in colliders)
        {
            GameObject tilemapObject = tilemapCollider.gameObject;

            if (tilemapObject.layer != LayerMask.NameToLayer("Ground") || tilemapCollider.isTrigger)
                continue;

            Undo.RecordObject(tilemapCollider, "Fix Tilemap Collider");
            tilemapCollider.usedByComposite = true;

            Rigidbody2D rigidbody = tilemapObject.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = Undo.AddComponent<Rigidbody2D>(tilemapObject);
            }

            Undo.RecordObject(rigidbody, "Fix Tilemap Rigidbody");
            rigidbody.bodyType = RigidbodyType2D.Static;

            if (tilemapObject.GetComponent<CompositeCollider2D>() == null)
            {
                Undo.AddComponent<CompositeCollider2D>(tilemapObject);
            }

            EditorUtility.SetDirty(tilemapObject);
            fixedCount++;
        }

        Debug.Log($"Fixed {fixedCount} ground TilemapCollider2D component(s).");
    }
}
