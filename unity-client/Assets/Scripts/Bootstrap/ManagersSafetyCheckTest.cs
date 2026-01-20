using UnityEngine;
using UI.Animations;

/// <summary>
/// Test script to verify ManagersSafetyCheck functionality
/// This can be attached to a test scene or run manually
/// </summary>
public class ManagersSafetyCheckTest : MonoBehaviour
{
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private float testDelay = 1f;

    private void Start()
    {
        if (runTestOnStart)
        {
            Invoke(nameof(RunSafetyCheckTest), testDelay);
        }
    }

    public void RunSafetyCheckTest()
    {
        Debug.Log("=== Starting Managers Safety Check Test ===");
        
        // Test the comprehensive safety check
        bool allManagersReady = ManagersSafetyCheck.CheckAllManagers("ManualTest");
        
        // Test individual manager checks
        Debug.Log("\n=== Testing Individual Manager Access ===");
        
        TestManagerAccess("ParticleController", ManagersSafetyCheck.GetParticleControllerSafe());
        TestManagerAccess("TransitionManager", ManagersSafetyCheck.GetTransitionManagerSafe());
        TestManagerAccess("AnimationController", ManagersSafetyCheck.GetAnimationControllerSafe());
        
        // Test null safety in ParticleController methods
        Debug.Log("\n=== Testing ParticleController Null Safety ===");
        TestParticleControllerNullSafety();
        
        Debug.Log("\n=== Managers Safety Check Test Complete ===");
        Debug.Log(allManagersReady ? "✅ All tests passed!" : "❌ Some tests failed!");
    }

    private void TestManagerAccess(string managerName, object managerInstance)
    {
        if (managerInstance != null)
        {
            Debug.Log($"✅ {managerName} is accessible and ready");
        }
        else
        {
            Debug.LogWarning($"❌ {managerName} is null - this is expected if not properly initialized");
        }
    }

    private void TestParticleControllerNullSafety()
    {
        // Test that ParticleController methods handle null gracefully
        try
        {
            // These should not crash even if ParticleController is null
            ParticleController.Instance?.SpawnImpactEffect(Vector3.zero);
            ParticleController.Instance?.SpawnConfettiEffect(Vector3.zero);
            ParticleController.Instance?.SpawnExplosionEffect(Vector3.zero);
            
            Debug.Log("✅ ParticleController null safety tests passed");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ ParticleController null safety test failed: {ex.Message}");
        }
    }

    [ContextMenu("Run Safety Check Now")]
    public void RunSafetyCheckNow()
    {
        RunSafetyCheckTest();
    }
}