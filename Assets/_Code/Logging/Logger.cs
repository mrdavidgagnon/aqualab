using UnityEngine;

namespace Logging
{
    public class Logger : MonoBehaviour
    {
        private void Awake()
        {
            SimpleLog slog = new SimpleLog("AQUALAB", 0);
        }
    }
}
