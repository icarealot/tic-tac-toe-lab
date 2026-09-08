#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using TicTacToeLab.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace TicTacToeLab.PlayModeTests
{
    public sealed class MainCameraWiringTests : SceneWiringTests
    {
        [UnityTest]
        public IEnumerator The_startup_factory_resolves_the_main_camera_role_for_camera_service()
        {
            yield return IE_LoadScene();

            IFactoryService factoryService = Object.FindFirstObjectByType<FactoryService>();
            IMainCamera created = factoryService.Get<IMainCamera>();

            Assert.That(created, Is.Not.Null);
            Assert.That(created, Is.TypeOf<MainCamera>());
            Assert.That(created.Camera, Is.Not.Null);

            Assert.That(created.Camera, Is.EqualTo(((Component)created).GetComponent<Camera>()));
            Assert.That(() => { _ = new CameraService(created.Camera); }, Throws.Nothing);

            factoryService.Return(created);
        }

        [UnityTest]
        public IEnumerator The_scene_starts_with_one_main_camera_behind_camera_main()
        {
            yield return IE_LoadScene();

            MainCamera[] startupMainCameras = Object.FindObjectsByType<MainCamera>(FindObjectsSortMode.None);

            Assert.That(startupMainCameras.Length, Is.EqualTo(1));
            Assert.That(Camera.main, Is.EqualTo(startupMainCameras[0].Camera));
        }
    }
}
#endif
