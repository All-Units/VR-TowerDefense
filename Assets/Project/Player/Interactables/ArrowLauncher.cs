using UnityEngine;

public class ArrowLauncher : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The projectile that's created")]
    Arrow m_ProjectilePrefab = null;

    [SerializeField]
    [Tooltip("The point that the project is created")]
    Transform m_StartPoint = null;

    public void Fire()
    {
        var newObject = Instantiate(m_ProjectilePrefab, m_StartPoint.position, m_StartPoint.rotation, null);

        newObject.Fire();
    }
}