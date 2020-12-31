using System;
using System.Collections;
using Extend.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extend.Asset.AssetOperator
{
    public class AsyncABSceneOperator : AssetOperatorBase
    {
        private readonly int assetBundleHash;
        public AsyncABSceneOperator(int abHash) {
            assetBundleHash = abHash;
        }
        public override void Execute(AssetAsyncLoadHandle handle, Type typ) {
            if( !( handle.Container.TryGetAsset(assetBundleHash) is AssetBundleInstance abInstance ) || 
                abInstance.Status != AssetRefObject.AssetStatus.DONE ) {
                throw new Exception("Asset depend asset bundle not loaded");
            }

            string[] scenePaths = abInstance.AB.GetAllScenePaths();
            var service = CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
            service.StartCoroutine(LoadAbAsyncScene(scenePaths[0]));
        }

        private IEnumerator LoadAbAsyncScene(string path)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(path);
            asyncOperation.completed += _ => { };
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
        }
    }
}