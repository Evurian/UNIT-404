using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Unit404.Integrante4.Tests
{
    // Stand-in for Integrante 1's CameraFollow: the guard matches by type name because my assembly cannot reference theirs.
    public class CameraFollow : MonoBehaviour { }

    public class CameraYieldTests
    {
        static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);

        static void LateUpdateOf(Component c) => c.GetType().GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(c, null);

        [TestCase("SideCameraFollow")]
        [TestCase("ThirdPersonCamera")]
        public void PlaceholderCamera_StaysEnabled_WhenNoRealCamera(string typeName)
        {
            var go = new GameObject("cam");
            var cam = (Behaviour)go.AddComponent(Find(typeName));
            LateUpdateOf(cam);
            Assert.IsTrue(cam.enabled);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [TestCase("SideCameraFollow")]
        [TestCase("ThirdPersonCamera")]
        public void PlaceholderCamera_Yields_WhenRealCameraFollowIsPresent(string typeName)
        {
            var go = new GameObject("cam");
            go.AddComponent<CameraFollow>();
            var cam = (Behaviour)go.AddComponent(Find(typeName));
            LateUpdateOf(cam);
            Assert.IsFalse(cam.enabled, typeName + " must not fight CameraFollow");
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
