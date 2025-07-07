using UnityEngine;

/// <summary>
/// Helper class per gestire i collider degli oggetti droppati
/// Crea un wrapper GameObject per evitare conflitti con i MeshCollider concavi
/// </summary>
public static class ColliderHelper
{
    /// <summary>
    /// Crea un wrapper GameObject che contiene il prefab come figlio
    /// Questo evita completamente i problemi con i MeshCollider concavi
    /// </summary>
    public static GameObject CreateDroppedObjectWrapper(GameObject prefab, Item itemData, Vector3 position)
    {
        Debug.Log($"=== CREATING WRAPPER FOR: {prefab.name} ===");
        
        // Crea un GameObject wrapper vuoto
        GameObject wrapper = new GameObject($"{prefab.name}_Dropped");
        wrapper.transform.position = position;
        wrapper.transform.rotation = Quaternion.identity;
        
        // Istanzia il prefab come figlio del wrapper
        GameObject visualModel = Object.Instantiate(prefab, wrapper.transform);
        visualModel.name = $"{prefab.name}_Visual";
        visualModel.transform.localPosition = Vector3.zero;
        visualModel.transform.localRotation = Quaternion.identity;
        
        // DISABILITA TUTTI I COLLIDER NEL MODELLO VISIVO
        DisableAllCollidersInModel(visualModel);
        
        // Aggiungi collider semplici al wrapper
        AddWrapperColliders(wrapper, visualModel);
        
        // Setup fisica sul wrapper
        SetupWrapperPhysics(wrapper);
        
        // Setup componente collectable sul wrapper
        SetupCollectableComponent(wrapper, itemData);
        
        Debug.Log($"=== WRAPPER CREATED: {wrapper.name} ===");
        return wrapper;
    }
    
    /// <summary>
    /// Disabilita completamente tutti i collider nel modello visivo
    /// </summary>
    private static void DisableAllCollidersInModel(GameObject model)
    {
        Collider[] allColliders = model.GetComponentsInChildren<Collider>(true);
        
        foreach (Collider col in allColliders)
        {
            col.enabled = false;
            Debug.Log($"Disabled {col.GetType().Name} on {col.gameObject.name}");
        }
        
        // Rimuovi tutti i Rigidbody dal modello se presenti
        Rigidbody[] allRigidbodies = model.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in allRigidbodies)
        {
            Object.DestroyImmediate(rb);
            Debug.Log($"Removed Rigidbody from {rb.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Aggiunge collider semplici al wrapper
    /// </summary>
    private static void AddWrapperColliders(GameObject wrapper, GameObject model)
    {
        // Calcola bounds dal modello visivo
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"No renderers found in {model.name}, using default collider size");
            AddDefaultColliders(wrapper);
            return;
        }
        
        // Calcola bounds combinati
        Bounds combinedBounds = GetCombinedBounds(renderers);
        
        // Aggiungi collider principale per la fisica
        AddMainPhysicsCollider(wrapper, combinedBounds);
        
        // Aggiungi trigger per la collezione
        AddCollectionTrigger(wrapper, combinedBounds);
    }
    
    /// <summary>
    /// Calcola i bounds combinati di tutti i renderer
    /// </summary>
    private static Bounds GetCombinedBounds(Renderer[] renderers)
    {
        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;
        
        foreach (Renderer renderer in renderers)
        {
            if (!boundsInitialized)
            {
                combinedBounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }
        
        return combinedBounds;
    }
    
    /// <summary>
    /// Aggiunge collider di default se non ci sono renderer
    /// </summary>
    private static void AddDefaultColliders(GameObject wrapper)
    {
        // Collider principale
        BoxCollider mainCollider = wrapper.AddComponent<BoxCollider>();
        mainCollider.size = Vector3.one;
        mainCollider.isTrigger = false;
        
        // Trigger per collezione
        SphereCollider triggerCollider = wrapper.AddComponent<SphereCollider>();
        triggerCollider.radius = 1.5f;
        triggerCollider.isTrigger = true;
        
        Debug.Log($"Added default colliders to {wrapper.name}");
    }
    
    /// <summary>
    /// Aggiunge il collider principale per la fisica
    /// </summary>
    private static void AddMainPhysicsCollider(GameObject wrapper, Bounds bounds)
    {
        // Determina il tipo di collider migliore
        Vector3 size = bounds.size;
        Vector3 localCenter = bounds.center - wrapper.transform.position;
        
        if (size.y > size.x && size.y > size.z && size.y > 1.5f)
        {
            // Oggetto alto: usa CapsuleCollider (perfetto per barili)
            CapsuleCollider capsuleCol = wrapper.AddComponent<CapsuleCollider>();
            capsuleCol.center = localCenter;
            capsuleCol.height = size.y;
            capsuleCol.radius = Mathf.Max(size.x, size.z) * 0.4f;
            capsuleCol.isTrigger = false;
            
            Debug.Log($"Added CapsuleCollider to {wrapper.name} - Height: {size.y}, Radius: {capsuleCol.radius}");
        }
        else
        {
            // Default: BoxCollider
            BoxCollider boxCol = wrapper.AddComponent<BoxCollider>();
            boxCol.center = localCenter;
            boxCol.size = size * 0.9f; // Leggermente più piccolo
            boxCol.isTrigger = false;
            
            Debug.Log($"Added BoxCollider to {wrapper.name} - Size: {boxCol.size}");
        }
    }
    
    /// <summary>
    /// Aggiunge il trigger per la collezione
    /// </summary>
    private static void AddCollectionTrigger(GameObject wrapper, Bounds bounds)
    {
        SphereCollider triggerCol = wrapper.AddComponent<SphereCollider>();
        triggerCol.center = bounds.center - wrapper.transform.position;
        triggerCol.radius = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.6f;
        triggerCol.isTrigger = true;
        
        Debug.Log($"Added collection trigger to {wrapper.name} - Radius: {triggerCol.radius}");
    }
    
    /// <summary>
    /// Setup delle proprietà fisiche del wrapper
    /// </summary>
    private static void SetupWrapperPhysics(GameObject wrapper)
    {
        Rigidbody rb = wrapper.AddComponent<Rigidbody>();
        
        // Configura proprietà del rigidbody
        rb.mass = 1f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        rb.useGravity = true;
        
        // Applica forza iniziale di drop
        Vector3 randomForce = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(0.2f, 0.8f),
            Random.Range(-0.3f, 0.3f)
        );
        rb.AddForce(randomForce, ForceMode.Impulse);
        
        Debug.Log($"Setup physics for {wrapper.name} - Mass: {rb.mass}");
    }
    
    /// <summary>
    /// Setup del componente CollectableItem
    /// </summary>
    private static void SetupCollectableComponent(GameObject wrapper, Item itemData)
    {
        CollectableItem collectable = wrapper.AddComponent<CollectableItem>();
        collectable.itemData = itemData;
        collectable.collectionRange = 2f;
        collectable.requireInteraction = false;
        
        // Disabilita temporaneamente per prevenire pickup immediato
        collectable.enabled = false;
        
        Debug.Log($"Setup CollectableItem for {itemData.itemName}");
    }
}